using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Services
{
    public class IngresoesService
    {
        private readonly IngresoDbContext _context;

        public IngresoesService(IngresoDbContext context)
        {
            _context = context;
        }
        public async Task<int> ObtenerSumaTotalUser(int id)
        {
            return await _context.Ingresos
                .Where(g => g.IdUsuario == id)
                .SumAsync(g => g.Monto);

        }
        public async Task<int> Obtenertotaltransacciones(int id)
        {
            var trans = await _context.Ingresos.Where(g => g.IdUsuario == id).ToListAsync();
            return trans.Count();
        }
    }
}
