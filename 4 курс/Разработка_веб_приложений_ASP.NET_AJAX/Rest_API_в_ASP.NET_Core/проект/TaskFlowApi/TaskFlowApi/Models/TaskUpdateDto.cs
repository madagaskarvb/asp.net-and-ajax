using System.ComponentModel.DataAnnotations;

namespace TaskFlowApi.Models;

/// <summary>
/// DTO для полного обновления задачи.
/// </summary>
public class TaskUpdateDto
{
    /// <summary>
    /// Название задачи.
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание задачи.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Статус задачи. Обязательное поле при полной замене.
    /// </summary>
    [Required]
    public Status? Status { get; set; }

    /// <summary>
    /// Идентификатор проекта.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Идентификатор исполнителя.
    /// </summary>
    public int? AssignedToId { get; set; }

    /// <summary>
    /// Срок выполнения. Если указан — должен быть в будущем.
    /// </summary>
    [FutureDate]
    public DateTime? DueDate { get; set; }
}