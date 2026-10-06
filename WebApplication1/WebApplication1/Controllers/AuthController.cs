using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;

public class LoginDto
{
    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}

public class RestablecerDto
{
    public string Correo { get; set; } = string.Empty;
    public string NuevaContrasena { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        if (string.IsNullOrEmpty(loginDto.Correo) || string.IsNullOrEmpty(loginDto.Contrasena))
            return BadRequest(new { mensaje = "Correo y contraseña son requeridos" });

        // Buscar usuario por correo
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                u.Correo == loginDto.Correo.ToLower().Trim() &&
                u.Activo);

        if (usuario == null)
            return Unauthorized(new { mensaje = "Credenciales incorrectas" });

        // Verificar contraseña con BCrypt
        bool passwordValida = BCrypt.Net.BCrypt.Verify(
            loginDto.Contrasena,
            usuario.Contrasena
        );

        if (!passwordValida)
            return Unauthorized(new { mensaje = "Credenciales incorrectas" });

        // Generar JWT
        var token = GenerarToken(usuario);

        return Ok(new
        {
            token,
            usuario = new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Correo
            }
        });
    }

    // Restablece la contraseña solo con el correo (sin token previo).
    // Pensado para uso personal; no usar así en apps multiusuario
    // sin verificación adicional (token por email, etc.).
    [HttpPost("restablecer")]
    [AllowAnonymous]
    public async Task<IActionResult> Restablecer(RestablecerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Correo) || string.IsNullOrEmpty(dto.NuevaContrasena))
            return BadRequest(new { mensaje = "Correo y nueva contraseña son requeridos" });

        if (dto.NuevaContrasena.Length < 6)
            return BadRequest(new { mensaje = "La contraseña debe tener al menos 6 caracteres" });

        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                u.Correo == dto.Correo.ToLower().Trim() &&
                u.Activo);

        if (usuario == null)
            return NotFound(new { mensaje = "No existe una cuenta activa con ese correo" });

        usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(dto.NuevaContrasena);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = "Contraseña actualizada. Inicia sesión con tu nueva contraseña." });
    }

    private string GenerarToken(Usuarios usuario)
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("JWT Secret no configurado.");
        var key = Encoding.ASCII.GetBytes(secret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(ClaimTypes.Name, usuario.Nombre)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(
                int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60")
            ),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}