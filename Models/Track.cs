namespace wt_lab6_6minimalapi_ivanov.Models;

public class Track
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DurationSeconds { get; set; }
    
    // Связь с исполнителем
    public int ArtistId { get; set; } 
    
    public bool IsAvailable { get; set; } = true;
}