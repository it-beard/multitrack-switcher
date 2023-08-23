using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PRProjMulticamCreator.Core;
using PRProjMulticamCreator.Core.Extensions;
using PRProjMulticamCreator.Models;
using PRProjMulticamCreator.Services;

namespace PRProjMulticamCreator;

public partial class MainWindow : Window
{
    private string PrprojPath { get; set; }
    private string Speaker1Wav { get; set; }
    private string Speaker2Wav { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        StartButton.IsEnabled = false;
        FirstSpeakerSensitivity.Text = "0.055";
        SecondSpeakerSensitivity.Text = "0.065";
        NoisyFrameDuration.Text = "2000";
        DiluteIterations.Value = 3;
        DiluteFrameDuration.Value = 4;
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        ProgressBar.IsVisible = true;
        var vm = DataContext as MainWindowViewModel;

        // load XML
        var gzipService = new GzipService();
        var prproj = gzipService.LoadXml(PrprojPath);

        // save files (for Debugging)
        // await gzipService.DecompressAndSave(PrprojPath, Constants.DecompressedPath);
        // prproj.Load(Constants.DecompressedPath);

        // prepare frames (primary, secondary, secondarySynthetic)
        var frameService = new FrameService();
        var primaryFrames = frameService.GetListOfPureWavFrames(
            Speaker1Wav,
            FirstSpeakerSensitivity.Text.ToDouble(),
            Constants.PremierProPrimaryTrackIndex); // get real frames from primary Track
        primaryFrames = frameService.RemoveNoiseFrames(primaryFrames, NoisyFrameDuration.Text.ToInt());
        var secondarySyntheticFrames = frameService.GetSyntheticFrames(primaryFrames); // create synthetic frames for secondary Track

        var secondaryFrames = frameService.GetListOfPureWavFrames(
            Speaker2Wav,
            SecondSpeakerSensitivity.Text.ToDouble(),
            Constants.PremierProSecondaryTrackIndex); // get real frames from secondary Track
        secondaryFrames = frameService.RemoveNoiseFrames(secondaryFrames, NoisyFrameDuration.Text.ToInt());

        // prepare fully frames list (based on primary and secondarySynthetic frames)
        var allFrames = new List<FrameModel>();
        allFrames.AddRange(primaryFrames);
        allFrames.AddRange(secondarySyntheticFrames);
        allFrames = allFrames.OrderBy(a => a.InPoint).ToList(); // sort frames by InPoint

        if(vm.IsThreeCameraMode)
        {
            var overlappingFrames = frameService.GetOverlappingFrames(primaryFrames, secondaryFrames);
            overlappingFrames = frameService.RemoveNoiseFrames(overlappingFrames, NoisyFrameDuration.Text.ToInt());
            allFrames = frameService.AddFramesToAllFrames(overlappingFrames, allFrames); //merge overlapping frames with all frames
        }

        // mix short secondary frames to allFrames
        var secondaryShortFrames = frameService.RemoveLongFrames(secondaryFrames, NoisyFrameDuration.Text.ToInt());
        var allWithSecondaryShortFrames = frameService.AddFramesToAllFrames(secondaryShortFrames, allFrames); //merge short secondary frames with all frames

        // start dilute long frames
        var result = new List<FrameModel>();
        if (DiluteIterations.Value > 0)
        {
            for (int i = 0; i < DiluteIterations.Value; i++)
            {
                result = frameService.DiluteLongFrames(allWithSecondaryShortFrames, vm.DiluteModeValue, DiluteFrameDuration.Value); // dilute long frames
            }
        }
        else
        {
            result = allWithSecondaryShortFrames;
        }

        // update multicam track
        var nodeService = new NodeService();
        nodeService.UpdateMulticamTrack(prproj, result);

        Result.Text = "Done!";
        ProgressBar.IsVisible = false;
        await ShowSaveFileDialog(prproj);

        // save files (for Debugging)
        // prproj.Save(Constants.TempFilePath);
        // gzipService.CompressAndSave(Constants.CompressedFilePath, Constants.TempFilePath);
    }

    private async Task ShowSaveFileDialog(XmlDocument xmlDoc)
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Title = "Save .prproj File",
            DefaultExtension = "prproj",
            Filters = new List<FileDialogFilter>
            {
                new FileDialogFilter { Name = "Premiere Pro Project", Extensions = new List<string> { "prproj" } }
            }
        };

        var result = await saveFileDialog.ShowAsync(this); // 'this' refers to your window or control

        if (!string.IsNullOrEmpty(result))
        {
            await using FileStream fs = File.Create(result);
            await using GZipStream gzipStream = new GZipStream(fs, CompressionLevel.Optimal);
            await using XmlTextWriter xmlWriter = new XmlTextWriter(gzipStream, Encoding.UTF8);
            xmlDoc.Save(xmlWriter);
        }
    }

    private async void SelectPrprojFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select .prproj file"
        };
        dlg.Filters?.Add(new FileDialogFilter() { Name = "Adobe Premier Pro project file", Extensions = { "prproj" } });
        var result = await dlg.ShowAsync(this);
        if (result != null)
        {
            SelectedPrprojFile.Text = Path.GetFileName(result[0]);
            PrprojPath = result[0];
            CheckStartButtonEnabled();
        }
    }

    private async void SelectSpeaker1WavButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select .wav file for first speaker"
        };
        dlg.Filters?.Add(new FileDialogFilter() { Name = ".wav", Extensions = { "wav" } });
        var result = await dlg.ShowAsync(this);
        if (result != null)
        {
            SelectedSpeaker1WavFile.Text = Path.GetFileName(result[0]);
            Speaker1Wav = result[0];
            CheckStartButtonEnabled();
        }
    }
    private async void SelectSpeaker2WavButton_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Select .wav file for second speaker"
        };
        dlg.Filters?.Add(new FileDialogFilter() { Name = ".wav", Extensions = { "wav" } });
        var result = await dlg.ShowAsync(this);
        if (result != null)
        {
            SelectedSpeaker2WavFile.Text = Path.GetFileName(result[0]);
            Speaker2Wav = result[0];
            CheckStartButtonEnabled();
        }
    }

    private void DiluteIterationsUpDown_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == NumericUpDown.ValueProperty)
        {
            if (e.NewValue != null && (double)e.NewValue > 0)
            {
                DiluteFramesDurationPanel.IsEnabled = true;
                DiluteModeBox.IsEnabled = true;
            }
            else
            {
                DiluteFramesDurationPanel.IsEnabled = false;
                DiluteModeBox.IsEnabled = false;
            }
        }
    }

    private void IsTwoCameraMode_On(object sender, RoutedEventArgs e)
    {
        DiluteModeBox.SelectedIndex = (int)DiluteMode.TwoCameras;
    }

    private void CheckStartButtonEnabled()
    {
        if (!string.IsNullOrEmpty(PrprojPath) && !string.IsNullOrEmpty(Speaker1Wav) && !string.IsNullOrEmpty(Speaker2Wav))
        {
            StartButton.IsEnabled = true;
        }
        else
        {
            StartButton.IsEnabled = false;
        }
    }
}