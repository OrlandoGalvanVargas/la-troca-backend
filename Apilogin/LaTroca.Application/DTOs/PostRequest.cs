using Microsoft.AspNetCore.Http;
using TorneoUniversitario.Domain.Entities;

namespace TorneoUniversitario.Application.DTOs
{
    public class PostRequest
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public IFormFile[] Fotos { get; set; } = Array.Empty<IFormFile>();
        public Location? Ubicacion { get; set; }
        public string Necesidad { get; set; } = string.Empty;
    }

    public class PostResponse
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string[] FotosUrl { get; set; } = Array.Empty<string>();

        // ✅ Cambiar a UbicacionDto en lugar de Location
        public UbicacionDto? Ubicacion { get; set; }

        public string Necesidad { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public string Estado { get; set; } = "activo";

        // 🆕 Datos del usuario propietario del post
        public UserBasicInfo? UserInfo { get; set; }
    }

    // 🆕 Información básica del usuario
    public class UserBasicInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProfileImageUrl { get; set; } = string.Empty;
    }

    // DTO para ubicación (separado de la entidad de dominio)
    public class UbicacionDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Manual { get; set; } = string.Empty;
    }
}