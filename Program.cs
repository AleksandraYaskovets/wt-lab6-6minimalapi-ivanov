using wt_lab6_6minimalapi_ivanov.Models;
using wt_lab6_6minimalapi_ivanov.Data;

var builder = WebApplication.CreateBuilder(args);

// Настройка OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// --- БАЗОВЫЕ ЭНДПОИНТЫ ---
app.MapGet("/", () => "Minimal API работает!");
app.MapGet("/api/info", () => new { Application = "Music Streaming API", Version = "1.0" });

// --- ЭНДПОИНТЫ ДЛЯ ИСПОЛНИТЕЛЕЙ (ARTISTS) ---

// Получить всех исполнителей
app.MapGet("/api/artists", () => Results.Ok(AppStore.Artists));

// Добавить нового исполнителя (POST)
app.MapPost("/api/artists", (Artist artist) =>
{
    artist.Id = AppStore.Artists.Any() ? AppStore.Artists.Max(a => a.Id) + 1 : 1;
    AppStore.Artists.Add(artist);
    return Results.Created($"/api/artists/{artist.Id}", artist);
});


// --- ЭНДПОИНТЫ ДЛЯ ТРЕКОВ (TRACKS) ---

// 1. GET: Получить список треков (с фильтрацией по названию)
app.MapGet("/api/tracks", (string? search) =>
{
    var tracks = AppStore.Tracks.AsEnumerable();
    if (!string.IsNullOrWhiteSpace(search))
    {
        tracks = tracks.Where(t => t.Title.Contains(search, StringComparison.OrdinalIgnoreCase));
    }
    return Results.Ok(tracks);
});

// 2. GET: Получить трек по ID (с информацией об исполнителе!)
app.MapGet("/api/tracks/{id}", (int id) =>
{
    var track = AppStore.Tracks.FirstOrDefault(t => t.Id == id);
    if (track == null) return Results.NotFound();

    // Находим исполнителя для этого трека, чтобы вернуть красивые данные
    var artist = AppStore.Artists.FirstOrDefault(a => a.Id == track.ArtistId);
    
    // Возвращаем анонимный объект, объединяющий данные трека и имя исполнителя
    return Results.Ok(new 
    {
        track.Id,
        track.Title,
        track.DurationSeconds,
        track.IsAvailable,
        ArtistName = artist?.Name ?? "Неизвестен"
    });
});

// 3. POST: Создать трек
app.MapPost("/api/tracks", (Track newTrack) =>
{
    // Проверка: существует ли исполнитель, на которого ссылается трек
    if (!AppStore.Artists.Any(a => a.Id == newTrack.ArtistId))
        return Results.BadRequest("Исполнитель с таким ArtistId не найден.");

    newTrack.Id = AppStore.Tracks.Any() ? AppStore.Tracks.Max(t => t.Id) + 1 : 1;
    AppStore.Tracks.Add(newTrack);
    return Results.Created($"/api/tracks/{newTrack.Id}", newTrack);
});

// 4. PUT: Обновить трек
app.MapPut("/api/tracks/{id}", (int id, Track updatedTrack) =>
{
    var track = AppStore.Tracks.FirstOrDefault(t => t.Id == id);
    if (track == null) return Results.NotFound();
    
    if (!AppStore.Artists.Any(a => a.Id == updatedTrack.ArtistId))
        return Results.BadRequest("Исполнитель с таким ArtistId не найден.");

    track.Title = updatedTrack.Title;
    track.DurationSeconds = updatedTrack.DurationSeconds;
    track.ArtistId = updatedTrack.ArtistId;
    track.IsAvailable = updatedTrack.IsAvailable;

    return Results.Ok(track);
});

// 5. DELETE: Удалить трек
app.MapDelete("/api/tracks/{id}", (int id) =>
{
    var track = AppStore.Tracks.FirstOrDefault(t => t.Id == id);
    if (track == null) return Results.NotFound();

    AppStore.Tracks.Remove(track);
    return Results.NoContent();
});

// Настройка порта для Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");