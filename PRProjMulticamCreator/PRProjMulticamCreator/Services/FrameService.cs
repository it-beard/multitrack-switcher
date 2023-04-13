using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;
using PRProjMulticamCreator.Core;
using PRProjMulticamCreator.Core.Extensions;
using PRProjMulticamCreator.Models;

namespace PRProjMulticamCreator.Services;

public class FrameService
{
    private const int MinimumSilenceDuration = 1500; // in milliseconds
    public List<FrameModel> RemoveNoiseFrames(List<FrameModel> frames)
    {
        const int noiseFrameLength = MinimumSilenceDuration / 2;
        var result = new List<FrameModel>();
        if (frames.Count == 0)
        {
            return result;
        }

        result.AddRange(
            frames.Where(
                frame => frame.OutPoint - frame.InPoint > noiseFrameLength * Constants.TicksInOneMillisecond));

        return result.OrderBy(f => f.InPoint).ToList();
    }

    public List<FrameModel> RemoveLongFrames(List<FrameModel> frames)
    {
        const int longFrameLength = MinimumSilenceDuration * 2;
        var result = new List<FrameModel>();
        foreach (var frame in frames)
        {
            if (frames.Count == 0)
            {
                return result;
            }

            if(frame.OutPoint - frame.InPoint <= longFrameLength * Constants.TicksInOneMillisecond)
            {
                result.Add(frame);
            }
        }

        return result.OrderBy(f => f.InPoint).ToList();
    }

    public List<FrameModel> AddShortFramesToAllFrames(List<FrameModel> shortFrames, List<FrameModel> allFrames)
    {
        var result = new List<FrameModel>(allFrames);
        foreach (var frame in allFrames)
        {
            foreach (var shortFrame in shortFrames)
            {
                if (shortFrame.InPoint > frame.InPoint && shortFrame.OutPoint < frame.OutPoint)
                {
                    result.Add(shortFrame);
                    result.Remove(frame);
                }
            }
        }

        return AddPrimaryFramesInsteadOfBlack(result.OrderBy(f => f.InPoint).ToList());
    }

    public List<FrameModel> DiluteLongFrames(List<FrameModel> frames)
    {
        var longFrameLength = 45000 * Constants.TicksInOneMillisecond; // in ticks
        var dilutionFrameLength = 3000 * Constants.TicksInOneMillisecond; // in ticks
        var result = new List<FrameModel>(frames);
        foreach (var frame in frames)
        {
            if (frame.OutPoint - frame.InPoint > longFrameLength)
            {
                result.Remove(frame);
                var dilutionFrameInPoint = frame.InPoint + (frame.OutPoint - frame.InPoint) / 2 - dilutionFrameLength / 2;
                var dilutionFrameOutPoint = frame.InPoint + (frame.OutPoint - frame.InPoint) / 2 + dilutionFrameLength / 2;
                result.Add(new FrameModel
                {
                    InPoint = frame.InPoint,
                    OutPoint = dilutionFrameInPoint,
                    TrackIndex = frame.TrackIndex
                });
                // add inverted frame in the middle
                result.Add(new FrameModel
                {
                    InPoint = dilutionFrameInPoint,
                    OutPoint = dilutionFrameOutPoint,
                    TrackIndex =
                        frame.TrackIndex == Constants.PremierProSecondaryTrackIndex
                            ? Constants.PremierProPrimaryTrackIndex
                            : Constants.PremierProSecondaryTrackIndex
                });
                result.Add(new FrameModel
                {
                    InPoint = dilutionFrameOutPoint,
                    OutPoint = frame.OutPoint,
                    TrackIndex = frame.TrackIndex
                });
            }
        }

        return result.OrderBy(f => f.InPoint).ToList();
    }

    public List<FrameModel> GetSyntheticFrames(List<FrameModel> primaryFrames)
    {
        var result = new List<FrameModel>();
        if (primaryFrames.Count == 0)
        {
            return result;
        }

        long inPoint = 0;
        for (int i = 0; i < primaryFrames.Count; i++)
        {
            if (inPoint < primaryFrames[i].InPoint)
            {
                result.Add(new FrameModel
                {
                    InPoint = inPoint,
                    OutPoint = primaryFrames[i].InPoint,
                    TrackIndex = Constants.PremierProSecondaryTrackIndex
                });
                inPoint = primaryFrames[i].OutPoint;
            }
        }

        return result.OrderBy(f => f.InPoint).ToList();
    }

    public List<FrameModel> MergeThroughSilence(List<FrameModel> frames)
    {
        var silenceStep = MinimumSilenceDuration;
        var result = new List<FrameModel>();
        if (frames.Count == 0)
        {
            return result;
        }

        var currentFrame = frames[0];
        for (int i = 1; i < frames.Count; i++)
        {
            if (frames[i].InPoint - currentFrame.OutPoint <= silenceStep * Constants.TicksInOneMillisecond)
            {
                currentFrame.OutPoint = frames[i].OutPoint;
            }
            else
            {
                result.Add(currentFrame);
                currentFrame = frames[i];
            }

        }

        return result.OrderBy(f => f.InPoint).ToList();
    }

    public List<FrameModel> GetListOfPureWavFrames(string wavFilePath, double threshold, int trackIndex)
    {
        var listOfFrames = new List<FrameModel>();

        using AudioFileReader reader = new AudioFileReader(wavFilePath);
        var samplesPerMillisecond = reader.WaveFormat.SampleRate / 1000;
        int bufferSize = MinimumSilenceDuration * samplesPerMillisecond;
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
                    currentFrame = new FrameModel { InPoint = position, TrackIndex = trackIndex};
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

            position = reader.CurrentTime.TotalMilliseconds.ToTicks();
        }

        if (currentFrame != null)
        {
            currentFrame.OutPoint = position;
            listOfFrames.Add(currentFrame);
        }

        return listOfFrames.OrderBy(f => f.InPoint).ToList();
    }

    private List<FrameModel> AddPrimaryFramesInsteadOfBlack(List<FrameModel> frames)
    {
        var result = new List<FrameModel>(frames);
        if (frames.Count == 0)
        {
            return result;
        }

        for (int i = 0; i < frames.Count-1; i++)
        {
            if (frames[i].OutPoint != frames[i+1].InPoint)
            {
                result.Add(new FrameModel
                {
                    InPoint = frames[i].OutPoint,
                    OutPoint = frames[i+1].InPoint,
                    TrackIndex = Constants.PremierProPrimaryTrackIndex
                });
            }
        }

        return result.OrderBy(f => f.InPoint).ToList();
    }
}