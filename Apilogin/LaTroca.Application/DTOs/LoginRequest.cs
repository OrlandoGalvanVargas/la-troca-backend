using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorneoUniversitario.Application.DTOs
{
    public record LoginRequest { 
        public string Email { get; init; } = string.Empty; 
        public string Password { get; init; } = string.Empty; }
}
