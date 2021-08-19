using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.DB;
using OrderService.DB.Entities;
using OrderService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrderService.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        DatabaseContex _db;
        private readonly IMapper _mapper;

        public OrderController(DatabaseContex db, IMapper mapper)
        {
            _db = db;
            _db.Orders.Include(x => x.Address).ToList();
            _db.Orders.Include(x => x.Product).ToList();
            _mapper = mapper;
        }


        // GET: api/<OrderController>

        [HttpGet("getAll")]
        public List<OrderDto> Get()
        {
            return _mapper.Map<List<Order>, List<OrderDto>>(_db.Orders.ToList());
        }

        // GET api/<OrderController>/5
        [HttpGet("{id}")]
        public OrderDto Get(int id)
        {
            return _mapper.Map<Order, OrderDto>(_db.Orders.Find(id));
        }

        // POST api/<OrderController>
        [HttpPost("Save")]
        public async Task<IActionResult> Post([FromBody] OrderDto orderDto)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage httpResponseMessage = new HttpResponseMessage();
            var isThereCustomer = false;

            if (_db.Customers.Any(x => x.Id == orderDto.CustomerId))
                isThereCustomer = true;

            else if ((await httpClient.GetAsync($"http://192.168.1.38:8001/customer/{orderDto.CustomerId}")).StatusCode == System.Net.HttpStatusCode.OK)
                isThereCustomer = true;

            //var adress = _db.Addresses.Find(orderDto.Address.Id);
            //if (adress!=null)
            if (isThereCustomer)
            {
                orderDto.CreatedAt = DateTime.Now;
                _db.Orders.Add(_mapper.Map<OrderDto, Order>(orderDto));
                
                _db.SaveChanges();
                return Ok();
            }

            return NotFound();
        }

        // PUT api/<OrderController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] OrderDto orderDto)
        {
            Order findResult = _db.Orders.Find(id);
            findResult.Price = orderDto.Price;
            findResult.Product =_mapper.Map<ProductDto,Product>(orderDto.Product);
            findResult.Quantity = orderDto.Quantity;
            findResult.Status = orderDto.Status;
            findResult.UpdatedAt = DateTime.Now;
            _db.SaveChanges();

        }

        // DELETE api/<OrderController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _db.Orders.Remove(_db.Orders.Find(id));
            _db.SaveChanges();

        }

    }
}
