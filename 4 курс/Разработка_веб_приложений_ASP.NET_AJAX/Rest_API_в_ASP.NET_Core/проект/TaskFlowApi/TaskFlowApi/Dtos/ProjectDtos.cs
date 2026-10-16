using System.ComponentModel.DataAnnotations;

namespace TaskFlowApi.Dtos;

/// <summary>Проект (ответ).</summary>
public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TaskCount { get; set; }
}

/// <summary>Создание проекта.</summary>
public class CreateProjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

/// <summary>Обновление проекта.</summary>
public class UpdateProjectDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}