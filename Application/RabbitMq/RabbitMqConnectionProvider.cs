using Application.Shared.RabbitMq;
using Core.Shared.Common;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Application.RabbitMq
{
    public class RabbitMqConnectionProvider : IRabbitMqConnectionProvider, IDisposable
    {
        #region Declarations
        private readonly IConnection _connection;
        private readonly IModel _channel;
        #endregion

        #region Constructor
        public RabbitMqConnectionProvider(IOptions<RabbitMqConfiguration> options)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(options.Value.AmqpUrl),
                Ssl = new SslOption
                {
                    Enabled = true,
                    ServerName = new Uri(options.Value.AmqpUrl).Host
                }
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: options.Value.QueueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
        }
        #endregion

        #region Methods

        #region R
        public IModel GetChannel()
        {
            return _channel;
        }
        #endregion

        #region D
        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
        #endregion

        #endregion
    }
}
