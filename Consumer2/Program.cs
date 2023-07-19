using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

ConnectionFactory factory = new();

//its possible to set multiple uri`s
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");

factory.ClientProvidedName = "My rabbitmq consumer2";

IConnection connection = factory.CreateConnection();

IModel channel = connection.CreateModel();

string routingKey2 = "6789";
string queueName2 = "errors";

channel.ExchangeDeclare("logs", ExchangeType.Direct);



channel.QueueDeclare(queueName2, durable: false, exclusive: false, autoDelete: true);
channel.QueueBind(queueName2, "logs", routingKey2);
//prefetchSize: how big can our received message be
//prefetchCount: how many messages can be send to us at once
//global: if true it applies prefetch for all consumers
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

var consumer = new EventingBasicConsumer(channel);

//event fired when a delivery arrives at the consumer:
consumer.Received += (sender, args) =>
{
    var body = args.Body.ToArray();

    string message = Encoding.UTF8.GetString(body);

    Console.WriteLine($"message received: {message}");
   
    // acknowledgment that we received message successfully
    channel.BasicAck(args.DeliveryTag, false);
};
//consumerTag:
//first parameter: queue name
//second parameter: autoack
//third parameter: our consumer

string consumerTag = channel.BasicConsume(queueName2, false, consumer);

Console.ReadLine();

channel.BasicCancel(consumerTag);

channel.Close();
connection.Close();