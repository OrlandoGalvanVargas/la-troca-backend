using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using LaTroca.Application.DTOs;
using LaTroca.Application.Interfaces;
using LaTroca.Application.Services;
using LaTroca.Domain.Interfaces;
using LaTroca.Infrastructure;
using LaTroca.Infrastructure.Data;
using LaTroca.Infrastructure.Repositories;
using LaTroca.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TorneoUniversitario.Application.Interfaces;
using TorneoUniversitario.Application.Services;
using TorneoUniversitario.Domain.Interfaces;
using TorneoUniversitario.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);
// Ruta absoluta o relativa al archivo .json descargado
var credentialPath = Path.Combine(AppContext.BaseDirectory, "la-troca-ed2d2-firebase-adminsdk-fbsvc-efc751c72d.json");

// Cargar las credenciales desde el archivo
var credential = GoogleCredential.FromFile(credentialPath);

// Crear el cliente Firestore con esas credenciales
var firestoreBuilder = new FirestoreDbBuilder
{
    ProjectId = "la-troca-ed2d2", // 🔥 tu project ID de Firebase
    Credential = credential
};

var firestoreDb = firestoreBuilder.Build();
Console.WriteLine($"✅ Firestore conectado: {firestoreDb.ProjectId}");

builder.Services.AddSingleton(firestoreDb);
builder.Services.AddSingleton<INotificationService, NotificationService>();
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddInfrastructure();

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Latroca", Version = "v1" });

    // Configure JWT authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field (e.g., 'Bearer {token}')",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//dbcontext
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings")
);


// Register repositories and services
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();



// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "La Troca v1");
    c.RoutePrefix = string.Empty; // Swagger disponible en la raíz (e.g., http://localhost:5001)
});

app.MapControllers();

app.Run();
public partial class Program { }
