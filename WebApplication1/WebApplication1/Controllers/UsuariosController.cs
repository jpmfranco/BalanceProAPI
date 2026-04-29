using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

public class UsuarioDto
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    public string Genero { get; set; }
    public string Correo { get; set; }
    public int Celular { get; set; }
    public string Contrasena { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioDbContext _context;


    public UsuariosController(UsuarioDbContext context)
    {
        _context = context;
    }

    // CREATE
    [HttpPost("CrearUsuario")]
    public async Task<IActionResult> Crear(UsuarioDto user)
    {
        var usuario = new Usuario
        {
            Nombre = user.Nombre,
            Genero = user.Genero,
            Edad = user.Edad,
            Contrasena = user.Contrasena,
            Correo = user.Correo,
            Celular = user.Celular,
            FechaRegistro = user.FechaRegistro,
            Activo = user.Activo
        };
        _context.Usuario.Add(usuario);
        await _context.SaveChangesAsync();
        return Ok(usuario);
    }

    // READ ALL
    [HttpGet("ObtenerUsuarios")]
    public async Task<IActionResult> ObtenerUsuarios()
    {
        return Ok(await _context.Usuario.ToListAsync());
    }

    // READ BY ID
    [HttpGet("ObtenerUsuarioporID/{id}")]
    public async Task<IActionResult> ObtenerUsuarioporID(int id)
    {
        var usuario = await _context.Usuario.FindAsync(id);
        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    // UPDATE
    [HttpPut("EditarUsuario/{id}")]
    public async Task<IActionResult> ActualizarUsuarios(int id, Usuario usuario)
    {
        try
        {
            if (id != usuario.Id) return BadRequest();

            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }catch(Exception ex)
        {
            return BadRequest(ex);
        }
        
    }

    // DELETE
    [HttpDelete("EliminarUsuario/{id}")]
    public async Task<IActionResult> EliminarUsuario(int id)
    {
        var usuario = await _context.Usuario.FindAsync(id);
        if (usuario== null) return NotFound();

        _context.Usuario.Remove(usuario);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}