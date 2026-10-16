using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Models;

namespace TaskFlowApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Comment> Comments => Set<Comment>();

    // НОВОЕ ДЛЯ УРОВНЯ 3:
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Project → TaskItem (1:M, cascade)
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // AppUser → TaskItem (AssignedToId, null on delete)
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        // TaskItem → Comment (1:M, cascade)
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.TaskItem)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // AppUser → Comment (AuthorId, cascade)
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индекс по Status
        modelBuilder.Entity<TaskItem>().HasIndex(t => t.Status);

        // НОВОЕ ДЛЯ УРОВНЯ 3: Первичный ключ для IdempotencyRecord
        modelBuilder.Entity<IdempotencyRecord>().HasKey(x => x.Key);

        // Seed data (CreatedAt должен быть константой!)
        var seed = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<AppUser>().HasData(new AppUser
        {
            Id = 1,
            Username = "admin",
            Email = "admin@taskflow.local",
            PasswordHash = "seeded-hash"
        });

        modelBuilder.Entity<Project>().HasData(new Project
        {
            Id = 1,
            Name = "Демо-проект",
            Description = "Проект из seed-данных",
            CreatedAt = seed
        });

        modelBuilder.Entity<TaskItem>().HasData(new TaskItem
        {
            Id = 1,
            ProjectId = 1,
            Title = "Демонстрационная задача",
            Description = "Создана автоматически",
            Status = Status.ToDo,
            Priority = Priority.Medium,
            AssignedToId = 1,
            CreatedAt = seed
        });

        base.OnModelCreating(modelBuilder);
    }
}