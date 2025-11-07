using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorneoUniversitario.Application.DTOs;
using TorneoUniversitario.Domain.Entities;

namespace LaTroca.Application.DTOs
{

    public class UpdateProfileRequest
    {
        public string? Nombre { get; set; }
        public string? Bio { get; set; }
        public IFormFile? ImagenPerfil { get; set; }

        // CAMPOS PLANOS para Location
        public double? LocationLatitude { get; set; }
        public double? LocationLongitude { get; set; }
        public string? LocationManual { get; set; }

        // Método para crear Location solo si hay datos
        public Location? GetLocation()
        {
            if (LocationLatitude.HasValue && LocationLongitude.HasValue)
            {
                return new Location
                {
                    Latitude = LocationLatitude.Value,
                    Longitude = LocationLongitude.Value,
                    Manual = LocationManual ?? string.Empty
                };
            }
            return null;
        }
    }
}
