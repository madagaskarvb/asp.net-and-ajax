using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Data;
using TaskFlowApi.Models;

namespace TaskFlowApi.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(RequestDelegate next, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        // Применяем только к POST /api/v2/tasks
        if (context.Request.Method != HttpMethods.Post || !context.Request.Path.StartsWithSegments("/api/v2/tasks"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Idempotency-Key", out var keyValues) || string.IsNullOrWhiteSpace(keyValues))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Idempotency key missing",
                Status = 400,
                Detail = "Заголовок X-Idempotency-Key обязателен для POST запросов."
            });
            return;
        }

        var key = keyValues.ToString();

        // Читаем тело запроса
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var requestBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var requestHash = ComputeHash(requestBody);

        var existingRecord = await db.IdempotencyRecords.FindAsync(key);
        if (existingRecord != null)
        {
            if (existingRecord.RequestBodyHash != requestHash)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Title = "Idempotency key reused with different body",
                    Status = 409,
                    Detail = "Ключ идемпотентности уже использовался с другим телом запроса."
                });
                return;
            }

            // Возвращаем сохраненный ответ
            context.Response.StatusCode = existingRecord.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(existingRecord.ResponseBody);
            return;
        }

        // Перехватываем ответ
        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await _next(context);

        context.Response.Body = originalBodyStream;
        responseBodyStream.Position = 0;
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();

        // Сохраняем только успешные ответы (2xx)
        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            db.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = key,
                RequestBodyHash = requestHash,
                ResponseBody = responseBody,
                StatusCode = context.Response.StatusCode,
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        responseBodyStream.Position = 0;
        await responseBodyStream.CopyToAsync(originalBodyStream);
    }

    private static string ComputeHash(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}