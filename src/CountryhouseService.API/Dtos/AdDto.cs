namespace CountryhouseService.API.Dtos
#nullable disable
{
    [Serializable]
    public record AdDto(
        int Id,
        string Title,
        string Description,
        string Address,
        int Budget,
        string ContactNumber,
        Uri PreviewImage,
        ICollection<Uri> NonPreviewImages,
        DateTime CreatedOn,
        DateTime UpdatedOn,
        string AuthorName,
        string AuthorId,
        string Status,
        DateTime? AccomplishFromDate,
        DateTime? AccomplishUntilDate);
}
