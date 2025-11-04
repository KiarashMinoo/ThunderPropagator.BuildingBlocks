using System.Text;

namespace RapidStreamer.BuildingBlocks.Application.Helpers
{
    public static class ExceptionHelper
    {
        public static string Describe(this Exception exception, string separator = " => ")
        {
            var sb = new StringBuilder();

            var ex = exception;
            while (ex is not null)
            {
                if (sb.Length == 0)
                {
                    sb.Append(ex.Message);
                }
                else
                {
                    sb.Append(separator);
                    sb.Append(ex.Message);
                }

                ex = ex.InnerException;
            }

            return sb.ToString();
        }
    }
}