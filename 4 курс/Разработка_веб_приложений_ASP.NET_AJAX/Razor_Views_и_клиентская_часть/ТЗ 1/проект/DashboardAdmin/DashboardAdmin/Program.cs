using DashboardAdmin.Models;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages (оставляем как было)
builder.Services.AddRazorPages();

// 🔹 Добавляем поддержку MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// Репозиторий
builder.Services.AddSingleton<IDashboardRepository, InMemoryDashboardRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 🔹 Маршрут для MVC-контроллеров (Dashboard станет страницей по умолчанию)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();