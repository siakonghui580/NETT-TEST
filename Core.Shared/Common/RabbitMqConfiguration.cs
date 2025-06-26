namespace Core.Shared.Common
{
    public class RabbitMqConfiguration
    {
        public string AmqpUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string QueueName { get; set; }
    }
}
