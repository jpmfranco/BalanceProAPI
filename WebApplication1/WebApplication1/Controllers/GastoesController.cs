using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

public class GastoDto
{
    public string Descripcion { get; set; }
    public string Categoria { get; set; }
    public DateTime Fecha { get; set; }
    public int Monto { get; set; }
    public int IdUsuario { get; set; }
    public string Clasificacion { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class GastoesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly GastoesService _gastoService;

    public GastoesController(ApplicationDbContext context, GastoesService gastoService)
    {
        _context = context;
        _gastoService = gastoService;
    }

    // CREATE
    [HttpPost("CrearGasto")]
    public async Task<IActionResult> Crear(GastoDto gas)
    {
        if (gas == null) return BadRequest();

        // Verificar que el usuario existe
        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == gas.IdUsuario);
        if (!usuarioExiste)
            return BadRequest(new { mensaje = "Usuario no encontrado" });

        var gasto = new Gasto
        {
            Descripcion = gas.Descripcion,
            Categoria = gas.Categoria,
            Clasificacion = gas.Clasificacion,
            Fecha = gas.Fecha,
            Monto = gas.Monto,
            IdUsuario = gas.IdUsuario
        };

        _context.Gastos.Add(gasto);
        await _context.SaveChangesAsync();
        return Ok(gasto);
    }

    // READ ALL
    [HttpGet("ObtenerGasto")]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int? idUsuario)
    {
        var query = _context.Gastos.AsQueryable();

        if (idUsuario.HasValue && idUsuario.Value > 0)
            query = query.Where(g => g.IdUsuario == idUsuario.Value);

        return Ok(await query.OrderByDescending(g => g.Fecha).ToListAsync());
    }

    // READ BY ID
    [HttpGet("ObtenerGastoporID/{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var gasto = await _context.Gastos.FindAsync(id);
        if (gasto == null) return NotFound();
        return Ok(gasto);
    }

    // SUMA TOTAL
    [HttpGet("ObtenerSumaTotal")]
    public async Task<IActionResult> ObtenerSumaTotal(int id)
    {
        if (id <= 0)
            return BadRequest("El id no es válido");

        var count = await _gastoService.Obtenertotaltransacciones(id);
        var suma = await _gastoService.ObtenerSumaTotalUser(id);
        return Ok(new { userId = id, montoTotal = suma, totaltrans = count });
    }

    // UPDATE
    [HttpPut("EditarGasto/{id}")]
    public async Task<IActionResult> Actualizar(int id, GastoDto gastoDto)
    {
        var gasto = await _context.Gastos.FindAsync(id);
        if (gasto == null) return NotFound();

        gasto.Descripcion = gastoDto.Descripcion;
        gasto.Categoria = gastoDto.Categoria;
        gasto.Clasificacion = gastoDto.Clasificacion;
        gasto.Fecha = gastoDto.Fecha;
        gasto.Monto = gastoDto.Monto;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE
    [HttpDelete("EliminarGasto/{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var gasto = await _context.Gastos.FindAsync(id);
        if (gasto == null) return NotFound();
        _context.Gastos.Remove(gasto);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}