using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using BCrypt.Net;

public class UsuarioDto
{
    public string Nombre { get; set; }
    public int Edad { get; set; }
    public string Genero { get; set; }
    public string Correo { get; set; }
    public long Celular { get; set; }
    public string Contrasena { get; set; }
    public DateTime FechaRegistro { get; set; }
    public bool Activo { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // CREATE
    [HttpPost("CrearUsuario")]
    public async Task<IActionResult> Crear(UsuarioDto user)
    {
        // Verificar si el correo ya existe
        var existe = await _context.Usuarios
            .AnyAsync(u => u.Correo == user.Correo);
        if (existe)
            return BadRequest(new { mensaje = "El correo ya está registrado" });

        var usuario = new Usuario
        {
            Nombre = user.Nombre,
            Genero = user.Genero,
            Edad = user.Edad,
            // ✅ Hash de contraseña con BCrypt
            Contrasena = BCrypt.Net.BCrypt.HashPassword(user.Contrasena),
            Correo = user.Correo.ToLower().Trim(),
            Celular = user.Celular,
            FechaRegistro = DateTime.UtcNow,
            Activo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // No retornar la contraseña
        usuario.Contrasena = string.Empty;
        return Ok(usuario);
    }

    // READ ALL
    [HttpGet("ObtenerUsuarios")]
    public async Task<IActionResult> ObtenerUsuarios()
    {
        var usuarios = await _context.Usuarios
            .Select(u => new
            {
                u.Id,
                u.Nombre,
                u.Correo,
                u.Edad,
                u.Genero,
                u.Celular,
                u.FechaRegistro,
                u.Activo
                // ✅ Nunca retornar Contrasena
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    // READ BY ID
    [HttpGet("ObtenerUsuarioporID/{id}")]
    public async Task<IActionResult> ObtenerUsuarioporID(int id)
    {
        var usuario = await _context.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Nombre,
                u.Correo,
                u.Edad,
                u.Genero,
                u.Celular,
                u.FechaRegistro,
                u.Activo
            })
            .FirstOrDefaultAsync();

        if (usuario == null) return NotFound();
        return Ok(usuario);
    }

    // UPDATE
    [HttpPut("EditarUsuario/{id}")]
    public async Task<IActionResult> ActualizarUsuarios(int id, UsuarioDto usuarioDto)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        usuario.Nombre = usuarioDto.Nombre;
        usuario.Genero = usuarioDto.Genero;
        usuario.Edad = usuarioDto.Edad;
        usuario.Correo = usuarioDto.Correo.ToLower().Trim();
        usuario.Celular = usuarioDto.Celular;

        // Solo actualizar contraseña si se envía una nueva
        if (!string.IsNullOrEmpty(usuarioDto.Contrasena))
            usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(usuarioDto.Contrasena);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE (desactivar en lugar de eliminar)
    [HttpDelete("EliminarUsuario/{id}")]
    public async Task<IActionResult> EliminarUsuario(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound();

        // ✅ Soft delete: desactivar en lugar de eliminar
        usuario.Activo = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}