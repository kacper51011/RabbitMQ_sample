using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

ConnectionFactory factory = new();
/* amqp - a protocol, guest:guest - our login and password to rabbitMQ MS
 * and then localhost with default port where our rabbitMQ container can run
 * we can pass those values more explicitly by using factory.UserName, factory.Password etc.
 there is a possibility to pass multiple URI`s */

factory.Uri = new Uri("amqp://guest:guest@localhost:5672");

// name of the client, useful for identifying which client have problems, is not working etc.
factory.ClientProvidedName = "My rabbitmq publisher";

IConnection connection = factory.CreateConnection();

IModel channel = connection.CreateModel();

string routingKey1 = "12345";
string routingKey2 = "6789";
string queueName1 = "database";
string queueName2 = "errors";

/*There are 6 types of exhanges in rabbitMQ, 4 of them are mostly used:
 * 
 * * Direct - uses a message routing key to transport messages to queues. 
 * The routing key is a message attribute that the producer adds to the message header.
 * image: https://www.cloudamqp.com/img/blog/direct-exchange.png
 * 
 * * Topic - sends messages to queues depending on wildcard matches 
 * between the routing key and the queue binding’s routing pattern.
 * image: https://www.cloudamqp.com/img/blog/topic-exchange.png
 * 
 * * Fanout - duplicates and routes a received message to any associated queues, 
 * regardless of routing keys or pattern matching. Here, your provided keys will be entirely ignored. 
 * image: https://www.cloudamqp.com/img/blog/fanout-exchange.png
 * 
 * * Header - A headers RabbitMQ exchange type is a message routing system
 * that uses arguments with headers and optional values to route messages.
 * image: https://www.cloudamqp.com/img/blog/headers-exchange.png
 */

channel.ExchangeDeclare("logs", ExchangeType.Direct);

/*When we declare queues, we can provide some additional parameters, parameters here:
 * queue: name of the queue, possible to provide empty name to let server generate the name
 * durable: Should this queue survive a broker restart?
 * exclusive: Should this queue use be limited to its declaring connection? 
 * Such a queue will be deleted when its declaring connection closes.
 * autoDelete: Should this queue be auto-deleted when its last consumer (if any) unsubscribes?
 * args: Optional; additional queue arguments, e.g. "x-queue-type"
*/
channel.QueueDeclare(queueName1, durable: false, exclusive: false, autoDelete: true);
channel.QueueDeclare(queueName2, durable: false, exclusive: false, autoDelete: true);

channel.QueueBind(queueName1, "logs", routingKey1);
channel.QueueBind(queueName2, "logs", routingKey2);


byte[] databaseMessage = Encoding.UTF8.GetBytes($"created new user in {DateTime.Now} ");
byte[] errorMessage = Encoding.UTF8.GetBytes("something went wrong in example/route");


channel.BasicPublish("logs", routingKey1, null, databaseMessage);
channel.BasicPublish("logs", routingKey2, null, errorMessage);


channel.Close();
connection.Close();