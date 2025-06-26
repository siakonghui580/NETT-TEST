using Application.Shared.RabbitMq;
using Core.Shared.Common;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace Application.RabbitMq
{
    public class RabbitMqPublisherService : IRabbitMqPublisherService
    {
        #region Declarations
        private readonly IModel _channel;
        private readonly string _queueName;
        #endregion

        #region Constructor
        public RabbitMqPublisherService(IRabbitMqConnectionProvider provider, IOptions<RabbitMqConfiguration> options)
        {
            _channel = provider.GetChannel();
            _queueName = options.Value.QueueName;
        }
        #endregion

        #region Methods
        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "",
                                  routingKey: _queueName,
                                  basicProperties: null,
                                  body: body);
        }
        #endregion
    }
}
