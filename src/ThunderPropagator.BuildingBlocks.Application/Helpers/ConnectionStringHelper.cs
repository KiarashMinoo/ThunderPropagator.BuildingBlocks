namespace ThunderPropagator.BuildingBlocks.Application.Helpers;

public static class ConnectionStringHelper
{
    public static string EnrichConnectionString(string connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            var connectionStringEnvironmentKeys = connectionString.GetEnvironmentKeys();

            foreach (var connectionStringEnvironmentKey in connectionStringEnvironmentKeys)
            {
                var environmentValue = Environment.GetEnvironmentVariable(connectionStringEnvironmentKey.Replace("$", ""));
                ArgumentException.ThrowIfNullOrWhiteSpace(environmentValue);
                connectionString = connectionString.Replace(connectionStringEnvironmentKey, environmentValue);
            }
        }

        return connectionString;
    }
}