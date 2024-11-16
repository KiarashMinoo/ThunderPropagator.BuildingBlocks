namespace RapidStreamer.BuildingBlocks.Application.Helpers;

public static class EnvironmentHelper
{
    public static IEnumerable<string> GetEnvironmentKeys(this string str)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(str);
        var index = 0;
        while (true)
        {
            index = str.IndexOf('$', index);
            if (index <= 0)
                break;

            var nextIndex = str.IndexOf('$', index + 1) + 1;
            if (nextIndex <= 0)
                break;

            yield return str.Substring(index, nextIndex - index);

            index = nextIndex;
        }
    }
}