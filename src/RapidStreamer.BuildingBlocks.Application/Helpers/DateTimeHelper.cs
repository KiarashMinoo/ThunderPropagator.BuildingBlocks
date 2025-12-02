namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class DateTimeHelper
    {
        public static bool IsMidnight(this DateTime dateTime, int variance = 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(variance);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(3600, variance, nameof(variance));

            var time = dateTime.TimeOfDay;
            if (time.Hours > 0) return false;
            var totalSeconds = time.Minutes * 60 + time.Seconds;
            return totalSeconds <= variance;
        }
    }
}