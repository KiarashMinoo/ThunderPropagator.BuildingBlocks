namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class DateTimeHelper
    {
        public static bool IsMidnight(this DateTime dateTime) => dateTime.TimeOfDay is { Hours: 0, Minutes: 0, Seconds: 0 };
    }
}