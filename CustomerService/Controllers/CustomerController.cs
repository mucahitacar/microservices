using AutoMapper;
using CustomerService.DB;
using CustomerService.DB.Entities;
using CustomerService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomerService.Controllers
{
    public enum MQMessageType
    {
        Create,
        Update,
        Delete,
    }

    public class RabbitMQMessage
    {
        public MQMessageType MQMessageType { get; set; }
        public Customer Customer { get; set; }
    }



    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        DatabaseContex db;
        private readonly IMapper _mapper;

        public CustomerController(DatabaseContex db, IMapper mapper)
        {
            this.db = db;
            db.Customers.Include(x => x.Address).ToList() ;
            _mapper = mapper;
        }

        [HttpGet("getAll")]
        public IEnumerable<CustomerDto> Get()
        {
            //List<Customer> list = db.Customers.Include(x => x.Address).ToList();
            List<Customer> customers = db.Customers.ToList();
            return _mapper.Map<List<Customer>, List<CustomerDto>>(customers);
        }

        // GET api/<CustomerController>/5
        [HttpGet("{id}")]
        public CustomerDto Get(int id)
        {
            return _mapper.Map<Customer,CustomerDto>(db.Customers.Find(id));
        }

        // POST api/<CustomerController>
        [HttpPost("Save")]
        public void Create([FromBody] CustomerDto customerDto)
        {
            try
            {
                Customer newCustomer = _mapper.Map<CustomerDto, Customer>(customerDto);
                db.Customers.Add(newCustomer);
                db.SaveChanges();
                Send(newCustomer,MQMessageType.Create);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);   
            }
        }

        // PUT api/<CustomerController>/5
        [HttpPut("{id}")]
        public void Put(int id, CustomerDto customerDto)
        {
            Customer find = db.Customers.Find(id);
            db.Customers.Remove(find);
            db.Customers.Add(_mapper.Map<CustomerDto, Customer>(customerDto));
            db.SaveChanges();

        }

        // DELETE api/<CustomerController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            Customer find = db.Customers.Find(id);
            db.Customers.Remove(find);
            db.SaveChanges();

        }
        void Send(Customer newCustomer,MQMessageType messageType)
        {
            RabbitMQMessage rabbitMQMessage = new RabbitMQMessage { Customer = newCustomer, MQMessageType = messageType };
            string rabbitMQMessageJson = Newtonsoft.Json.JsonConvert.SerializeObject(rabbitMQMessage);
            var factory = new ConnectionFactory() { HostName = "192.168.1.38", Port=5672};

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "Microservices",
                                        type: ExchangeType.Direct);
                var body = Encoding.UTF8.GetBytes(rabbitMQMessageJson);
                channel.BasicPublish(exchange: "Microservices",
                                     routingKey: "Customer",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" Sent '{0}':'{1}'", DateTime.Now, rabbitMQMessageJson);
            }
        }
    }
}
