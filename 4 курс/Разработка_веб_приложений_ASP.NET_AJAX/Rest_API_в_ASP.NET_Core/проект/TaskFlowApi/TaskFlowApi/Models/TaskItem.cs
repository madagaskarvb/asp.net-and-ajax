namespace TaskFlowApi.Models;

/// <summary>Задача.</summary>
public class TaskItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Status Status { get; set; } = Status.ToDo;
    public Priority Priority { get; set; } = Priority.Medium;
    public int? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public AppUser? AssignedTo { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}