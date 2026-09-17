using Backend.Dtos;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController(IReportService reportService) : ControllerBase
{
  private readonly IReportService _reportService = reportService;

  [HttpPost]
  [Authorize]
  public async Task<IActionResult> Create(CreateReportDto dto)
  {
    try
    {
      var result = await _reportService.CreateAsync(dto);
      return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }

  [HttpGet("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetById(Guid id)
  {
    var result = await _reportService.GetByIdAsync(id);
    return result is null ? NotFound() : Ok(result);
  }

  [HttpGet]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> GetAll()
  {
    return Ok(await _reportService.GetAllAsync());
  }

  [HttpPut("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Update(Guid id, UpdateReportDto dto)
  {
    try
    {
      return Ok(await _reportService.UpdateAsync(id, dto));
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }

  [HttpDelete("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Delete(Guid id)
  {
    try
    {
      await _reportService.DeleteAsync(id);
      return NoContent();
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }
}