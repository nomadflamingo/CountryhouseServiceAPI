namespace CountryhouseService.API.Dtos
{
    public record RequestDto(
        int Id,
        string Comment,
        string Status,
        string ContractorId,
        string ContractorName,
        Uri ContractorAvatar,
        string ContactNumber,
        string Email,
        string AdTitle,
        string AdDescription,
        Uri AdPreviewImageSource,
        DateTime CreatedOn,
        DateTime UpdatedOn);
}
