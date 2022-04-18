using System.ComponentModel.DataAnnotations;

#nullable disable
namespace CountryhouseService.API.Dtos
{
    [Serializable]
    public record ImageDto(
        int Id,
        Uri Source);
}
