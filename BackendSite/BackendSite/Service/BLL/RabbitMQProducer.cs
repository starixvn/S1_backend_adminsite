using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace BackendSite.Service.BLL
{
    public interface IRabbitMQProducer
    {
        void Publish(string exchange, string routingKey, object data);
    }

    public class RabbitMQProducer : IRabbitMQProducer
    {
        private readonly IConnectionFactory _connectionFactory;

        public RabbitMQProducer(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Publish(string exchange, string routingKey, object data)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            var message = JsonConvert.SerializeObject(data);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: exchange,
                                 routingKey: routingKey,
                                 basicProperties: null,
                                 body: body);
        }
    }
}
