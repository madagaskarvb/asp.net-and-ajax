namespace TaskFlowApi.Models;

/// <summary>Комментарий к задаче.</summary>
public class Comment
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public TaskItem TaskItem { get; set; } = null!;
    public AppUser Author { get; set; } = null!;
}