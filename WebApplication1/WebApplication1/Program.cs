using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Render termina TLS en su proxy y habla HTTP con el contenedor:
// confiar X-Forwarded-Proto/For para que auth y redirects vean el esquema real.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

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
// BASE DE DATOS (SQL Server local / PostgreSQL en prod)
// Prod (Render) usa DATABASE_URL o ConnectionStrings__DefaultConnection
// con formato postgres:// o Host=...; acepta ambos.
// ============================================
static string ToNpgsqlConnectionString(string raw)
{
    if (raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(raw);
        // Las credenciales en la URL pueden venir percent-encoded: decodificarlas
        var userInfo = Uri.UnescapeDataString(uri.UserInfo).Split(':', 2);
        var db = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        // sslmode via query (?sslmode=require) o Prefer por defecto:
        // negocia SSL si el servidor lo exige (Render externo) y baja a
        // texto plano en red privada interna donde no hay TLS.
        var sslmode = "Prefer";
        var q = uri.Query.TrimStart('?');
        foreach (var part in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 && kv[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase)
                && kv[1].Length > 0)
                sslmode = kv[1];
        }
        return $"Host={uri.Host};Port={uri.Port};Database={db};Username={userInfo[0]};Password={(userInfo.Length > 1 ? userInfo[1] : "")};SslMode={sslmode};Trust Server Certificate=true";
    }
    return raw;
}

static bool IsNpgsqlConnectionString(string cs) =>
    cs.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
    cs.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
    cs.Contains("Host=", StringComparison.OrdinalIgnoreCase);

var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string no configurada.");

if (IsNpgsqlConnectionString(connectionString))
{
    var npgsqlCs = ToNpgsqlConnectionString(connectionString);
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(npgsqlCs));
}
else
{
    // Azure SQL serverless se pausa con inactividad: reintentos ante
    // transitorios (incluido el wake-up) y timeout amplio para el resume.
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, sql => sql
            .EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null)
            .CommandTimeout(60)));
}

// ============================================
// JWT (env JWT_SECRET tiene prioridad; no commitear el secreto real)
// ============================================
var jwtSecret =
    Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Jwt:Secret"];

if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
    throw new Exception("JWT Secret inválido. Mínimo 32 caracteres.");

// En Production el secreto debe venir del entorno, nunca del placeholder commiteado
if (builder.Environment.IsProduction() &&
    Environment.GetEnvironmentVariable("JWT_SECRET") is not { Length: >= 32 })
    throw new Exception("JWT_SECRET no configurado en el entorno de producción.");

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
// Normaliza orígenes CORS (sin slash final: el Origin del browser nunca lo trae)
var allowedOrigins = (builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:4200" })
    .Select(o => o.Trim().TrimEnd('/'))
    .Where(o => o.Length > 0)
    .ToArray();

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
// Primero: headers del proxy (Render) antes que HSTS/redirecciones.
app.UseForwardedHeaders();

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