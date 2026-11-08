using wt_lab7_7postgres_efcore_ivanov.Models;
using wt_lab7_7postgres_efcore_ivanov.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Получаем строку подключения (для локальной разработки)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Проверяем наличие переменной Render (DATABASE_URL)
var databaseUrl = builder.Configuration["DATABASE_URL"];

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    // Заменяем postgresql:// на postgres://, чтобы C# правильно прочитал URL
    databaseUrl = databaseUrl.Replace("postgresql://", "postgres://");
    
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Disable";
}

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    // Формируем строку подключения для облачной базы Render
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Disable";
}

// 3. Регистрируем DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// 4. Автоматическое применение миграций при запуске
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// --- БАЗОВЫЕ ЭНДПОИНТЫ ---
app.MapGet("/", () => "Minimal API с PostgreSQL работает!");
app.MapGet("/api/info", () => new { Application = "Music Streaming API (EF Core)", Version = "2.0" });

// --- ЭНДПОИНТЫ ДЛЯ ИСПОЛНИТЕЛЕЙ (ARTISTS) ---

// GET: Получить всех исполнителей
app.MapGet("/api/artists", async (AppDbContext db) =>
    await db.Artists.ToListAsync());

// POST: Добавить исполнителя
app.MapPost("/api/artists", async (Artist artist, AppDbContext db) =>
{
    db.Artists.Add(artist);
    await db.SaveChangesAsync();
    return Results.Created($"/api/artists/{artist.Id}", artist);
});


// --- ЭНДПОИНТЫ ДЛЯ ТРЕКОВ (TRACKS) ---

// GET: Список треков с фильтрацией по названию
app.MapGet("/api/tracks", async (string? search, AppDbContext db) =>
{
    var query = db.Tracks.AsQueryable();
    
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(t => t.Title.ToLower().Contains(search.ToLower()));
    }
    
    return Results.Ok(await query.ToListAsync());
});

// GET: Один трек по ID (с информацией об исполнителе)
app.MapGet("/api/tracks/{id}", async (int id, AppDbContext db) =>
{
    var track = await db.Tracks.FindAsync(id);
    if (track == null) return Results.NotFound();

    var artist = await db.Artists.FindAsync(track.ArtistId);

    return Results.Ok(new
    {
        track.Id,
        track.Title,
        track.DurationSeconds,
        track.IsAvailable,
        ArtistName = artist?.Name ?? "Неизвестен"
    });
});

// POST: Создать новый трек
app.MapPost("/api/tracks", async (Track newTrack, AppDbContext db) =>
{
    // Проверка: существует ли исполнитель, на которого ссылается трек
    if (!await db.Artists.AnyAsync(a => a.Id == newTrack.ArtistId))
        return Results.BadRequest("Исполнитель с таким ArtistId не найден.");

    db.Tracks.Add(newTrack);
    await db.SaveChangesAsync();
    
    return Results.Created($"/api/tracks/{newTrack.Id}", newTrack);
});

// PUT: Обновить существующий трек
app.MapPut("/api/tracks/{id}", async (int id, Track updatedTrack, AppDbContext db) =>
{
    var track = await db.Tracks.FindAsync(id);
    if (track == null) return Results.NotFound();

    // Обновляем поля
    track.Title = updatedTrack.Title;
    track.DurationSeconds = updatedTrack.DurationSeconds;
    track.ArtistId = updatedTrack.ArtistId;
    track.IsAvailable = updatedTrack.IsAvailable;

    await db.SaveChangesAsync();
    return Results.Ok(track);
});

// DELETE: Удалить трек
app.MapDelete("/api/tracks/{id}", async (int id, AppDbContext db) =>
{
    var track = await db.Tracks.FindAsync(id);
    if (track == null) return Results.NotFound();

    db.Tracks.Remove(track);
    await db.SaveChangesAsync();
    
    return Results.NoContent();
});

// --- ДОПОЛНИТЕЛЬНЫЕ LINQ-ЗАПРОСЫ ---

// Получить только доступные треки, отсортированные по алфавиту
app.MapGet("/api/tracks/available", async (AppDbContext db) =>
    await db.Tracks.Where(t => t.IsAvailable).OrderBy(t => t.Title).ToListAsync());

// Получить треки длиннее 5 минут (300 секунд)
app.MapGet("/api/tracks/long", async (AppDbContext db) =>
    await db.Tracks.Where(t => t.DurationSeconds > 300).ToListAsync());

// --- ЗАПУСК ПРИЛОЖЕНИЯ (Для Render и локально) ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");