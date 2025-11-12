using LaTroca.Application.DTOs;
using LaTroca.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TorneoUniversitario.Application.Interfaces;

namespace TorneoUniversitario.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IImageModerationService _imageModerationService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ITextModerationServices _textModerationServices; // AÑADIDO

        public UserController(
            IAuthService authService,
            IImageModerationService imageModerationService,
            ICloudinaryService cloudinaryService,
            ITextModerationServices textModerationServices)
        {
            _authService = authService;
            _imageModerationService = imageModerationService;
            _cloudinaryService = cloudinaryService;
            _textModerationServices = textModerationServices;
        }

        /// <summary>
        /// Obtiene el perfil público de un usuario (desde una publicación)
        /// </summary>
        [HttpGet("profile/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileResponse>> GetUserProfile(string userId)
        {
            try
            {
                var profile = await _authService.GetUserProfileAsync(userId);
                return Ok(new { Message = "Perfil obtenido correctamente.", Data = profile });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en GetUserProfile: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserProfileResponse>> GetMyProfile()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Token inválido o userId no encontrado." });

            try
            {
                var profile = await _authService.GetUserProfileAsync(userId);
                return Ok(new { Message = "Tu perfil ha sido cargado.", Data = profile });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Actualiza el perfil del usuario autenticado
        /// </summary>
        [HttpPut("me")]
[Authorize]
[Consumes("multipart/form-data")]
public async Task<ActionResult> UpdateMyProfile([FromForm] UpdateProfileRequest request)
{
    var userId = User.FindFirst("userId")?.Value;
    if (string.IsNullOrEmpty(userId))
        return Unauthorized(new { Message = "Token inválido." });

    try
    {
                // === MODERACIÓN DE TEXTO: NOMBRE Y BIO ===
                if (!string.IsNullOrWhiteSpace(request.Nombre) &&
                    !await _textModerationServices.IsTextSafeAsync(request.Nombre))
                {
                    return BadRequest(new { Message = "El nombre contiene lenguaje inapropiado." });
                }

                if (!string.IsNullOrWhiteSpace(request.Bio) &&
                    !await _textModerationServices.IsTextSafeAsync(request.Bio))
                {
                    return BadRequest(new { Message = "La biografía contiene lenguaje inapropiado." });
                }
                // Moderar imagen si se sube


                // Pasar la request tal como está (con campos planos)
                await _authService.UpdateUserProfileAsync(userId, request);
        return Ok(new { Message = "Perfil actualizado correctamente." });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR en UpdateMyProfile: {ex.Message}\n{ex.StackTrace}");
        return StatusCode(500, new { Message = "Error interno del servidor." });
    }
}
        /// <summary>
        /// Cambia la contraseña del usuario autenticado (sin pedir la actual)
        /// </summary>
        [HttpPut("me/password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordSimpleRequest request)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { Message = "Token inválido." });

            try
            {
                await _authService.ChangePasswordAsync(userId, request);
                return Ok(new { Message = "Contraseña actualizada correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en ChangePassword: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }
    }
}