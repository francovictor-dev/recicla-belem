using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ImageController(IImageService imageService) : ControllerBase
{
  private readonly IImageService _imageService = imageService;

  [HttpPost]
  public async Task<IActionResult> Create(CreateImageDto dto)
  {
    try
    {
      var result = await _imageService.CreateAsync(dto);
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
    var result = await _imageService.GetByIdAsync(id);
    return result is null ? NotFound() : Ok(result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    return Ok(await _imageService.GetAllAsync());
  }

  [HttpPut("{id}")]
  public async Task<IActionResult> Update(Guid id, UpdateImageDto dto)
  {
    try
    {
      return Ok(await _imageService.UpdateAsync(id, dto));
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
      await _imageService.DeleteAsync(id);
      return NoContent();
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }
}