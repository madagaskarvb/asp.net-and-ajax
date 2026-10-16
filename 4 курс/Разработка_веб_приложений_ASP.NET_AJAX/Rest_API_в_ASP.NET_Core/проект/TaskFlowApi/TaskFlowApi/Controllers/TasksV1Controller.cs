using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.Dtos;
using TaskFlowApi.Models;

namespace TaskFlowApi.Controllers;

/// <summary>Задачи (API v1).</summary>
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[Route("api/v{version:apiVersion}/tasks")]
public class TasksV1Controller : ControllerBase
{
    private readonly AppDbContext _db;
    public TasksV1Controller(AppDbContext db) => _db = db;

    /// <summary>Список всех задач.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetAll()
    {
        var items = await _db.Tasks.Include(t => t.AssignedTo).ToListAsync();
        return Ok(items.Select(t => t.ToV1Dto()));
    }

    /// <summary>Задача по идентификатору.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemDto>> GetById(int id)
    {
        var task = await _db.Tasks.Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);
        return task == null ? NotFound() : Ok(task.ToV1Dto());
    }

    /// <summary>Создать задачу. Если Status не указан — ToDo.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskItemDto>> Create([FromBody] CreateTaskDto dto)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == dto.ProjectId))
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid ProjectId",
                Detail = $"Project {dto.ProjectId} not found.",
                Status = StatusCodes.Status400BadRequest
            });

        var task = new TaskItem
        {
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status ?? Status.ToDo,
            Priority = Priority.Medium,
            AssignedToId = dto.AssignedToId,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        await _db.Entry(task).Reference(t => t.AssignedTo).LoadAsync();

        return CreatedAtAction(nameof(GetById),
            new { id = task.Id, version = "1" },
            task.ToV1Dto());
    }

    /// <summary>Обновить задачу целиком. Status обязателен.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status!.Value;
        task.AssignedToId = dto.AssignedToId;
        task.DueDate = dto.DueDate;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Удалить задачу.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}