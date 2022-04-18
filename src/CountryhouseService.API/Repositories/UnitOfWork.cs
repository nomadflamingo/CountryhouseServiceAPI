using CountryhouseService.API.Data;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CountryhouseService.API.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public IAdsRepository AdsRepository { get; set; }
        public IStatusesRepository<AdStatus> AdStatusesRepository { get; set; }
        public IStatusesRepository<RequestStatus> RequestStatusesRepository { get; set; }
        public IImagesRepository<AdImage> AdImagesRepository { get; set; }
        public IImagesRepository<Avatar> AvatarsRepository { get; set; }
        public IRequestsRepository RequestsRepository { get; set; }


        private readonly AppDbContext _db;


        public UnitOfWork(
            IAdsRepository adsRepository,
            IStatusesRepository<AdStatus> adStatusesRepository,
            IStatusesRepository<RequestStatus> requestStatusesRepository,
            IImagesRepository<AdImage> adImagesRepository,
            IImagesRepository<Avatar> avatarsRepository,
            IRequestsRepository requestsRepository,
            AppDbContext db)
        {
            AdsRepository = adsRepository;
            AdStatusesRepository = adStatusesRepository;
            RequestStatusesRepository = requestStatusesRepository;
            AdImagesRepository = adImagesRepository;
            AvatarsRepository = avatarsRepository;
            RequestsRepository = requestsRepository;
            _db = db;
        }

        public TRepository GetRepository<TRepository>()
        {
            var properties = GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property is TRepository repository)
                {
                    return repository;
                }
            }
            throw new ArgumentException("Repository type is not recognised");
        }


        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
