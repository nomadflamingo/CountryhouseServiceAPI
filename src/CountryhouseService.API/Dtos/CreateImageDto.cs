using System.ComponentModel.DataAnnotations;

#nullable disable
namespace CountryhouseService.API.Dtos
{
    [Serializable]
    public record CreateImageDto(
        [Required]
        string Base64,

        [MaxLength(256)]
        string Name);
}
