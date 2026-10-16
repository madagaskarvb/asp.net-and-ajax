using LiveMonitoring;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<ConnectionTracker>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

app.MapHub<MonitoringHub>("/monitoringHub");

// Задание 3: системное сообщение всем
app.MapPost("/api/system-message", async (ConnectionTracker tracker, SystemMessageRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Text))
        return Results.BadRequest("Text is required");

    await tracker.SendSystemMessageAsync(req.Text);
    return Results.Ok(new { sent = true });
});

// Задание 5: внешнее уведомление в комнату
app.MapPost("/api/room-message", async (ConnectionTracker tracker, RoomMessageRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Room) || string.IsNullOrWhiteSpace(req.Text))
        return Results.BadRequest("Room and Text are required");

    await tracker.SendToRoomAsync(req.Room, req.Text);
    return Results.Ok(new { sent = true });
});

// Задание 5: внешнее приватное сообщение клиенту
app.MapPost("/api/private-message", async (ConnectionTracker tracker, PrivateMessageRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.ConnectionId) || string.IsNullOrWhiteSpace(req.Text))
        return Results.BadRequest("ConnectionId and Text are required");

    await tracker.SendPrivateAsync(req.ConnectionId, req.Text);
    return Results.Ok(new { sent = true });
});

app.Run();