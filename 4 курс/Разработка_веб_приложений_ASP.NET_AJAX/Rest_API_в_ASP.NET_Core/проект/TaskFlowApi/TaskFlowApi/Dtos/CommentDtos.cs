using System.ComponentModel.DataAnnotations;

namespace TaskFlowApi.Dtos;

/// <summary>Комментарий (ответ).</summary>
public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>Создание комментария.</summary>
public class CreateCommentDto
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;

    public int AuthorId { get; set; }
}