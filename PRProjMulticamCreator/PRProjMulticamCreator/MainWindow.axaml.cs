using System.Collections.Generic;
using System.IO;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PRProjMulticamCreator.Models;
using PRProjMulticamCreator.Services;

namespace PRProjMulticamCreator;

public partial class MainWindow : Window
{
    private const string DecompressedPath = "UntitledDecompressed.prproj";
    private const string TempFilePath = "UntitledTemp.prproj";
    private const string CompressedFilePath = "Result.prproj";
    private string PrprojPath { get; set; }
    private long StartTick { get; }
    private long EndTick { get; }
    private List<FrameModel> ListOfFrames { get; }

    public MainWindow()
    {
        InitializeComponent();
        StartButton.IsEnabled = false;

        // init data
        StartTick = 0;
        EndTick = 151891407360000;
        ListOfFrames = new List<FrameModel>()
        {
            new()
            {
                InPoint = StartTick,
                OutPoint = EndTick/4 * 1,
                TrackIndex = 0
            },
            new()
            {
                InPoint = EndTick/4 * 1,
                OutPoint = EndTick/4 * 2,
                TrackIndex = 1
            },
            new()
            {
                InPoint = EndTick/4 * 2,
                OutPoint = EndTick/4 * 3,
                TrackIndex = 0
            },
            new()
            {
                InPoint = EndTick/4 * 3,
                OutPoint = EndTick,
                TrackIndex = 2
            }
        };
    }

    private async void SelectProjFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Выберите файл"
        };
        dlg.Filters?.Add(new FileDialogFilter() { Name = "Файлы Adobe Premier Pro", Extensions = { "prproj" } });
        var result = await dlg.ShowAsync(this);
        if (result != null)
        {
            SelectedFile.Text = Path.GetFileName(result[0]);
            PrprojPath = result[0];
            StartButton.IsEnabled = true;
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        ProgressBar.IsVisible = true;

        // decompress prproj and load it
        await GzipService.DecompressGZip(PrprojPath, DecompressedPath);
        var prproj = new XmlDocument();
        prproj.Load(DecompressedPath);

        // Проанализировать аудиодорожки и определить активного участника
        // string audioPath1 = GetAudioPath(audioTrack1);
        // string audioPath2 = GetAudioPath(audioTrack2);
        // List<int> activeSpeakers = AnalyzeAudioTracks(audioPath1, audioPath2);

        // update multicam track
        UpdateMulticamTrack(prproj);

        // save prproj
        prproj.Save(TempFilePath);
        await GzipService.CompressGZip(CompressedFilePath, TempFilePath);

        // File.Delete(decompressedPath);
        // File.Delete(tempFilePath);
        Result.Text = "Готово!";
        ProgressBar.IsVisible = false;
    }

    private void UpdateMulticamTrack(XmlDocument prproj)
    {
        // get original nodes for cloning
        var originalVideoClipNode = prproj.SelectSingleNode("(//VideoClip[Clip/IsMulticam='true'])[1]");
        var originalVideoClipObjectId = originalVideoClipNode?.Attributes?["ObjectID"]?.Value;

        var originalSubClipNode = prproj.SelectSingleNode($"//SubClip[Clip[@ObjectRef='{originalVideoClipObjectId}']]");
        var originalSubClipObjectId = originalSubClipNode?.Attributes?["ObjectID"]?.Value;

        var originalVideoClipTrackItemNode = prproj.SelectSingleNode($"//VideoClipTrackItem[ClipTrackItem[SubClip[@ObjectRef='{originalSubClipObjectId}']]]");
        var originalVideoClipTrackItemObjectId = originalVideoClipTrackItemNode?.Attributes?["ObjectID"]?.Value;
        var originalComponentsNode = originalVideoClipTrackItemNode?.SelectSingleNode(".//Components");
        var originalComponentsNodeObjectId = originalComponentsNode?.Attributes?["ObjectRef"]?.Value;

        var originalVideoComponentChainNode = prproj.SelectSingleNode($"//VideoComponentChain[@ObjectID='{originalComponentsNodeObjectId}']");

        var originalTrackItemNode = prproj.SelectSingleNode($"//TrackItem[@ObjectRef='{originalVideoClipTrackItemObjectId}']");

        // start cloning
        var i = 0;
        foreach (var frame in ListOfFrames)
        {
            var videoClipObjectId = 10000 + i;
            var subClipObjectId = 20000 + i;
            var videoComponentChainObjectId = 30000 + i;
            var videoClipTrackItemObjectId = 40000 + i;

            NodeService.CloneVideoClipNode(
                originalVideoClipNode,
                videoClipObjectId,
                frame.InPoint,
                frame.OutPoint,
                frame.TrackIndex);

            NodeService.CloneSubClipNode(originalSubClipNode, subClipObjectId, videoClipObjectId);

            NodeService.CloneVideoComponentChainNode(originalVideoComponentChainNode, videoComponentChainObjectId);

            NodeService.CloneVideoClipTrackItemNode(
                originalVideoClipTrackItemNode,
                videoClipTrackItemObjectId,
                videoComponentChainObjectId,
                subClipObjectId,
                frame.InPoint,
                frame.OutPoint);
            NodeService.CloneTrackItemNode(originalTrackItemNode, i, videoClipTrackItemObjectId);

            i += 1;
        }

        // remove original nodes
        originalVideoClipNode?.ParentNode?.RemoveChild(originalVideoClipNode);
        originalSubClipNode?.ParentNode?.RemoveChild(originalSubClipNode);
        originalVideoClipTrackItemNode?.ParentNode?.RemoveChild(originalVideoClipTrackItemNode);
        originalTrackItemNode?.ParentNode?.RemoveChild(originalTrackItemNode);
    }
}