using CountryhouseService.API.Defaults;
using CountryhouseService.API.Extensions;
using CountryhouseService.API.Helpers;
using System.ComponentModel.DataAnnotations;

#nullable disable
namespace CountryhouseService.API.Dtos
{
    [Serializable]
    public record UpdateAdDto(
        [Required]
        [MaxLength(100, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        string Title,

        [MaxLength(1000, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        string Description,

        [Required]
        [MaxLength(100, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        string Address,

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = ErrorMessagesTemplates.NUMBER_NEGATIVE)]
        int Budget,

        [Required]
        [DataType(DataType.PhoneNumber)]
        [MaxLength(25, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        string ContactNumber,

        [MaxInts(8)]
        ICollection<int> ImagesIds = null,

        [LaterThanTodayOrToday]
        DateTime? AccomplishFromDate = null,

        [LaterThanTodayOrToday]
        DateTime? AccomplishUntilDate = null);
}
