using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using NAudio.Wave;
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
    private string Speaker1Wav { get; set; }
    private string Speaker2Wav { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        StartButton.IsEnabled = false;
        FirstSpeakerSensitivity.Text = "0.075";
        SecondSpeakerSensitivity.Text = "0.065";
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

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        ProgressBar.IsVisible = true;

        // decompress prproj and load it
        await GzipService.DecompressGZip(PrprojPath, DecompressedPath);
        var prproj = new XmlDocument();
        prproj.Load(DecompressedPath);

        var firstPersonFrames = CreateListOfFramesFromWavFile(
            Speaker1Wav,
            double.Parse(FirstSpeakerSensitivity.Text, NumberStyles.Float, CultureInfo.InvariantCulture),
            1000,
            1,
            0);
        var secondPersonFrames = CreateListOfFramesFromWavFile(
            Speaker2Wav,
            double.Parse(SecondSpeakerSensitivity.Text, NumberStyles.Float, CultureInfo.InvariantCulture),
            1000,
            0,
            1);
        var normalizedFirstSpeakerFrames = ExtendAndMergeShortFrames(
            RemoveShortFrames(firstPersonFrames, 1500),
            1000);
        var normalizedSecondSpeakerFrames = ExtendAndMergeShortFrames(
            RemoveShortFrames(secondPersonFrames, 1500),
            2000);
        var mergedFrames = MergeFrameLists(normalizedFirstSpeakerFrames, normalizedSecondSpeakerFrames);
        var mergedSimilarFrames = MergeConsecutiveFramesWithSamePriority(mergedFrames);

        // update multicam track
        UpdateMulticamTrack(prproj, mergedSimilarFrames);

        // save prproj
        prproj.Save(TempFilePath);
        await GzipService.CompressGZip(CompressedFilePath, TempFilePath);

        // File.Delete(decompressedPath);
        // File.Delete(tempFilePath);
        Result.Text = "Done! Find your Result.prproj in folder of this app :)";
        ProgressBar.IsVisible = false;
    }

    private void UpdateMulticamTrack(XmlDocument prproj, List<FrameModel> frames)
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
        foreach (var frame in frames)
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

    public List<FrameModel> CreateListOfFramesFromWavFile(string wavFilePath, double threshold, int minimumSilenceDuration, int priority, int trackIndex)
    {
        List<FrameModel> listOfFrames = new List<FrameModel>();

        using (AudioFileReader reader = new AudioFileReader(wavFilePath))
        {
            var samplesPerMillisecond = reader.WaveFormat.SampleRate / 1000;
            int bufferSize = minimumSilenceDuration * samplesPerMillisecond;
            float[] buffer = new float[bufferSize];
            int bytesRead;

            long position = 0;
            FrameModel currentFrame = null;

            while ((bytesRead = reader.Read(buffer, 0, bufferSize)) > 0)
            {
                double maximumAmplitude = buffer.Take(bytesRead).Select(Math.Abs).Max();
                if (maximumAmplitude > threshold)
                {
                    if (currentFrame == null)
                    {
                        currentFrame = new FrameModel { InPoint = position, TrackIndex = trackIndex, Priority = priority };
                    }
                }
                else
                {
                    if (currentFrame != null)
                    {
                        currentFrame.OutPoint = position;
                        listOfFrames.Add(currentFrame);
                        currentFrame = null;
                    }
                }

                position = (long) (reader.CurrentTime.TotalMilliseconds * 254000000) ;
            }

            if (currentFrame != null)
            {
                currentFrame.OutPoint = position;
                listOfFrames.Add(currentFrame);
            }
        }

        return listOfFrames;
    }

    public static List<FrameModel> MergeFrameLists(List<FrameModel> firstPersonFrames, List<FrameModel> secondPersonFrames)
    {
        var combinedFrames = new List<FrameModel>(firstPersonFrames.Count + secondPersonFrames.Count);
        combinedFrames.AddRange(firstPersonFrames);
        combinedFrames.AddRange(secondPersonFrames);

        combinedFrames.Sort((a, b) => a.InPoint.CompareTo(b.InPoint));

        var resultFrames = new List<FrameModel>();

        foreach (var frame in combinedFrames)
        {
            if (resultFrames.Count == 0)
            {
                resultFrames.Add(frame);
                continue;
            }

            var lastFrame = resultFrames[resultFrames.Count - 1];

            if (frame.InPoint > lastFrame.OutPoint)
            {
                // Fill empty frames by merging them with the previous frame
                lastFrame.OutPoint = frame.InPoint;
                resultFrames.Add(frame);
            }
            else
            {
                if (frame.Priority < lastFrame.Priority) // First person frame has higher priority
                {
                    if (frame.OutPoint > lastFrame.OutPoint) // Extend the first person frame if needed
                    {
                        lastFrame.OutPoint = frame.OutPoint;
                    }
                }
                else // Second person frame
                {
                    if (frame.OutPoint > lastFrame.OutPoint) // Partially overlapping
                    {
                        // Add a new frame with the non-overlapping part of the second person frame
                        resultFrames.Add(new FrameModel
                        {
                            InPoint = lastFrame.OutPoint,
                            OutPoint = frame.OutPoint,
                            Priority = frame.Priority
                        });
                    }
                }
            }
        }

        return resultFrames;
    }

    public static List<FrameModel> MergeConsecutiveFramesWithSamePriority(List<FrameModel> frames)
    {
        var mergedFrames = new List<FrameModel>();

        if (frames.Count == 0)
        {
            return mergedFrames;
        }

        FrameModel currentFrame = frames[0];

        for (int i = 1; i < frames.Count; i++)
        {
            if (frames[i].Priority == currentFrame.Priority && frames[i].InPoint == currentFrame.OutPoint)
            {
                // Merge the consecutive frame with the same priority
                currentFrame.OutPoint = frames[i].OutPoint;
            }
            else
            {
                // Add the merged frame to the result and start a new frame
                mergedFrames.Add(currentFrame);
                currentFrame = frames[i];
            }
        }

        // Add the last merged frame to the result
        mergedFrames.Add(currentFrame);

        return mergedFrames;
    }

    public static List<FrameModel> ExtendAndMergeShortFrames(List<FrameModel> frames, long minimumDuration)
    {
        var extendedFrames = new List<FrameModel>();

        if (frames.Count == 0)
        {
            return extendedFrames;
        }

        long minimumDurationTicks = minimumDuration * 254000000; // Convert milisecs to TimeSpan ticks

        FrameModel currentFrame = frames[0];

        for (int i = 1; i < frames.Count; i++)
        {
            long currentFrameDuration = currentFrame.OutPoint - currentFrame.InPoint;

            if (currentFrameDuration < minimumDurationTicks)
            {
                currentFrame.OutPoint += (minimumDurationTicks - currentFrameDuration);

                // Check if the extended frame overlaps the next frame
                if (currentFrame.OutPoint >= frames[i].InPoint)
                {
                    currentFrame.OutPoint = frames[i].OutPoint;
                }
                else
                {
                    // Add the extended frame to the result
                    extendedFrames.Add(currentFrame);
                }
            }
            else
            {
                // Add the frame to the result
                extendedFrames.Add(currentFrame);
            }

            // Start a new frame
            currentFrame = frames[i];
        }

        // Add the last frame to the result
        extendedFrames.Add(currentFrame);

        return extendedFrames;
    }

    public static List<FrameModel> RemoveShortFrames(List<FrameModel> frames, long minimumDuration)
    {
        return frames
            .Where(frame => frame.OutPoint - frame.InPoint >= minimumDuration * 254000000)
            .ToList();
    }
}