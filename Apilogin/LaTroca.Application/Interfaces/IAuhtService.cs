using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorneoUniversitario.Application.DTOs;
using TorneoUniversitario.Domain.Entities;

namespace TorneoUniversitario.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginWithGoogleAsync(string googleIdToken);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task RegisterAsync(RegisterRequest request);
        Task LogoutAsync();
        Task DeactivateAccountAsync(string userId, string reason); // 👈 Nuevo
    }
}
