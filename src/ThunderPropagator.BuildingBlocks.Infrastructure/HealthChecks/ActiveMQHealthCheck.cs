using Apache.NMS.ActiveMQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ThunderPropagator.BuildingBlocks.Infrastructure.HealthChecks
{
    internal
#if !DEBUG
        sealed
#endif
        class ActiveMQHealthCheck : IHealthCheck
    {
        private readonly ActiveMQHealthCheckOptions _activeMQHealthCheckOptions;

        public ActiveMQHealthCheck(ActiveMQHealthCheckOptions activeMQHealthCheckOptions)
        {
            _activeMQHealthCheckOptions = activeMQHealthCheckOptions;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionFactory = new ConnectionFactory(new Uri(_activeMQHealthCheckOptions.BrokerUri));

                if (!string.IsNullOrWhiteSpace(_activeMQHealthCheckOptions.ClientId))
                {
                    connectionFactory.ClientId = _activeMQHealthCheckOptions.ClientId;
                }

                if (!string.IsNullOrWhiteSpace(_activeMQHealthCheckOptions.UserName))
                {
                    connectionFactory.UserName = _activeMQHealthCheckOptions.UserName;
                }

                if (!string.IsNullOrWhiteSpace(_activeMQHealthCheckOptions.Password))
                {
                    connectionFactory.Password = _activeMQHealthCheckOptions.Password;
                }

                using var connection = await connectionFactory.CreateConnectionAsync();
                await connection.StartAsync();
                using var session = await connection.CreateSessionAsync();
                await session.CloseAsync();

                return HealthCheckResult.Healthy();
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Unhealthy(exception.Message, exception);
            }
        }
    }
}