using Backend.Data;
using Backend.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AuthController(IAuthService authService, AppDbContext context, IOptions<S3Options> s3Options) : ControllerBase
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

  [HttpPost("google")]
  public async Task<IActionResult> GoogleLogin(GoogleLoginDto dto)
  {
    try { return Ok(await _authService.GoogleLoginAsync(dto)); }
    catch (UnauthorizedAccessException error) { return Unauthorized(new { message = error.Message }); }
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
  public async Task<IActionResult> Me()
  {
    var subject = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
    if (!Guid.TryParse(subject, out var id)) return Unauthorized();
    var user = await context.Users.Include(user => user.Profile).AsNoTracking().SingleOrDefaultAsync(user => user.Id == id);
    if (user is null) return Unauthorized();
    var baseUrl = s3Options.Value.PublicUrl.TrimEnd('/');
    return Ok(new
    {
      id = user.Id,
      userId = user.Id,
      email = user.Email,
      name = user.Profile?.Name ?? user.Email,
      profileImageUrl = user.Profile?.PhotoUrl ?? (string.IsNullOrWhiteSpace(baseUrl) ? null : $"{baseUrl}/users/{user.Id}/profile.png")
    });
  }
}
