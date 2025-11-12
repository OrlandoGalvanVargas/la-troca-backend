using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTroca.Application.DTOs
{
    public class AdminUpdateUserRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; } // ADMIN o USER
        public string? Status { get; set; } // active, deactivated, suspended
        public string? Bio { get; set; }
    }
}
