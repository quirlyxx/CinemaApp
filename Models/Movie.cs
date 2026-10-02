namespace CinemaApp.Models;

public class Movie
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Genre { get; set; } = "";
    public string PosterBlobName { get; set; } = "";
    public string VideoBlobName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
