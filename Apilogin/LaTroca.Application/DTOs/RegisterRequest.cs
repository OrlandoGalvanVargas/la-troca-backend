using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorneoUniversitario.Application.DTOs
{
    public record RegisterRequest
    {
        public string Nombre { get; init; } = string.Empty; // name
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string Rol { get; init; } = string.Empty;
        public string Bio { get; init; } = string.Empty;

    }
}
