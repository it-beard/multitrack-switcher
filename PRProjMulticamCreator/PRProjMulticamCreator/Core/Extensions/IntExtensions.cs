namespace PRProjMulticamCreator.Core.Extensions;

public static class IntExtensions
{
    public static long ToTicks(this int value)
    {
        return value * Constants.TicksInOneMillisecond;
    }
}