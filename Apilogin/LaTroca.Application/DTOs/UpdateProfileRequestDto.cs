// Archivo: LaTroca.Application/DTOs/UpdateProfileRequestDto.cs
using Microsoft.AspNetCore.Http;

namespace LaTroca.Application.DTOs
{
    // LaTroca.Application/DTOs/UpdateProfileRequestDto.cs
        public class UpdateProfileRequestDto
        {
            public string? Nombre { get; set; }
            public string? Bio { get; set; }
            public IFormFile? ImagenPerfil { get; set; }  // ← SOLO ESTO sube el cliente

            public double? Latitude { get; set; }
            public double? Longitude { get; set; }
            public string? Manual { get; set; }
        }
    
}