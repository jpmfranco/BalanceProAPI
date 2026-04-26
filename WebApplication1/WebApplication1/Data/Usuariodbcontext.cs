using Microsoft.EntityFrameworkCore;
using System;
using WebApplication1.Models;
namespace WebApplication1.Data
{
    public class UsuarioDbContext : DbContext
    {
        public UsuarioDbContext(DbContextOptions<UsuarioDbContext> options)
        : base(options) { }

        public DbSet<Usuario> Usuario { get; set; }
    }
}
