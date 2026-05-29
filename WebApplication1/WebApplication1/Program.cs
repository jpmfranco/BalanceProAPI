using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen();
}

// ============================================
// BASE DE DATOS (UN SOLO DBCONTEXT)
// ============================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ============================================
// JWT
// ============================================
var jwtSecret = builder.Configuration["Jwt:Secret"];

if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
    throw new Exception("JWT Secret inválido. Mínimo 32 caracteres.");

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
                context.Response.Headers.Append("Token-Expired", "true");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ============================================
// CORS
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
// MIGRACIONES (UN SOLO DBCONTEXT)
// ============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        logger.LogInformation("✅ Migraciones aplicadas correctamente");
    }
    catch (Exception ex)
    {
        logger.LogWarning("⚠️ Error en migraciones: {Message}", ex.Message);
    }
}

// ============================================
// PIPELINE
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseCors("BalanceProPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();