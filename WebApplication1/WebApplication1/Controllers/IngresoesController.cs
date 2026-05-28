using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

public class IngresoDto
{
    public string Descripcion { get; set; }
    public DateTime Fecha { get; set; }
    public int Monto { get; set; }
    public int IdUsuario { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class IngresoesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IngresoesService _ingresoService;

    public IngresoesController(ApplicationDbContext context, IngresoesService ingresoService)
    {
        _context = context;
        _ingresoService = ingresoService;
    }

    // CREATE
    [HttpPost("CrearIngreso")]
    public async Task<IActionResult> Crear(IngresoDto ing)
    {
        if (ing == null) return BadRequest();

        // Verificar que el usuario existe
        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == ing.IdUsuario);
        if (!usuarioExiste)
            return BadRequest(new { mensaje = "Usuario no encontrado" });

        var ingreso = new Ingreso
        {
            Descripcion = ing.Descripcion,
            Fecha = ing.Fecha,
            Monto = ing.Monto,
            IdUsuario = ing.IdUsuario
        };

        _context.Ingresos.Add(ingreso);
        await _context.SaveChangesAsync();
        return Ok(ingreso);
    }

    // READ ALL
    [HttpGet("ObtenerIngreso")]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int? id)
    {
        var query = _context.Ingresos.AsQueryable();

        if (id.HasValue && id.Value > 0)
            query = query.Where(i => i.IdUsuario == id.Value);

        return Ok(await query.OrderByDescending(i => i.Fecha).ToListAsync());
    }

    // READ BY ID
    [HttpGet("ObtenerIngresoporID/{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var ingreso = await _context.Ingresos.FindAsync(id);
        if (ingreso == null) return NotFound();
        return Ok(ingreso);
    }

    // SUMA TOTAL
    [HttpGet("ObtenerSumaTotal")]
    public async Task<IActionResult> ObtenerSumaTotal(int id)
    {
        if (id <= 0)
            return BadRequest("El id no es válido");

        var count = await _ingresoService.Obtenertotaltransacciones(id);
        var suma = await _ingresoService.ObtenerSumaTotalUser(id);
        return Ok(new { userId = id, montoTotal = suma, totaltrans = count });
    }

    // UPDATE
    [HttpPut("EditarIngreso/{id}")]
    public async Task<IActionResult> Actualizar(int id, IngresoDto ingresoDto)
    {
        var ingreso = await _context.Ingresos.FindAsync(id);
        if (ingreso == null) return NotFound();

        ingreso.Descripcion = ingresoDto.Descripcion;
        ingreso.Fecha = ingresoDto.Fecha;
        ingreso.Monto = ingresoDto.Monto;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE
    [HttpDelete("EliminarIngreso/{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var ingreso = await _context.Ingresos.FindAsync(id);
        if (ingreso == null) return NotFound();
        _context.Ingresos.Remove(ingreso);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}