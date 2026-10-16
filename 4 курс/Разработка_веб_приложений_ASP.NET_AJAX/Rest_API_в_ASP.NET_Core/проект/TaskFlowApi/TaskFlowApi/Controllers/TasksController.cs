using Microsoft.AspNetCore.Mvc;
using TaskFlowApi.Models;

namespace TaskFlowApi.Controllers;

/// <summary>
/// Контроллер для управления задачами.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Obsolete("This controller is deprecated. Use the /api/v1/tasks endpoints instead.")] // <--- Add this here
public class TasksController : ControllerBase
{
    // ... rest of the code
}