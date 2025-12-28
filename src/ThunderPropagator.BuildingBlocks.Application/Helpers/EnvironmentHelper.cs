namespace ThunderPropagator.BuildingBlocks.Application.Helpers;

public static class EnvironmentHelper
{
    public static IEnumerable<string> GetEnvironmentKeys(this string str)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(str);
        var index = 0;
        while ((index = str.IndexOf('$', index)) >= 0)
        {
            var start = index;
            index = str.IndexOf('$', index + 1);
            if (index < 0) break;
            yield return str.Substring(start, index - start + 1);
            index++;
        }
    }
}