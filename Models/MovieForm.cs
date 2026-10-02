using System.ComponentModel.DataAnnotations;

namespace CinemaApp.Models;

public class MovieForm
{
    [Required(ErrorMessage = "Вкажіть назву")]
    [StringLength(200)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть жанр")]
    [StringLength(100)]
    public string Genre { get; set; } = "";

    [Required(ErrorMessage = "Вкажіть опис")]
    public string Description { get; set; } = "";
}
