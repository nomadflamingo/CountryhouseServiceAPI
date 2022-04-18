using System.ComponentModel.DataAnnotations;
using CountryhouseService.API.Defaults;

namespace CountryhouseService.API.Dtos
{
    public record CreateRequestDto(
        [MaxLength(2000, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        string Comment);
}

