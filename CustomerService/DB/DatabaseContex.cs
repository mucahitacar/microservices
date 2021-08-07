using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomerService.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.DB
{
    public class DatabaseContex:DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Address{ get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=my_db;Username=postgres;Password=postgres");
    }
}
