using AutoMapper;
using OrderService.DB.Entities;
using OrderService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Mapping
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>();
            CreateMap<OrderDto, Order>()
                .ForMember(x => x.Id, opt => opt.Ignore());
            CreateMap<Address, AddressDto>();
            CreateMap<AddressDto, Address>()
                .ForMember(x => x.Id, opt => opt.Ignore());
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>()
                .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<Customer, CustomerDto>();
        }
    }
}
