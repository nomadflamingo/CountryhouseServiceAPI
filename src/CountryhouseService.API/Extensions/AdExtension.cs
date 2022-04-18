using CountryhouseService.API.Dtos;
using CountryhouseService.API.Models;

namespace CountryhouseService.API.Extensions
{
    public static class AdExtension
    {
        public static AdDto AsDto(this Ad ad)
        {
            List<Uri> imagesUrls = new();

            if (ad.NonPreviewImages is not null)
                foreach (AdImage img in ad.NonPreviewImages)
                    imagesUrls.Add(img.Source);

            return new AdDto(
                Id: ad.Id,
                Title: ad.Title,
                Description: ad.Description,
                Address: ad.Address,
                Budget: ad.Budget,
                ContactNumber: ad.ContactNumber,
                PreviewImage: ad.PreviewImageSource,
                NonPreviewImages: imagesUrls,
                CreatedOn: ad.CreatedOn,
                UpdatedOn: ad.UpdatedOn,
                AuthorName: $"{ad.Author.FirstName} {ad.Author.LastName}",
                AuthorId: ad.AuthorId,
                Status: ad.Status.Name,
                AccomplishFromDate: ad.AccomplishFromDate,
                AccomplishUntilDate: ad.AccomplishUntilDate);
        }
    }
}
