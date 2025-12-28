namespace ThunderPropagator.BuildingBlocks.Application.Helpers
{
    public static class ExceptionHelper
    {
        public static string Describe(this Exception exception, string separator = " => ")
        {
            ArgumentNullException.ThrowIfNull(exception);

            var messages = new List<string>();
            var ex = exception;
            while (ex is not null)
            {
                messages.Add(ex.Message);
                ex = ex.InnerException;
            }

            return string.Join(separator, messages);
        }
    }
}