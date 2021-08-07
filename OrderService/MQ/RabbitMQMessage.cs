using OrderService.DB.Entities;
using System;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrderService.MQ
{
    public class RabbitMQMessage
    {
        public MQMessageType MQMessageType { get; set; }
        public Customer Customer { get; set; }
    }
}
