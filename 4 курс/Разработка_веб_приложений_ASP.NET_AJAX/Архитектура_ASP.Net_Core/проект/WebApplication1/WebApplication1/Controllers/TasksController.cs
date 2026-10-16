using Microsoft.AspNetCore.Mvc;
using WebApplication1.Services;
namespace WebApplication1.Controllers;

public class CreateTaskRequest
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
}

public class TasksController : Controller
{
    private readonly ITaskService _service;
    public TasksController(ITaskService service)
    {
        _service = service;
    }
    // GET /tasks
    public IActionResult Index(string? status)
    {
        var all = _service.GetAll();

        // Нормализуем: всё, что не done/active — считаем all
        var normalized = status is "done" or "active" ? status : "all";

        var filtered = normalized switch
        {
            "done" => all.Where(t => t.IsDone).ToList(),
            "active" => all.Where(t => !t.IsDone).ToList(),
            _ => all
        };

        // Счётчики считаем по ПОЛНОМУ списку, а не по отфильтрованному,
        // чтобы «Всего задач: X, выполнено: Y» не менялось от фильтра.
        ViewBag.TotalTasks = all.Count;
        ViewBag.CompletedTasks = all.Count(t => t.IsDone);
        ViewBag.CurrentStatus = normalized;

        return View(filtered);
    }
    // GET /tasks/details/1
    public IActionResult Details(int id)
    {
        var task = _service.GetById(id);
        if (task == null) return NotFound();
        return View(task);
    }
    // GET /tasks/create
    public IActionResult Create()
    {
        return View();
    }
    // POST /tasks/create
    [HttpPost]
    public IActionResult Create(string title, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError("title", "Заголовок обязателен");
            return View();
        }
        _service.Add(title, description);
        return RedirectToAction(nameof(Index));
    }
    // POST /tasks/done/1?status=active
    [HttpPost]
    public IActionResult Done(int id, string? status)
    {
        _service.MarkDone(id);
        return RedirectToAction(nameof(Index), new { status });
    }
    // POST /tasks/delete/1?status=done
    [HttpPost]
    public IActionResult Delete(int id, string? status)
    {
        _service.Delete(id);
        return RedirectToAction(nameof(Index), new { status });
    }

    // GET /tasks/api/list
    [HttpGet("tasks/api/list")]
    public IActionResult ApiList()
    {
        var tasks = _service.GetAll();
        return Json(tasks);
    }
    // GET /tasks/api/1
    [HttpGet("tasks/api/{id:int}")]
    public IActionResult ApiGet(int id)
    {
        var task = _service.GetById(id);
        if (task == null) return NotFound();
        return Json(task);
    }
    // POST /tasks/api/create
    [HttpPost("tasks/api/create")]
    public IActionResult ApiCreate([FromBody] CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Заголовок обязателен" });
        var task = _service.Add(request.Title, request.Description);
        return CreatedAtAction(nameof(ApiGet), new { id = task.Id }, task);
    }
}

