using Microsoft.AspNetCore.Mvc;
using TaskFlow_G2.DTOs;
using TaskFlow_G2.Services;

namespace TaskFlow_G2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // POST /api/Auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _authService.Register(dto);

            if (result is null)
            {
                return Conflict(new { message = "Ya existe una cuenta registrada con ese correo." });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/Auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.Login(dto);

        if (result is null)
        {
            return Unauthorized(new { message = "Correo o contraseña incorrectos." });
        }

        return Ok(result);
    }
}