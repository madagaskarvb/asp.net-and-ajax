using System.Diagnostics;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ITaskService, InMemoryTaskService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// ---- Проверка API-ключа для /tasks/api/* ----
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/tasks/api"))
    {
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey)
            || apiKey != "secret123")
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"error\":\"Unauthorized: invalid or missing X-Api-Key\"}");
            return; // короткое замыкание — next не вызываем
        }
    }

    await next(context);
});
// ---------------------------------------------

app.UseRouting();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();

    context.Response.OnStarting(() =>
    {
        sw.Stop();
        context.Response.Headers["X-Response-Time-ms"] =
            sw.ElapsedMilliseconds.ToString();
        return Task.CompletedTask;
    });

    await next(context);
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-App-Name", "TaskBoard");
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("--> {Method} {Path}", context.Request.Method, context.Request.Path);
    await next(context);
    logger.LogInformation("<-- {StatusCode}", context.Response.StatusCode);
});

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/health")
    {
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("healthy");
        return; // не вызываем next — запрос не идёт дальше
    }
    await next(context);
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();