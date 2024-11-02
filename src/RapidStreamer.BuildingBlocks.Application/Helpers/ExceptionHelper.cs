namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class ExceptionHelper
    {
        public static string Describe(this Exception exception, string separator = " => ")
        {
            var rtn = "";

            var ex = exception;
            while (ex is not null)
            {
                rtn += string.IsNullOrWhiteSpace(rtn) switch
                {
                    false => $"{separator}{ex.Message}",
                    _ => ex.Message
                };

                ex = ex.InnerException;
            }

            return rtn;
        }
    }
}