using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Services
{
    public class IngresoesService
    {
        private readonly ApplicationDbContext _context;

        public IngresoesService(ApplicationDbContext context)
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
            return await _context.Ingresos
                .Where(g => g.IdUsuario == id)
                .CountAsync();
        }
    }
}