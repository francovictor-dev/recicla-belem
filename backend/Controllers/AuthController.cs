using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
  private readonly IAuthService _authService = authService;

  [HttpPost("login")]
  public async Task<IActionResult> Login(LoginDto dto)
  {
    try
    {
      var result = await _authService.LoginAsync(dto);
      return Ok(result);
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new { message = ex.Message });
    }
  }

  [HttpPost("refresh")]
  public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
  {
    try
    {
      var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
      return Ok(result);
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new { message = ex.Message });
    }
  }

  [HttpGet("me")]
  [Authorize]
  public IActionResult Me()
  {
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
    var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
        ?? User.FindFirst("email")?.Value;

    return Ok(new { userId, email });
  }
}