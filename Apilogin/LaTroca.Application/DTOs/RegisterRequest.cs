using Microsoft.AspNetCore.Http;

namespace TorneoUniversitario.Application.DTOs
{
    public record RegisterRequest
    {
        public string Nombre { get; init; } = string.Empty; // name
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string Rol { get; init; } = string.Empty;
        public string Bio { get; init; } = string.Empty;
        public IFormFile? ImagenPerfil { get; init; }

        public LocationRequest? Location { get; init; } // Nuevo campo para ubicación
    }

    public record LocationRequest
    {
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public string Manual { get; init; } = string.Empty;
    }
}
