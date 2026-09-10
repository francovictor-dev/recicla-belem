// Backend/Controllers/ProfileController.cs
using Backend.Dtos;
using Backend.Filters;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
  private readonly IProfileService _profileService;

  public ProfileController(IProfileService profileService)
  {
    _profileService = profileService;
  }

  [HttpPost]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Create(CreateProfileDto dto)
  {
    var userId = Guid.Parse(User.FindFirst("sub")!.Value);

    try
    {
      var result = await _profileService.CreateAsync(userId, dto);
      return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    catch (InvalidOperationException ex)
    {
      return Conflict(new { message = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }

  [HttpGet("{id}")]
  [Authorize]
  [ServiceFilter(typeof(OwnershipFilter<Profile>))]
  public async Task<IActionResult> GetById(Guid id)
  {
    var result = await _profileService.GetByIdAsync(id);
    return result is null ? NotFound() : Ok(result);
  }

  [HttpPut("{id}")]
  [Authorize]
  [ServiceFilter(typeof(OwnershipFilter<Profile>))]
  public async Task<IActionResult> Update(Guid id, UpdateProfileDto dto)
  {
    try
    {
      var result = await _profileService.UpdateAsync(id, dto);
      return Ok(result);
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }

  [HttpDelete("{id}")]
  [Authorize]
  [ServiceFilter(typeof(OwnershipFilter<Profile>))]
  public async Task<IActionResult> Delete(Guid id)
  {
    try
    {
      await _profileService.DeleteAsync(id);
      return NoContent();
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }
}