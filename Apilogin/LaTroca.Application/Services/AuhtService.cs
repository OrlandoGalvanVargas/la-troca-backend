using BCrypt.Net;
using Google.Apis.Auth;
using LaTroca.Application.DTOs;
using LaTroca.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TorneoUniversitario.Application.DTOs;
using TorneoUniversitario.Application.Interfaces;
using TorneoUniversitario.Domain.Entities;
using TorneoUniversitario.Domain.Interfaces;

namespace TorneoUniversitario.Application.Services
{
    public class AuthService : IAuthService
    {

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly JwtSettings _jwtSettings;
        private static readonly string[] ValidRoles = { "ADMIN", "USER" };

        public AuthService(IUsuarioRepository usuarioRepository, IOptions<JwtSettings> jwtSettings, ICloudinaryService cloudinaryService)
        {
            _usuarioRepository = usuarioRepository;
            _jwtSettings = jwtSettings.Value;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<LoginResponse> LoginWithGoogleAsync(string googleIdToken)
        {
            // 1. Validar el token de Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken);
            if (payload == null)
                throw new UnauthorizedAccessException("Token de Google inválido.");

            var email = payload.Email;

            // 2. Buscar usuario
            var usuario = await _usuarioRepository.GetByEmailAsync(email);
            if (usuario == null)
                throw new UnauthorizedAccessException("Usuario no registrado.");

            // 3. Verificar status (sin envolver en otro mensaje)
            if (!string.Equals(usuario.Status, "active", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Tu cuenta está inactiva o ha sido suspendida. Contacta con el administrador.");  // 👈 MISMO mensaje que login normal

            // 4. Generar token JWT
            var token = GenerateJwtToken(usuario);

            return new LoginResponse
            {
                Token = token,
                Rol = usuario.Role,
                UserId = usuario.Id
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("El email y la contraseña son obligatorios.");

            var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            if (!string.Equals(usuario.Status, "active", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Tu cuenta está inactiva o ha sido suspendida. Contacta con el administrador.");

            var token = GenerateJwtToken(usuario);
            return new LoginResponse
            {
                Token = token,
                Rol = usuario.Role,
                UserId = usuario.Id // Corregir para incluir userId
            };
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("El email es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("La contraseña es obligatoria.");
            if (request.Password.Length < 8)
                throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");
            if (string.IsNullOrWhiteSpace(request.Rol))
                throw new ArgumentException("El rol es obligatorio.");

            if (!IsValidEmail(request.Email))
                throw new ArgumentException("El formato del email es inválido.");
            if (!ValidRoles.Contains(request.Rol.ToUpper()))
                throw new ArgumentException("Rol no existente. Válidos: ADMIN, USER.");

            var existingUser = await _usuarioRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new ArgumentException("El email ya está registrado.");

            string imageUrl = string.Empty;
            if (request.ImagenPerfil != null)
                imageUrl = await _cloudinaryService.UploadImageAsync(request.ImagenPerfil, request.Email);

            var usuario = new Usuario
            {
                Name = request.Nombre,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Rol.ToUpper(),
                Bio = request.Bio,
                ProfilePicUrl = imageUrl,
                Location = request.Location != null
                    ? new Location
                    {
                        Latitude = request.Location.Latitude,
                        Longitude = request.Location.Longitude,
                        Manual = request.Location.Manual
                    }
                    : null,
                TermsAccepted = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "active"
            };

            await _usuarioRepository.AddAsync(usuario);
        }

        public async Task<UserProfileResponse> GetUserProfileAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("El ID de usuario es obligatorio.");

            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado.");

            return new UserProfileResponse
            {
                Id = usuario.Id,
                Name = usuario.Name,
                Email = usuario.Email,
                ProfilePicUrl = usuario.ProfilePicUrl ?? "",
                Bio = usuario.Bio ?? "",
                Role = usuario.Role,
                Location = usuario.Location != null ? new LocationDto
                {
                    Latitude = usuario.Location.Latitude,
                    Longitude = usuario.Location.Longitude,
                    Manual = usuario.Location.Manual
                } : null,
                Reputation = 0, // 👈 Valor por defecto
                CreatedAt = usuario.CreatedAt
            };
        }
        public async Task LogoutAsync()
        {
            await Task.CompletedTask;
        }

        private bool IsValidEmail(string email)
        {
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, usuario.Role),
                new Claim("userId", usuario.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task DeactivateAccountAsync(string userId, string reason)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("El ID de usuario es obligatorio.");

            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado.");

            if (usuario.Status != "active")
                throw new InvalidOperationException("La cuenta ya está desactivada.");

            // Actualizar el estado a "deactivated"
            usuario.Status = "deactivated";
            usuario.UpdatedAt = DateTime.UtcNow;

            // Opcional: Guardar el motivo y fecha de desactivación
            // Puedes agregar estos campos al modelo Usuario si quieres
            // usuario.DeactivationReason = reason;
            // usuario.DeactivationDate = DateTime.UtcNow;
            // usuario.ScheduledDeletionDate = DateTime.UtcNow.AddDays(30);

            await _usuarioRepository.UpdateAsync(usuario);
        }
    }


public class JwtSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
    }
}
