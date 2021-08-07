using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderService.DB;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.MQ
{

    public class CustomerMQ
    {
        public void Consume()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var routingKey = "Customer";

            channel.ExchangeDeclare(exchange: "Microservices", type: ExchangeType.Direct);

            channel.QueueDeclare(routingKey, true, false, false);

            channel.QueueBind(queue: "Customer", exchange: "Microservices", routingKey = "Customer");

            Console.WriteLine(" [*] Waiting for messages. ");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;
                RabbitMQMessage recievedCustomer = JsonConvert.DeserializeObject<RabbitMQMessage>(message);
                FetchCustomer(recievedCustomer);
                Console.WriteLine(" [x] Received '{0}':'{1}' ",
                                   message, DateTime.Now);
            };
            channel.BasicConsume(queue: "Customer",
                                 autoAck: true,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
        }

        private void FetchCustomer(RabbitMQMessage recievedCustomer)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContex>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=order_service;Username=postgres;Password=postgres");
            using (var _db = new DatabaseContex(optionsBuilder.Options))
            {

                switch (recievedCustomer.MQMessageType)
                {
                    case MQMessageType.Create:
                        _db.Customers.Add(recievedCustomer.Customer);
                        _db.SaveChanges();
                        break;
                    case MQMessageType.Update:
                        _db.Customers.Remove(_db.Customers.Find(recievedCustomer.Customer.Id));
                        _db.Customers.Add(recievedCustomer.Customer);
                        break;
                    case MQMessageType.Delete:
                        _db.Customers.Remove(_db.Customers.Find(recievedCustomer.Customer.Id));
                        break;
                    default:
                        break;
                }
                _db.SaveChanges();
            }
        }
    }
}
