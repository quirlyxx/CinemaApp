using CinemaApp.Models;
using CinemaApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controllers;

public class MoviesController : Controller
{
    private readonly IMovieStore _store;
    private readonly IBlobService _blobs;

    public MoviesController(IMovieStore store, IBlobService blobs)
    {
        _store = store;
        _blobs = blobs;
    }

    public async Task<IActionResult> Index() => View(await _store.GetAllAsync());

    public async Task<IActionResult> Details(string id)
    {
        var movie = await _store.GetAsync(id);
        return movie == null ? NotFound() : View(movie);
    }

    public IActionResult Create() => View(new MovieForm());

    [HttpPost]
    [RequestSizeLimit(500L * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 500L * 1024 * 1024)]
    public async Task<IActionResult> Create(MovieForm form, IFormFile? poster, IFormFile? video)
    {
        if (poster == null || poster.Length == 0)
            ModelState.AddModelError("poster", "Виберіть постер");
        if (video == null || video.Length == 0)
            ModelState.AddModelError("video", "Виберіть відеофайл");

        if (!ModelState.IsValid) return View(form);

        var movie = new Movie
        {
            Title = form.Title,
            Genre = form.Genre,
            Description = form.Description,
            PosterBlobName = await _blobs.UploadAsync(poster!, "posters"),
            VideoBlobName = await _blobs.UploadAsync(video!, "videos")
        };

        await _store.AddAsync(movie);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var movie = await _store.RemoveAsync(id);
        if (movie == null) return NotFound();

        await _blobs.DeleteAsync(movie.PosterBlobName);
        await _blobs.DeleteAsync(movie.VideoBlobName);
        return RedirectToAction(nameof(Index));
    }
}
