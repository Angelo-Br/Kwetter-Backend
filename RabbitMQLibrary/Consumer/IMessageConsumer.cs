namespace RabbitMQLibrary
{
    public interface IMessageConsumer
    {
        Task ConsumeMessageAsync(string queueName, string messageType);
    }
}