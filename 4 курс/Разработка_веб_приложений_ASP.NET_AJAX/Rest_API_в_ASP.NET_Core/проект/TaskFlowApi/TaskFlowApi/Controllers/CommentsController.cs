using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.Dtos;
using TaskFlowApi.Models;

namespace TaskFlowApi.Controllers;

/// <summary>Комментарии задач (вложенный ресурс).</summary>
[ApiController]
[ApiVersion("1.0")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public CommentsController(AppDbContext db) => _db = db;

    /// <summary>Список комментариев задачи.</summary>
    [HttpGet("api/v{version:apiVersion}/tasks/{taskId:int}/comments")]
    [ProducesResponseType(typeof(IEnumerable<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetForTask(int taskId)
    {
        if (!await _db.Tasks.AnyAsync(t => t.Id == taskId))
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = $"Task {taskId} does not exist.",
                Status = StatusCodes.Status404NotFound
            });

        var comments = await _db.Comments
            .Include(c => c.Author)
            .Where(c => c.TaskItemId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments.Select(c => c.ToDto()));
    }

    /// <summary>Добавить комментарий к задаче.</summary>
    [HttpPost("api/v{version:apiVersion}/tasks/{taskId:int}/comments")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> Create(int taskId, [FromBody] CreateCommentDto dto)
    {
        if (!await _db.Tasks.AnyAsync(t => t.Id == taskId))
            return NotFound(new ProblemDetails
            {
                Title = "Task not found",
                Detail = $"Task {taskId} does not exist.",
                Status = StatusCodes.Status404NotFound
            });

        if (!await _db.Users.AnyAsync(u => u.Id == dto.AuthorId))
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid AuthorId",
                Detail = $"User {dto.AuthorId} not found.",
                Status = StatusCodes.Status400BadRequest
            });

        var comment = new Comment
        {
            TaskItemId = taskId,
            AuthorId = dto.AuthorId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        await _db.Entry(comment).Reference(c => c.Author).LoadAsync();

        return CreatedAtAction(nameof(GetForTask),
            new { taskId, version = "1" },
            comment.ToDto());
    }

    /// <summary>Удалить комментарий.</summary>
    [HttpDelete("api/v{version:apiVersion}/comments/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment == null) return NotFound();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}