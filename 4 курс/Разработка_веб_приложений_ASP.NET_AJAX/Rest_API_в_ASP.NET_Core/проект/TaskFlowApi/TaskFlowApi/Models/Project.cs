using System.ComponentModel.DataAnnotations;

namespace TaskFlowApi.Models;

/// <summary>Проект.</summary>
public class Project
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}