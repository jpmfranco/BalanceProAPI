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

public class GastoDto
{
    public string Descripcion { get; set; }
    public string Categoria { get; set; }
    public DateTime Fecha { get; set; }
    public int Monto { get; set; }
    public int IdUsuario { get; set; }

}

[ApiController]
[Route("api/[controller]")]
public class GastoesController : ControllerBase
{
    private readonly Gastodbcontext _context;
    private readonly GastoesService _gastoService;



    public GastoesController(Gastodbcontext context, GastoesService gastoService)
    {
        _context = context;
        this._gastoService = gastoService;
    }

    // CREATE
    [HttpPost("CrearGasto")]
    public async Task<IActionResult> Crear(GastoDto gas)
    {
        if (gas == null) return BadRequest();
        var gasto = new Gasto()
        {
            Descripcion = gas.Descripcion,
            Categoria = gas.Categoria,
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
    public async Task<IActionResult> ObtenerTodos()
    {
        return Ok(await _context.Gastos.ToListAsync());
    }

    // READ BY ID
    [HttpGet("ObtenerGastoporID/{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var gasto = await _context.Gastos.FindAsync(id);
        if (gasto == null) return NotFound();
        return Ok(gasto);
    }
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
    public async Task<IActionResult> Actualizar(int id, Gasto gasto)
    {
        if (id != gasto.Id) return BadRequest();

        _context.Entry(gasto).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE
    [HttpDelete("EliminarGasto/{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _context.Gastos.FindAsync(id);
        if (producto == null) return NotFound();

        _context.Gastos.Remove(producto);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
