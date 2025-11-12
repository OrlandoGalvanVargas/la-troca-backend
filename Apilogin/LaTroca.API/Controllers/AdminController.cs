using LaTroca.Application.DTOs;
using LaTroca.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TorneoUniversitario.Application.Interfaces;
using TorneoUniversitario.Domain.Interfaces;

namespace TorneoUniversitario.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUsuarioRepository _usuarioRepository;

        public AdminController(IAuthService authService, IUsuarioRepository usuarioRepository)
        {
            _authService = authService;
            _usuarioRepository = usuarioRepository;
        }

        /// <summary>
        /// Obtiene todos los usuarios (solo ADMIN)
        /// </summary>
        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<AdminUserResponse>>> GetAllUsers()
        {
            try
            {
                var usuarios = await _usuarioRepository.GetAllAsync();
                var response = usuarios.Select(u => new AdminUserResponse
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role,
                    Status = u.Status,
                    ProfilePicUrl = u.ProfilePicUrl ?? "",
                    Bio = u.Bio ?? "",
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                }).ToList();

                return Ok(new { Message = "Usuarios obtenidos correctamente.", Data = response });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en GetAllUsers: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Obtiene un usuario por ID (solo ADMIN)
        /// </summary>
        [HttpGet("users/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AdminUserResponse>> GetUserById(string id)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                if (usuario == null)
                    return NotFound(new { Message = "Usuario no encontrado." });

                var response = new AdminUserResponse
                {
                    Id = usuario.Id,
                    Name = usuario.Name,
                    Email = usuario.Email,
                    Role = usuario.Role,
                    Status = usuario.Status,
                    ProfilePicUrl = usuario.ProfilePicUrl ?? "",
                    Bio = usuario.Bio ?? "",
                    CreatedAt = usuario.CreatedAt,
                    UpdatedAt = usuario.UpdatedAt
                };

                return Ok(new { Message = "Usuario obtenido correctamente.", Data = response });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en GetUserById: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Actualiza un usuario (rol, estado, etc.) - Solo ADMIN
        /// </summary>
        [HttpPut("users/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] AdminUpdateUserRequest request)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                if (usuario == null)
                    return NotFound(new { Message = "Usuario no encontrado." });

                // Validar rol
                if (!string.IsNullOrWhiteSpace(request.Role) && !new[] { "ADMIN", "USER" }.Contains(request.Role.ToUpper()))
                    return BadRequest(new { Message = "Rol inválido. Solo: ADMIN, USER." });

                // Validar estado
                if (!string.IsNullOrWhiteSpace(request.Status) && !new[] { "active", "deactivated", "suspended" }.Contains(request.Status.ToLower()))
                    return BadRequest(new { Message = "Estado inválido. Solo: active, deactivated, suspended." });

                // Actualizar campos
                if (!string.IsNullOrWhiteSpace(request.Name))
                    usuario.Name = request.Name;

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        return BadRequest(new { Message = "Email inválido." });
                    usuario.Email = request.Email;
                }

                if (!string.IsNullOrWhiteSpace(request.Role))
                    usuario.Role = request.Role.ToUpper();

                if (!string.IsNullOrWhiteSpace(request.Status))
                    usuario.Status = request.Status.ToLower();

                if (request.Bio != null)
                    usuario.Bio = request.Bio;

                usuario.UpdatedAt = DateTime.UtcNow;
                await _usuarioRepository.UpdateAsync(usuario);

                return Ok(new { Message = "Usuario actualizado correctamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en UpdateUser: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Elimina (o desactiva permanentemente) un usuario - Solo ADMIN
        /// </summary>
        [HttpDelete("users/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                var usuario = await _usuarioRepository.GetByIdAsync(id);
                if (usuario == null)
                    return NotFound(new { Message = "Usuario no encontrado." });

                // Opción: eliminar de la base
                await _usuarioRepository.DeleteAsync(id);

                // O bien: marcar como "deleted"
                // usuario.Status = "deleted";
                // usuario.UpdatedAt = DateTime.UtcNow;
                // await _usuarioRepository.UpdateAsync(usuario);

                return Ok(new { Message = "Usuario eliminado correctamente." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en DeleteUser: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }
    }
}