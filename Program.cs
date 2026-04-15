using Pomolog.Api.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- KONFIGURASI DATABASE ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- KONFIGURASI CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// --- KONFIGURASI JWT AUTHENTICATION ---
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSecret = jwtSection["Secret"];
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

if (string.IsNullOrWhiteSpace(jwtSecret) ||
    string.IsNullOrWhiteSpace(jwtIssuer) ||
    string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("JwtSettings must be configured.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!))
        };
    });

// --- KONFIGURASI OPENAPI ---
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
        });

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
        return Task.CompletedTask;
    });
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// --- MIDDLEWARE ---
app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --- MIGRASI DATABASE OTOMATIS SAAT APLIKASI JALAN ---
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.Run();