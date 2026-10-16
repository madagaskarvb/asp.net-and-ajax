namespace TaskFlowApi.Models;

/// <summary>
/// Пользователь.
/// </summary>
public class AppUser
{
    /// <summary>Идентификатор пользователя.</summary>
    public int Id { get; set; }

    /// <summary>Имя пользователя.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Email пользователя.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Хэш пароля (никогда не отдаётся в API).</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}