using System.ComponentModel.DataAnnotations;
using TaskFlowApi.Models;

namespace TaskFlowApi.Dtos;

/// <summary>Задача (ответ API v1).</summary>
public class TaskItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Status Status { get; set; }
    public int ProjectId { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToUsername { get; set; }
    public DateTime? DueDate { get; set; }
}

/// <summary>Задача (ответ API v2 — без Description, с Priority).</summary>
public class TaskItemV2Dto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public Status Status { get; set; }
    public Priority Priority { get; set; }
    public int ProjectId { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToUsername { get; set; }
    public DateTime? DueDate { get; set; }
}

/// <summary>Создание задачи.</summary>
public class CreateTaskDto
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }
    public Status? Status { get; set; }
    public int ProjectId { get; set; }
    public int? AssignedToId { get; set; }

    [FutureDate]
    public DateTime? DueDate { get; set; }
}

/// <summary>Обновление задачи.</summary>
public class UpdateTaskDto
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Status? Status { get; set; }

    public int? AssignedToId { get; set; }

    [FutureDate]
    public DateTime? DueDate { get; set; }
}
public class UpdateStatusDto
{
    public Status Status { get; set; }
}

/// <summary>
/// DTO для создания задачи в API v2.
/// </summary>
public class CreateTaskV2Dto
{
    /// <summary>Название задачи.</summary>
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Статус задачи. Если не указан — ToDo.</summary>
    public Status? Status { get; set; }

    /// <summary>Приоритет задачи (Low, Medium, High). Если не указан — Medium.</summary>
    public Priority? Priority { get; set; }

    /// <summary>Идентификатор проекта.</summary>
    public int ProjectId { get; set; }

    /// <summary>Идентификатор исполнителя.</summary>
    public int? AssignedToId { get; set; }

    /// <summary>Срок выполнения. Если указан — должен быть в будущем.</summary>
    [FutureDate]
    public DateTime? DueDate { get; set; }
}