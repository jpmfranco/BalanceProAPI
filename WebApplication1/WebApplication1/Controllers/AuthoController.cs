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
    public string Correo { get; set; }
    public string Contrasena { get; set; }
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

    private string GenerarToken(Usuario usuario)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

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