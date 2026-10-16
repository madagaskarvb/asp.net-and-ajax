using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.Dtos;
using TaskFlowApi.Exceptions;
using TaskFlowApi.Models;

namespace TaskFlowApi.Controllers;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/tasks")]
public class TasksV2Controller : ControllerBase
{
    private readonly AppDbContext _db;
    public TasksV2Controller(AppDbContext db) => _db = db;

    /// <summary>
    /// Получить список задач с пагинацией, фильтрацией и сортировкой.
    /// </summary>
    /// <param name="page">Номер страницы (по умолчанию 1).</param>
    /// <param name="pageSize">Размер страницы (по умолчанию 10, максимум 50).</param>
    /// <param name="status">Фильтр по статусу (ToDo, InProgress, Done).</param>
    /// <param name="priority">Фильтр по приоритету (Low, Medium, High).</param>
    /// <param name="sortBy">Поле для сортировки (title, dueDate, createdAt).</param>
    /// <param name="sortDir">Направление сортировки (asc, desc).</param>
    /// <param name="search">Поиск по Title (содержит).</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskItemV2Dto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<TaskItemV2Dto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? sortBy = "createdAt",
        [FromQuery] string? sortDir = "desc",
        [FromQuery] string? search = null)
    {
        if (pageSize > 50)
            return BadRequest(new ProblemDetails { Title = "Invalid pageSize", Detail = "Максимальный размер страницы — 50.", Status = 400 });
        if (page < 1) page = 1;

        var query = _db.Tasks.Include(t => t.AssignedTo).AsQueryable();

        // Фильтрация
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Status>(status, true, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);
        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<Priority>(priority, true, out var priorityEnum))
            query = query.Where(t => t.Priority == priorityEnum);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Title.Contains(search));

        // Сортировка
        query = sortBy?.ToLower() switch
        {
            "title" => sortDir?.ToLower() == "asc" ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title),
            "duedate" => sortDir?.ToLower() == "asc" ? query.OrderBy(t => t.DueDate) : query.OrderByDescending(t => t.DueDate),
            _ => sortDir?.ToLower() == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new PagedResult<TaskItemV2Dto>
        {
            Items = items.Select(t => t.ToV2Dto()).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    /// <summary>
    /// Получить задачу по идентификатору.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskItemV2Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItemV2Dto>> GetById(int id)
    {
        var task = await _db.Tasks.Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            throw new NotFoundException($"Задача с ID {id} не найдена.");

        return Ok(task.ToV2Dto());
    }

    /// <summary>
    /// Создать новую задачу (v2). Требует заголовок X-Idempotency-Key.
    /// </summary>
    /// <param name="dto">Данные для создания задачи.</param>
    /// <returns>Созданная задача.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TaskItemV2Dto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TaskItemV2Dto>> Create(
        [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKey,
        [FromBody] CreateTaskV2Dto dto)
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
            Status = dto.Status ?? Status.ToDo,
            Priority = dto.Priority ?? Priority.Medium,
            AssignedToId = dto.AssignedToId,
            DueDate = dto.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        await _db.Entry(task).Reference(t => t.AssignedTo).LoadAsync();

        return CreatedAtAction(nameof(GetById),
            new { id = task.Id, version = "2" },
            task.ToV2Dto());
    }

    /// <summary>
    /// Обновить статус задачи (для проверки BusinessRuleException).
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        var task = await _db.Tasks.Include(t => t.Comments).FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
            throw new NotFoundException($"Задача с ID {id} не найдена.");

        if (dto.Status == Status.Done && (task.Comments == null || !task.Comments.Any()))
        {
            throw new BusinessRuleException("Нельзя завершить задачу без комментариев");
        }

        task.Status = dto.Status;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}