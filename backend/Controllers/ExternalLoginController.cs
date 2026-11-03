using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ExternalLoginController(IExternalLoginService externalLoginService) : ControllerBase
{
  private readonly IExternalLoginService _externalLoginService = externalLoginService;

  [HttpPost]
  public async Task<IActionResult> Create(CreateExternalLoginDto dto)
  {
    try
    {
      var result = await _externalLoginService.CreateAsync(dto);
      return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(Guid id)
  {
    var result = await _externalLoginService.GetByIdAsync(id);
    return result is null ? NotFound() : Ok(result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    return Ok(await _externalLoginService.GetAllAsync());
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> Update(Guid id, UpdateExternalLoginDto dto)
  {
    try
    {
      return Ok(await _externalLoginService.UpdateAsync(id, dto));
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(Guid id)
  {
    try
    {
      await _externalLoginService.DeleteAsync(id);
      return NoContent();
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }
}