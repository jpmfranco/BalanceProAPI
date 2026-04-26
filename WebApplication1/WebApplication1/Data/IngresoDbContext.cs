using Microsoft.EntityFrameworkCore;
using System;
using WebApplication1.Models;
namespace WebApplication1.Data
{
    public class IngresoDbContext : DbContext
    {
        public IngresoDbContext(DbContextOptions<IngresoDbContext> options)
        : base(options) { }

        public DbSet<Ingreso> Ingresos { get; set; }
    }
}