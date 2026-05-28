using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// SERVICIOS
// ============================================
builder.Services.AddScoped<GastoesService>();
builder.Services.AddScoped<IngresoesService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger solo en desarrollo
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen();
}

// ============================================
// BASE DE DATOS
// ============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<Gastodbcontext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<IngresoDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<UsuarioDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<PerfilFinancieroDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============================================
// JWT AUTENTICACIÓN
// ============================================
var jwtSecret = builder.Configuration["Jwt:Secret"];

if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
{
    throw new Exception("JWT Secret inválido. Debe tener mínimo 32 caracteres.");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ============================================
// CORS SEGURO
// ============================================
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("BalanceProPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ============================================
// BUILD
// ============================================
var app = builder.Build();

// ============================================
// MIGRACIONES
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        services.GetRequiredService<UsuarioDbContext>().Database.Migrate();
        services.GetRequiredService<Gastodbcontext>().Database.Migrate();
        services.GetRequiredService<IngresoDbContext>().Database.Migrate();
        services.GetRequiredService<PerfilFinancieroDbContext>().Database.Migrate();
        logger.LogInformation("✅ Migraciones aplicadas correctamente");
    }
    catch (Exception ex)
    {
        logger.LogWarning("⚠️ Error en migraciones: {Message}", ex.Message);
    }
}

// ============================================
// PIPELINE (ORDEN IMPORTANTE)
// ============================================

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS
if (app.Environment.IsProduction())
{
    app.UseHsts();
}
app.UseHttpsRedirection();

// CORS antes de Authentication
app.UseCors("BalanceProPolicy");

// Auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();