using BCrypt.Net;
using LaTroca.Application.Interfaces;
using LaTroca.Application.Services;
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

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("El email y la contraseña son obligatorios.");

            var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas.");

            var token = GenerateJwtToken(usuario);
            return new LoginResponse { Token = token, Rol = usuario.Role };
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
                ProfilePicUrl = imageUrl, // nuevo campo
                TermsAccepted = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = "active"
            };

            await _usuarioRepository.AddAsync(usuario);
        }

        public async Task LogoutAsync()
        {
            // En JWT, el logout es responsabilidad del cliente (borrar token del localStorage o header).
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
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Email),
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
    }

    public class JwtSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
    }
}
