// backend/FarmerMarketplace.Api/Program.cs

using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Middleware;
using FarmerMarketplace.Api.Security;
using FarmerMarketplace.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FarmerMarketplace API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token like this: **Bearer &lt;your token&gt;**"
    });
});

// Database connection string parsing (supports standard Key-Value & Render postgres:// or postgresql:// URLs)
var rawConnStr = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["DATABASE_URL"] 
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString = ParsePostgresConnectionString(rawConnStr);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// Security helpers
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<JwtService>();

builder.Services.AddSingleton<IPlatformConfigService, PlatformConfigService>();
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFpoService, FpoService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IPaymentEscrowService, PaymentEscrowService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<RouteService>();
builder.Services.AddSingleton<RouteBatchingService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<RouteBatchingService>());
builder.Services.AddScoped<IForecastService, ForecastService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (jti == null) return;

            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var isBlocked = await db.TokenBlocklist.AnyAsync(t => t.Jti == jti);

            if (isBlocked)
                context.Fail("Token has been logged out.");
        }
    };
});

builder.Services.AddAuthorization();

// CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var originsConfig = builder.Configuration["AllowedOrigins"];
        var configuredOrigins = string.IsNullOrWhiteSpace(originsConfig)
            ? new[] { "http://localhost:5173", "http://localhost:3000" }
            : originsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        policy.WithOrigins(configuredOrigins)
              .SetIsOriginAllowed(origin =>
                  string.IsNullOrEmpty(origin) ||
                  origin.StartsWith("http://localhost:") ||
                  origin.EndsWith(".vercel.app") ||
                  configuredOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Ensure database tables exist on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    // Ensure SalesHistories table exists in PostgreSQL
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ""SalesHistories"" (
            ""Id"" uuid NOT NULL PRIMARY KEY,
            ""CropName"" character varying(100) NOT NULL,
            ""Category"" character varying(50) NOT NULL DEFAULT '',
            ""Region"" character varying(100) NOT NULL DEFAULT '',
            ""FarmerId"" uuid NULL,
            ""Date"" timestamp with time zone NOT NULL,
            ""QuantitySoldKg"" real NOT NULL,
            ""AveragePricePerKg"" real NOT NULL,
            ""CreatedAt"" timestamp with time zone NOT NULL
        );
    ");
}

// Enable Swagger UI in both Dev and Production for easy testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FarmerMarketplace API v1");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Helper method to parse PostgreSQL URI strings (e.g. postgres://user:pass@host:5432/db)
static string ParsePostgresConnectionString(string? rawConnStr)
{
    if (string.IsNullOrWhiteSpace(rawConnStr))
    {
        throw new InvalidOperationException("PostgreSQL Connection String is missing. Please set ConnectionStrings__DefaultConnection or DATABASE_URL environment variable.");
    }

    rawConnStr = rawConnStr.Trim();

    if (rawConnStr.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        rawConnStr.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(rawConnStr);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    }

    return rawConnStr;
}