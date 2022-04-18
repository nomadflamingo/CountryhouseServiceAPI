using CountryhouseService.API.Models;

namespace CountryhouseService.API.Interfaces
{
    public interface IUnitOfWork
    {
        IAdsRepository AdsRepository { get; set; }
        IRequestsRepository RequestsRepository { get; set; }
        IStatusesRepository<AdStatus> AdStatusesRepository { get; set; }
        IStatusesRepository<RequestStatus> RequestStatusesRepository { get; set; }
        IImagesRepository<AdImage> AdImagesRepository { get; set; }
        IImagesRepository<Avatar> AvatarsRepository { get; set; }

        TRepository GetRepository<TRepository>();

        Task SaveChangesAsync();
    }
}
