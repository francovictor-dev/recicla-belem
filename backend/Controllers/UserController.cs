using Backend.Dtos;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
  private readonly IUserService _userService;

  public UserController(IUserService userService)
  {
    _userService = userService;
  }

  [HttpPost]
  public async Task<IActionResult> Create(CreateUserDto dto)
  {
    try
    {
      var result = await _userService.CreateAsync(dto);
      return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    } 
    catch(InvalidOperationException ex)
    {
      return Conflict(new { message = ex.Message });
    }
  }

  [HttpPost("default/json")]
  [Consumes("application/json")]
  public async Task<IActionResult> CreateDefault(CreateDefaultUserDto dto)
  {
    try
    {
      var result = await _userService.CreateDefaultAsync(dto.User, dto.Profile, dto.ExternalLogin);
      return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    catch (ArgumentException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
      return Conflict(new { message = ex.Message });
    }
  }

  [HttpPost("default")]
  [Consumes("multipart/form-data")]
  public async Task<IActionResult> CreateDefaultWithImage([FromForm] CreateDefaultUserFormDto dto)
  {
    try
    {
      var result = await _userService.CreateDefaultAsync(dto.User, dto.Profile, dto.ExternalLogin, dto.Image);
      return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    catch (ArgumentException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
      return Conflict(new { message = ex.Message });
    }
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(Guid id)
  {
      var result = await _userService.GetByIdAsync(id);
      return result is null ? NotFound() : Ok(result);
  }

  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
      var result = await _userService.GetAllAsync();
      return Ok(result);
  }

  [HttpPut("{id}")]
  [Authorize]
  [ServiceFilter(typeof(OwnershipFilter<User>))]
  public async Task<IActionResult> Update(Guid id, UpdateUserDto dto)
  {
      try
      {
          var result = await _userService.UpdateAsync(id, dto);
          return Ok(result);
      }
      catch (KeyNotFoundException)
      {
          return NotFound();
      }
  }

  [HttpDelete("{id}")]
  [Authorize]
  [ServiceFilter(typeof(OwnershipFilter<User>))]
  public async Task<IActionResult> Delete(Guid id)
  {
    try
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
    catch (KeyNotFoundException)
    {
        return NotFound();
    }
  }
}
