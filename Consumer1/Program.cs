using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

ConnectionFactory factory = new();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");

factory.ClientProvidedName = "My rabbitmq consumer1";

IConnection connection = factory.CreateConnection();

IModel channel = connection.CreateModel();

string routingKey1 = "12345";
string queueName1 = "database";

channel.ExchangeDeclare("logs", ExchangeType.Direct);

channel.QueueDeclare(queueName1, durable: false, exclusive: false, autoDelete: true);
channel.QueueBind(queueName1, "logs", routingKey1);
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);


var consumer = new EventingBasicConsumer(channel);

consumer.Received += (sender, args) =>
{
    var body = args.Body.ToArray();

    string message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"message received: {message}");

    channel.BasicAck(args.DeliveryTag, false);
};

string consumerTag = channel.BasicConsume(queueName1, false, consumer);

Console.ReadLine();

channel.BasicCancel(consumerTag);

channel.Close();
connection.Close();