using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollectionPointController(ICollectionPointService collectionPointService) : ControllerBase
{
  private readonly ICollectionPointService _collectionPointService = collectionPointService;

  [HttpPost]
  [Authorize]
  public async Task<IActionResult> Create(CreateCollectionPointDto dto)
  {
    try
    {
      var result = await _collectionPointService.CreateAsync(dto);
      return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(Guid id, [FromQuery] string? include = null)
  {
    try
    {
      var result = await _collectionPointService.GetByIdAsync(id, ParseIncludes(include));
      return result is null ? NotFound() : Ok(result);
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }


  [HttpGet]
  public async Task<IActionResult> GetAll([FromQuery] string? include = null)
  {
    var result = await _collectionPointService.GetAllAsync(ParseIncludes(include));
    return Ok(result);
  }

  [HttpPut("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Update(Guid id, UpdateCollectionPointDto dto, [FromQuery] string? include = null)
  {
    try
    {
      var result = await _collectionPointService.UpdateAsync(id, dto, ParseIncludes(include));
      return Ok(result);
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }

  [HttpDelete("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Delete(Guid id)
  {
    try
    {
      await _collectionPointService.DeleteAsync(id);
      return NoContent();
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }

  private static string[] ParseIncludes(string? include)
      => string.IsNullOrWhiteSpace(include)
          ? Array.Empty<string>()
          : include
              .Split(',', StringSplitOptions.RemoveEmptyEntries)
              .Select(value => value.Trim().ToLowerInvariant())
              .Where(value => value.Length > 0)
              .Distinct()
              .ToArray();
}
