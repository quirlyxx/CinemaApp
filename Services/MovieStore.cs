using System.Text.Json;
using CinemaApp.Models;

namespace CinemaApp.Services;

public interface IMovieStore
{
    Task<List<Movie>> GetAllAsync();
    Task<Movie?> GetAsync(string id);
    Task AddAsync(Movie movie);
    Task<Movie?> RemoveAsync(string id);
}
public class MovieStore : IMovieStore
{
    private const string FileName = "movies.json";
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    private readonly IBlobService _blobs;
    private readonly SemaphoreSlim _lock = new(1, 1);
    public MovieStore(IBlobService blobs) => _blobs = blobs;

    private async Task<List<Movie>> LoadAsync()
    {
        var json = await _blobs.ReadTextAsync(FileName);
        return string.IsNullOrWhiteSpace(json)
            ? new List<Movie>()
            : JsonSerializer.Deserialize<List<Movie>>(json) ?? new List<Movie>();
    }

    private Task SaveAsync(List<Movie> movies)
        => _blobs.WriteTextAsync(FileName, JsonSerializer.Serialize(movies, Json));

    public async Task<List<Movie>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try { return (await LoadAsync()).OrderByDescending(m => m.CreatedAt).ToList(); }
        finally { _lock.Release(); }
    }

    public async Task<Movie?> GetAsync(string id)
        => (await GetAllAsync()).FirstOrDefault(m => m.Id == id);
    public async Task AddAsync(Movie movie)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await LoadAsync();
            all.Add(movie);
            await SaveAsync(all);
        }
        finally { _lock.Release(); }
    }
    public async Task<Movie?> RemoveAsync(string id)
    {
        await _lock.WaitAsync();
        try
        {
            var all = await LoadAsync();
            var movie = all.FirstOrDefault(m => m.Id == id);
            if (movie == null) return null;
            all.Remove(movie);
            await SaveAsync(all);
            return movie;
        }finally { _lock.Release(); }
    }
}
