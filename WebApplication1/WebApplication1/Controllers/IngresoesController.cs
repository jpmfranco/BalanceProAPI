using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    private readonly IngresoDbContext _context;
    private readonly IngresoesService ingresoesService;

    public IngresoesController(IngresoDbContext context,IngresoesService _ingresoService)
    {
        _context = context;
        ingresoesService = _ingresoService;
    }

    // CREATE
    [HttpPost("CrearIngreso")]
    public async Task<IActionResult> Crear(IngresoDto ing)
    {
        if (ing == null) return BadRequest();
        var ingreso = new Ingreso()
        {
            Descripcion  = ing.Descripcion,
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
    public async Task<IActionResult> ObtenerTodos()
    {
        return Ok(await _context.Ingresos.ToListAsync());
    }

    // READ BY ID
    [HttpGet("ObtenerIngresoporID/{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var gasto = await _context.Ingresos.FindAsync(id);
        if (gasto == null) return NotFound();
        return Ok(gasto);
    }
    [HttpGet("ObtenerSumaTotal")]
    public async Task<IActionResult> ObtenerSumaTotal(int id)
    {
        if (id <= 0)
            return BadRequest("El id no es válido");
        var count = await ingresoesService.Obtenertotaltransacciones(id);
        var suma = await ingresoesService.ObtenerSumaTotalUser(id);

        return Ok(new { userId = id, montoTotal = suma, totaltrans = count });
    }
    // UPDATE
    [HttpPut("EditarIngreso/{id}")]
    public async Task<IActionResult> Actualizar(int id, Ingreso ingreso)
    {
        if (id != ingreso.Id) return BadRequest();

        _context.Entry(ingreso).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE
    [HttpDelete("EliminarIngreso/{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _context.Ingresos.FindAsync(id);
        if (producto == null) return NotFound();

        _context.Ingresos.Remove(producto);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
