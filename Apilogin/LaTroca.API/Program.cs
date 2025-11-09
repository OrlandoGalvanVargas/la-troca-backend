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
// ===================== 🔥 FIREBASE SETUP =====================
GoogleCredential credential;

// 1️⃣ Si existe la variable de entorno (Render o GitHub Actions)
var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
if (!string.IsNullOrEmpty(firebaseJson))
{
    Console.WriteLine("✅ Cargando credenciales de Firebase desde variable de entorno...");
    credential = GoogleCredential.FromJson(firebaseJson);
}
else
{
    // 2️⃣ Si no existe, usar el archivo local (para desarrollo)
    var credentialPath = Path.Combine(AppContext.BaseDirectory, "la-troca-ed2d2-firebase-adminsdk-fbsvc-67a0cf6df5.json");
    Console.WriteLine($"✅ Cargando credenciales de Firebase desde archivo local: {credentialPath}");

    if (!File.Exists(credentialPath))
    {
        Console.WriteLine($"❌ ERROR: No se encontró el archivo de credenciales en: {credentialPath}");
        throw new FileNotFoundException("Archivo de credenciales Firebase no encontrado", credentialPath);
    }

    credential = GoogleCredential.FromFile(credentialPath);
}

// 🔥 Inicializar Firebase Admin SDK (para FCM)
try
{
    if (FirebaseAdmin.FirebaseApp.DefaultInstance == null)
    {
        FirebaseAdmin.FirebaseApp.Create(new FirebaseAdmin.AppOptions
        {
            Credential = credential,
            ProjectId = "la-troca-ed2d2"
        });
        Console.WriteLine("✅ Firebase Admin SDK inicializado correctamente");
    }
    else
    {
        Console.WriteLine("ℹ️ Firebase Admin SDK ya estaba inicializado");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERROR al inicializar Firebase Admin SDK: {ex.Message}");
    throw;
}

// 🔥 Crear cliente Firestore
var firestoreBuilder = new FirestoreDbBuilder
{
    ProjectId = "la-troca-ed2d2",
    Credential = credential
};

var firestoreDb = firestoreBuilder.Build();
Console.WriteLine($"✅ Firestore conectado OK: {firestoreDb.ProjectId}");

// ============================================================

builder.Services.AddSingleton(firestoreDb);
builder.Services.AddSingleton<INotificationService, NotificationService>();

// ... resto de tu código ...
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
