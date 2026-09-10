using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddressController(IAddressService addressService) : ControllerBase
{
  private readonly IAddressService _addressService = addressService;

  [HttpPost]
  [Authorize]
  public async Task<IActionResult> Create(CreateAddressDto dto)
  {
    var result = await _addressService.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
  }

  [HttpGet("{id}")]
  [Authorize]
  public async Task<IActionResult> GetById(Guid id)
  {
    var result = await _addressService.GetByIdAsync(id);
    return result is null ? NotFound() : Ok(result);
  }

  [HttpPut("{id}")]
  [Authorize(Roles = "Admin")]
  public async Task<IActionResult> Update(Guid id, UpdateAddressDto dto)
  {
    try
    {
      var result = await _addressService.UpdateAsync(id, dto);
      return Ok(result);
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
      await _addressService.DeleteAsync(id);
      return NoContent();
    }
    catch (KeyNotFoundException ex)
    {
      return NotFound(new { message = ex.Message });
    }
  }
}
