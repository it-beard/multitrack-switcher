namespace PRProjMulticamCreator.Models;

public class FrameModel
{
    public long InPoint { get; set; }
    public long OutPoint { get; set; }
    public int TrackIndex { get; set; }

    public int Priority { get; set; } // 0 - lowest
}