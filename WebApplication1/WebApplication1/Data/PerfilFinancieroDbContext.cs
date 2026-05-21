using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class PerfilFinancieroDbContext : DbContext
    {
        public PerfilFinancieroDbContext(DbContextOptions<PerfilFinancieroDbContext> options)
            : base(options) { }

        public DbSet<PerfilFinanciero> PerfilesFinancieros { get; set; }
    }
}
