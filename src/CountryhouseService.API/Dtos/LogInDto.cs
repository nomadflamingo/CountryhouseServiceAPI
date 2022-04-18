using System.ComponentModel.DataAnnotations;

#nullable disable
namespace CountryhouseService.API.Dtos
{
    [Serializable]
    public record LogInDto(
        [Required]
        string Email,

        [Required]
        string Password);
}
