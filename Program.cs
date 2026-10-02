using CinemaApp.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IBlobService, BlobService>();
builder.Services.AddSingleton<IMovieStore, MovieStore>();

const long maxSize = 500L * 1024 * 1024;
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = maxSize);
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = maxSize);

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Movies}/{action=Index}/{id?}");
app.Run();
