using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data; // Verifica que tus archivos en la carpeta Data tengan este namespace
using System.Net.Http.Json;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IAController : ControllerBase
    {
        // Usamos los nombres exactos que detectó Entity Framework antes
        private readonly WebApplication1.Data.Gastodbcontext _contextGastos;
        private readonly WebApplication1.Data.IngresoDbContext _contextIngresos;

        public IAController(Gastodbcontext contextGastos, IngresoDbContext contextIngresos)
        {
            _contextGastos = contextGastos;
            _contextIngresos = contextIngresos;
        }

        [HttpGet("analizar/{usuarioId}")]
        public async Task<IActionResult> AnalizarFinanzas(int usuarioId)
        {
            try 
            {
                // 1. Obtener datos reales de SQL Server
                // Nota: Asegúrate que tus modelos tengan la propiedad 'Monto' y 'UsuarioId'
                var listaIngresos = await _contextIngresos.Ingresos
                    .Where(i => i.IdUsuario == usuarioId)
                    .ToListAsync();

                var listaGastos = await _contextGastos.Gastos
                    .Where(g => g.IdUsuario == usuarioId)
                    .ToListAsync();

                decimal totalIngresos = listaIngresos.Sum(i => i.Monto);
                decimal totalGastos = listaGastos.Sum(g => g.Monto);

                if (totalIngresos == 0) 
                    return BadRequest("El usuario no tiene ingresos registrados para el análisis.");

                // 2. Calcular variables para tu IA (basado en tu data-set.csv)
                double gastoPct = (double)(totalGastos / totalIngresos) * 100;

                var datosParaPython = new
                {
                    perfil = 1, // Valor por defecto (Empleado)
                    ingresos = (double)totalIngresos,
                    estabilidad = 1,
                    gasto_pct = gastoPct,
                    categoria = 1,
                    tend_ingresos = 1,
                    tend_egresos = 0,
                    control = 4,
                    planificacion = 3
                };

                // 3. Llamar al microservicio de Python en Docker
                using var client = new HttpClient();
                // Importante: "ia-service" es el nombre en tu docker-compose.yml
                var response = await client.PostAsJsonAsync("http://localhost:5002/predict", datosParaPython);
                
                if (response.IsSuccessStatusCode)
                {
                    var prediccion = await response.Content.ReadFromJsonAsync<dynamic>();
                    return Ok(new { 
                        mensaje = "Análisis de IA BalancePro", 
                        resumenFinanciero = new { totalIngresos, totalGastos, porcentajeGasto = gastoPct },
                        prediccionIA = prediccion 
                    });
                }
                
                return StatusCode(502, "El servicio de IA (Python) no respondió correctamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}