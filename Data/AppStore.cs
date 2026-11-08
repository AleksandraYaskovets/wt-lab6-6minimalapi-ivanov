using wt_lab6_6minimalapi_ivanov.Models;

namespace wt_lab6_6minimalapi_ivanov.Data;

public static class AppStore
{
    public static List<Artist> Artists { get; } = new()
    {
        new Artist { Id = 1, Name = "Queen", Country = "UK" },
        new Artist { Id = 2, Name = "John Lennon", Country = "UK" },
        new Artist { Id = 3, Name = "Michael Jackson", Country = "USA" }
    };

    public static List<Track> Tracks { get; } = new()
    {
        new Track { Id = 1, Title = "Bohemian Rhapsody", DurationSeconds = 355, ArtistId = 1, IsAvailable = true },
        new Track { Id = 2, Title = "Imagine", DurationSeconds = 183, ArtistId = 2, IsAvailable = true },
        new Track { Id = 3, Title = "Billie Jean", DurationSeconds = 294, ArtistId = 3, IsAvailable = false },
        new Track { Id = 4, Title = "Love of My Life", DurationSeconds = 219, ArtistId = 1, IsAvailable = true },
        new Track { Id = 5, Title = "Thriller", DurationSeconds = 358, ArtistId = 3, IsAvailable = true }
    };
}