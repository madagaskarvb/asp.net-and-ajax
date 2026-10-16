namespace TaskFlowApi.Models;

/// <summary>Статус задачи.</summary>
public enum Status { ToDo, InProgress, Done }

/// <summary>Приоритет задачи (используется в API v2).</summary>
public enum Priority { Low, Medium, High }