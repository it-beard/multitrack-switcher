namespace PRProjMulticamCreator.Core.Extensions;

public static class DoubleExtensions
{
    public static long ToTicks(this double value)
    {
        return  (long) (value * Constants.TicksInOneMillisecond);
    }
}