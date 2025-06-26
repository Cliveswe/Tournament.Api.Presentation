using System.ComponentModel.DataAnnotations;

namespace Tournament.Core.Dto;
public class TournamentUpdateDto
{
    [Required(ErrorMessage = "Title is a required field.")]
    [MaxLength(100, ErrorMessage = "Maximum length for the Title is 100 characters.")]
    public required string Title { get; set; }
}
