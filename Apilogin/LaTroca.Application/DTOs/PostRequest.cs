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
        public Location? Ubicacion { get; set; }
        public string Necesidad { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
        public DateTime ActualizadoEn { get; set; }
        public string Estado { get; set; } = "activo";
    }
}