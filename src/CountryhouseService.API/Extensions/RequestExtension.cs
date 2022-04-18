using CountryhouseService.API.Dtos;
using CountryhouseService.API.Models;

namespace CountryhouseService.API.Extensions
{
    public static class RequestExtension
    {
        public static RequestDto AsDto(this Request rq) =>
            new(
                Id: rq.Id,
                Comment: rq.Comment,
                Status: rq.Status.Name,
                ContractorId: rq.ContractorId,
                ContractorName: $"{rq.Contractor.FirstName} {rq.Contractor.LastName}",
                ContractorAvatar: rq.Contractor.PreviewAvatarSource,
                ContactNumber: rq.Contractor.PhoneNumber,
                Email: rq.Contractor.Email,
                AdTitle: rq.Ad.Title,
                AdDescription: rq.Ad.Description,
                AdPreviewImageSource: rq.Ad.PreviewImageSource,
                CreatedOn: rq.CreatedOn,
                UpdatedOn: rq.UpdatedOn);
    }
}
