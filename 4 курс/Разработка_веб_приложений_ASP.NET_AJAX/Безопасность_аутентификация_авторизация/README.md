# Безопасное приложение для управления задачами

**Цель проекта** — Создать веб-приложение для управления задачами с полноценной системой безопасности: регистрацией и входом через ASP.NET Core Identity, ролями и политиками доступа, resource-based авторизацией, JWT для REST API, защитой от CSRF и XSS, security-заголовками и Swagger для тестирования API с токеном. В фокусе — не CRUD задач, а безопасность: аутентификация, авторизация, разграничение доступа к своим/чужим задачам, защита форм и API.

**Стек**: ASP.NET Core, Razor Pages, EF Core (SQLite или SQL Server), ASP.NET Core Identity, JWT Bearer, Swagger, HTML + JavaScript (fetch), Bootstrap.