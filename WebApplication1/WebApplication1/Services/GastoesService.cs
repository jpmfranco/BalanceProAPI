using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Services
{
    public class GastoesService
    {
        private readonly Gastodbcontext _context;

        public GastoesService(Gastodbcontext context)
        {
            _context = context;
        }
        public async Task<int> ObtenerSumaTotalUser(int id)
        {
            return await _context.Gastos
                .Where(g => g.IdUsuario == id)
                .SumAsync(g => g.Monto);

        }
        public async Task<int> Obtenertotaltransacciones(int id)
        {
            var trans = await _context.Gastos.Where(g => g.IdUsuario == id).ToListAsync();
            return trans.Count();
        }

    }
}
