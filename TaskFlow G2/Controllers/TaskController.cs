using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow_G2.DTOs;
using TaskFlow_G2.Services;

namespace TaskFlow_G2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    private readonly TaskService _taskService;

    public TaskController(TaskService taskService)
    {
        _taskService = taskService;
    }
    
    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? throw new UnauthorizedAccessException("El token no contiene un identificador de usuario válido.");
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaskCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetUserId();
        var created = await _taskService.Create(userId, dto);

        return CreatedAtAction(nameof(GetAll), new { }, created);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var tasks = await _taskService.GetAllForUser(userId);
        return Ok(tasks);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = GetUserId();
        var deleted = await _taskService.Delete(userId, id);

        if (!deleted)
        {
            return NotFound(new { message = "La tarea no existe o no pertenece al usuario actual." });
        }

        return NoContent();
    }
}