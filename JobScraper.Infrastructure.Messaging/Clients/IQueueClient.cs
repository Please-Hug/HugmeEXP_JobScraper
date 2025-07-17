using System.Text;
using System.Text.Json;
using JobScraper.Core.Commands;
using RabbitMQ.Client;
// ReSharper disable InconsistentNaming

namespace JobScraper.Infrastructure.Messaging.Clients;

public interface IQueueClient
{
    Task<ScrapingCommand?> ReceiveCommandAsync();
}

public class RabbitMQClient : IQueueClient
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _queueName;
    
    private RabbitMQClient(IConnection connection, IChannel channel, string queueName)
    {
        _connection = connection;
        _channel = channel;
        _queueName = queueName;
    }
    
    public static async Task<RabbitMQClient> CreateAsync(string hostName, string queueName)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
            
        return new RabbitMQClient(connection, channel, queueName);
    }

    public async Task<ScrapingCommand?> ReceiveCommandAsync()
    {
        var result = await _channel.BasicGetAsync(_queueName, autoAck: true);
        if (result == null) return null;
        
        var body = result.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        return JsonSerializer.Deserialize<ScrapingCommand>(message);
    }
}