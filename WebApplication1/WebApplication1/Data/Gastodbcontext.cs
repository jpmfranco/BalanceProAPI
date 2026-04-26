using Microsoft.EntityFrameworkCore;
using System;
using WebApplication1.Models;
namespace WebApplication1.Data
{
    public class Gastodbcontext: DbContext
    {
        public Gastodbcontext(DbContextOptions<Gastodbcontext> options)
        : base(options) { }

        public DbSet<Gasto> Gastos { get; set; }
    }
}
