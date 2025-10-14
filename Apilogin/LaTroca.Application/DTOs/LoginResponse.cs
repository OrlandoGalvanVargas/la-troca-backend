using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorneoUniversitario.Application.DTOs
{
    public record LoginResponse { 
        public string Token { get; init; } = string.Empty; 
        public string Rol { get; init; } = string.Empty; }
}
