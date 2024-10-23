using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RapidStreamer.BuildingBlocks.Infrastructure.HealthChecks
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
                using var session = await connection.CreateSessionAsync();
                using var queue = new ActiveMQQueue(_activeMQHealthCheckOptions.Queue);
                using var producer = await session.CreateProducerAsync(queue);

                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
                producer.Priority = MsgPriority.AboveLow;

                var message = new ActiveMQMessage { NMSTimeToLive = TimeSpan.FromMilliseconds(1000) };
                await producer.SendAsync(message);

                return HealthCheckResult.Healthy();
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Unhealthy(exception.Message, exception);
            }
        }
    }
}