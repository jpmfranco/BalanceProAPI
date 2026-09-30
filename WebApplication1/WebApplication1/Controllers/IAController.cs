using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System.Net.Http.Json;

namespace WebApplication1.Controllers
{
    public class PerfilDto
    {
        public int Perfil { get; set; }
        public int Estabilidad { get; set; }
        public int Categoria { get; set; }
        public double ingresos { get; set; }
        public int TendIngresos { get; set; }
        public int TendEgresos { get; set; }
        public int Control { get; set; }
        public int Planificacion { get; set; }
        public double gastosPct { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IAController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public IAController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: recupera el perfil guardado del usuario
        [HttpGet("analizar/{usuarioId}")]
        public async Task<IActionResult> AnalizarFinanzasGet(int usuarioId)
        {
            var perfilGuardado = await _context.PerfilesFinancieros
                .FirstOrDefaultAsync(p => p.IdUsuario == usuarioId);

            if (perfilGuardado == null)
                return NotFound("El usuario aún no ha configurado su perfil financiero.");

            var totalIngresos = await _context.Ingresos
                .Where(i => i.IdUsuario == usuarioId)
                .SumAsync(i => (decimal)i.Monto);

            var totalGastos = await _context.Gastos
                .Where(g => g.IdUsuario == usuarioId)
                .SumAsync(g => (decimal)g.Monto);

            double gastoPct = totalIngresos > 0
                ? (double)(totalGastos / totalIngresos) * 100
                : 0;

            var perfil = new PerfilDto
            {
                Perfil = perfilGuardado.Perfil,
                Estabilidad = perfilGuardado.Estabilidad,
                Categoria = perfilGuardado.Categoria,
                ingresos = (double)totalIngresos,
                gastosPct = gastoPct,
                TendIngresos = perfilGuardado.TendIngresos,
                TendEgresos = perfilGuardado.TendEgresos,
                Control = perfilGuardado.Control,
                Planificacion = perfilGuardado.Planificacion
            };

            return await EjecutarAnalisis(usuarioId, perfil);
        }

        // POST: guarda el perfil y ejecuta el análisis
        [HttpPost("analizar/{usuarioId}")]
        public async Task<IActionResult> AnalizarFinanzasPost(int usuarioId, [FromBody] PerfilDto perfil)
        {
            // Upsert: actualiza si existe, crea si no
            var perfilExistente = await _context.PerfilesFinancieros
                .FirstOrDefaultAsync(p => p.IdUsuario == usuarioId);

            if (perfilExistente == null)
            {
                _context.PerfilesFinancieros.Add(new PerfilFinanciero
                {
                    IdUsuario = usuarioId,
                    Perfil = perfil.Perfil,
                    Estabilidad = perfil.Estabilidad,
                    Categoria = perfil.Categoria,
                    TendIngresos = perfil.TendIngresos,
                    TendEgresos = perfil.TendEgresos,
                    Control = perfil.Control,
                    Planificacion = perfil.Planificacion,
                    FechaCreacion = DateTime.UtcNow
                });
            }
            else
            {
                perfilExistente.Perfil = perfil.Perfil;
                perfilExistente.Estabilidad = perfil.Estabilidad;
                perfilExistente.Categoria = perfil.Categoria;
                perfilExistente.TendIngresos = perfil.TendIngresos;
                perfilExistente.TendEgresos = perfil.TendEgresos;
                perfilExistente.Control = perfil.Control;
                perfilExistente.Planificacion = perfil.Planificacion;
                perfilExistente.FechaCreacion = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return await EjecutarAnalisis(usuarioId, perfil);
        }

        // Lógica compartida entre GET y POST
        private async Task<IActionResult> EjecutarAnalisis(int usuarioId, PerfilDto perfil)
        {
            try
            {
                var totalIngresos = await _context.Ingresos
                    .Where(i => i.IdUsuario == usuarioId)
                    .SumAsync(i => (decimal)i.Monto);

                var totalGastos = await _context.Gastos
                    .Where(g => g.IdUsuario == usuarioId)
                    .SumAsync(g => (decimal)g.Monto);

                double gastoPct = totalIngresos > 0
                    ? (double)(totalGastos / totalIngresos) * 100
                    : 0;

                var datosParaPython = new
                {
                    perfil = perfil.Perfil,
                    ingresos = (double)totalIngresos,
                    estabilidad = perfil.Estabilidad,
                    gasto_pct = gastoPct,
                    categoria = perfil.Categoria,
                    tend_ingresos = perfil.TendIngresos,
                    tend_egresos = perfil.TendEgresos,
                    control = perfil.Control,
                    planificacion = perfil.Planificacion
                };

                // URL del servicio Python: env PYTHON_IA_URL > config PythonIA:Url > default local
                var pythonUrl = Environment.GetEnvironmentVariable("PYTHON_IA_URL")
                    ?? _configuration["PythonIA:Url"]
                    ?? "http://localhost:5002/predict";

                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync(pythonUrl, datosParaPython);

                if (response.IsSuccessStatusCode)
                {
                    var prediccion = await response.Content.ReadFromJsonAsync<dynamic>();
                    return Ok(new
                    {
                        mensaje = "Análisis de IA BalancePro",
                        resumenFinanciero = new
                        {
                            totalIngresos,
                            totalGastos,
                            porcentajeGasto = gastoPct
                        },
                        prediccionIA = prediccion,
                        iaDisponible = true
                    });
                }

                // Python respondió con error: el perfil YA quedó guardado
                // (upsert previo); se devuelve el resumen sin proyección.
                return ResumenSinIA(totalIngresos, totalGastos, gastoPct);
            }
            catch (HttpRequestException)
            {
                // Sin servicio Python (PYTHON_IA_URL sin configurar): degradar,
                // no fallar. El perfil ya quedó guardado.
                var totales = await Totales(usuarioId);
                return ResumenSinIA(totales.ingresos, totales.gastos, totales.gastoPct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        private async Task<(decimal ingresos, decimal gastos, double gastoPct)> Totales(int usuarioId)
        {
            var totalIngresos = await _context.Ingresos
                .Where(i => i.IdUsuario == usuarioId)
                .SumAsync(i => (decimal)i.Monto);
            var totalGastos = await _context.Gastos
                .Where(g => g.IdUsuario == usuarioId)
                .SumAsync(g => (decimal)g.Monto);
            double gastoPct = totalIngresos > 0
                ? (double)(totalGastos / totalIngresos) * 100
                : 0;
            return (totalIngresos, totalGastos, gastoPct);
        }

        private OkObjectResult ResumenSinIA(decimal totalIngresos, decimal totalGastos, double gastoPct)
        {
            return Ok(new
            {
                mensaje = "Perfil guardado. IA no disponible por el momento.",
                resumenFinanciero = new
                {
                    totalIngresos,
                    totalGastos,
                    porcentajeGasto = gastoPct
                },
                prediccionIA = (object?)null,
                advertencia = "El servicio de IA (Python) no está configurado; se muestra tu resumen sin proyección.",
                iaDisponible = false
            });
        }
    }

    public class PythonResponse
    {
        public string status { get; set; } = string.Empty;
        public List<ProyeccionData> proyecciones { get; set; } = new();
    }

    public class ProyeccionData
    {
        public string mes { get; set; } = string.Empty;
        public double valor { get; set; }
    }
}