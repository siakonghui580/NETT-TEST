namespace Application.Shared.RabbitMq
{
    public interface IRabbitMqPublisherService
    {
        void Publish(string message);
    }
}
