using RabbitMQ.Client;

namespace Application.Shared.RabbitMq
{
    public interface IRabbitMqConnectionProvider
    {
        IModel GetChannel();
    }
}
