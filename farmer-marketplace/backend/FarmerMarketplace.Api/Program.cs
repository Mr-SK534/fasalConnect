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

// ==========================================
// 1. CONTROLLERS & SWAGGER CONFIGURATION
// ==========================================
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
        Description = "Enter your JWT token like this: **Bearer <your token>**"
    });
});

// ==========================================
// 2. DATABASE & HTTP CLIENTS
// ==========================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// General HTTP client factory
builder.Services.AddHttpClient();

// Dedicated HTTP client for WhatsApp Baileys gateway service with timeout
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(8);
});

// ==========================================
// 3. SECURITY & PLATFORM HELPERS
// ==========================================
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<IPlatformConfigService, PlatformConfigService>();

// ==========================================
// 4. DOMAIN APPLICATION SERVICES
// ==========================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFpoService, FpoService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Orders & Payments (with WhatsApp notifications)
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentEscrowService, PaymentEscrowService>();
builder.Services.AddScoped<IReportingService, ReportingService>();

// Route Optimization & Background Dispatch Batcher
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddSingleton<RouteBatchingService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<RouteBatchingService>());

// Machine Learning Forecasting Service
builder.Services.AddScoped<IForecastService, ForecastService>();

// ==========================================
// 5. JWT AUTHENTICATION & TOKEN BLOCKLIST
// ==========================================
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

// ==========================================
// 6. CORS POLICY FOR FRONTEND
// ==========================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ==========================================
// 7. DATABASE MIGRATIONS & SCHEMA PREPARATION
// ==========================================
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

// ==========================================
// 8. MIDDLEWARE PIPELINE
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();