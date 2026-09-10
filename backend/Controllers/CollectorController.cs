using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CollectorController(ICollectorService collectorService) : ControllerBase
{
  private readonly ICollectorService _collectorService = collectorService;

  [HttpPost]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Create(CreateCollectorDto dto)
  {
    try
    {
      var result = await _collectorService.CreateAsync(dto);
      return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    } 
    catch(Exception ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }
  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(Guid id)
  {
    var result = await _collectorService.GetByIdAsync(id);
    return result is null ? NotFound() : Ok(result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var result = await _collectorService.GetAllAsync();   
    return result is null ? NotFound() : Ok(result);
  }

  [HttpPut("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Update(Guid id, UpdateCollectorDto dto)
  {
    try
    {
      var result = await _collectorService.UpdateAsync(id, dto);
      return Ok(result);
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(Guid id)
  {
    try
    {
      await _collectorService.DeleteAsync(id);
      return NoContent();
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }
}