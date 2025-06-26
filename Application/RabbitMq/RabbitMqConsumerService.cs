using Application.Shared.RabbitMq;
using Core.Shared.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Application.RabbitMq
{
    public class RabbitMqConsumerService : BackgroundService
    {
        #region Declarations
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly ILogger<RabbitMqConsumerService> _logger;
        #endregion

        #region Constructor
        public RabbitMqConsumerService(IRabbitMqConnectionProvider provider,
                                IOptions<RabbitMqConfiguration> options,
                                ILogger<RabbitMqConsumerService> logger)
        {
            _channel = provider.GetChannel();
            _queueName = options.Value.QueueName;
            _logger = logger;
        }
        #endregion

        #region Methods
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"[RabbitMQ] Received: {message}");
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }
        #endregion
    }
}
