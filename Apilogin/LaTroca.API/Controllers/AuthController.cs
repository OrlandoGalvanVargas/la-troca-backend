using Microsoft.AspNetCore.Mvc;
using TorneoUniversitario.Application.DTOs;
using TorneoUniversitario.Application.Interfaces;

namespace TorneoUniversitario.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"🔥 ERROR en AuthController: {ex.Message}");
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"🔥 ERROR en AuthController: {ex.Message}");
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> Register([FromForm] RegisterRequest request)
    {
        try
        {
            await _authService.RegisterAsync(request);
            return StatusCode(201, new { Message = "Usuario creado correctamente" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 ERROR en AuthController: {ex.Message}");
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return Ok(new { Message = "Logout exitoso (token invalidado en el cliente)" });
    }
}
