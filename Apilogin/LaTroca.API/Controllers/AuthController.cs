using LaTroca.Application.DTOs;
using LaTroca.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TorneoUniversitario.Application.DTOs;
using TorneoUniversitario.Application.Interfaces;

namespace TorneoUniversitario.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IImageModerationService _imageModerationService;

        public AuthController(IAuthService authService, IImageModerationService imageModerationService)
        {
            _authService = authService;
            _imageModerationService = imageModerationService;
        }

        [HttpPost("login-google")]
        public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequest request)
        {
            try
            {
                var response = await _authService.LoginWithGoogleAsync(request.IdToken);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // DTO
        public class GoogleLoginRequest
        {
            public string IdToken { get; set; }
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(new { Message = "Inicio de sesión exitoso.", Data = response });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"🔥 ERROR en AuthController: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"🔥 ERROR en AuthController: {ex.Message}");
                return Unauthorized(new { Message = ex.Message });
            }
        }
        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Register([FromForm] RegisterRequest request)
        {
            try
            {
             
                if (request.ImagenPerfil != null && request.ImagenPerfil.Length > 0)
                {
                    var moderationResult = await _imageModerationService.AnalyzeImageAsync(request.ImagenPerfil);

                    if (!moderationResult.IsSafe)
                    {
                        return BadRequest(new
                        {
                            Message = "La imagen de perfil no es apropiada.",
                           
                        });
                    }
                }

                await _authService.RegisterAsync(request);

                return StatusCode(201, new { Message = "Usuario creado correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ERROR en AuthController: {ex.Message}");
                return StatusCode(500, new { Message = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpPost("deactivate-account")]
        [Authorize] // 👈 Requiere estar autenticado
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeactivateAccount([FromBody] DeactivateAccountRequest request)
        {
            try
            {
                // Obtener el userId del token JWT
                var userIdClaim = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { Message = "Usuario no autenticado." });

                await _authService.DeactivateAccountAsync(userIdClaim, request.Reason);

                return Ok(new
                {
                    Message = "Tu cuenta ha sido desactivada con éxito. Será eliminada de forma permanente después de 30 días."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ERROR en DeactivateAccount: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }

        [HttpGet("profile")]
        [Authorize] // 👈 Requiere token
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileResponse>> GetProfile()
        {
            try
            {
                // Obtener userId del token JWT
                var userIdClaim = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { Message = "Usuario no autenticado." });

                var profile = await _authService.GetUserProfileAsync(userIdClaim);

                return Ok(profile);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 ERROR en GetProfile: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }

        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(new { Message = "Logout exitoso (token invalidado en el cliente)." });
        }
    }
}