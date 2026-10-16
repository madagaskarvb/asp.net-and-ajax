using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TaskFlowApi.Data;
using TaskFlowApi.Exceptions;
using TaskFlowApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// EF Core + SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
                      ?? "Data Source=taskflow.db"));

// Controllers + enum as string
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskFlow API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "TaskFlow API", Version = "v2" });

    options.DocInclusionPredicate((docName, apiDesc) => apiDesc.GroupName == docName);
});

var app = builder.Build();

// Автоматически применяем миграции при запуске
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "TaskFlow API v2");
    });
}

// Global Exception Handler должен быть высоко
app.UseExceptionHandler();

app.UseHttpsRedirection();

// Order: UseRouting -> UseCors -> UseAuthorization
app.UseRouting();

// Idempotency middleware (после роутинга, но перед CORS/авторизацией, 
// чтобы перехватывать запросы к /api/v2/tasks)
app.UseMiddleware<IdempotencyMiddleware>();

app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();
app.Run();