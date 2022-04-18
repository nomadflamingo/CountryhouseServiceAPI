using CountryhouseService.API.Data;
using CountryhouseService.API.Extensions;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CountryhouseService.API.Repositories
{
    public class AdsRepository : IAdsRepository
    {
        private readonly AppDbContext _db;

        public AdsRepository(AppDbContext db)
        {
            _db = db;
        }


        public async Task<IEnumerable<Ad>> GetFixedAmountAsync(
            Expression<Func<Ad, bool>>? searchBy = null,
            bool trackChanges = false,
            int skip = 0,
            int? limit = null,
            params Expression<Func<Ad, object>>[] includes)
        {
            IQueryable<Ad> ads = _db.Ads.IncludeMultiple(includes);

            ads = searchBy is null ? ads : ads.Where(searchBy);
            ads = trackChanges ? ads : ads.AsNoTracking();
            ads = ads.Skip(skip);
            ads = limit is null ? ads : ads.Take((int)limit);

            return await ads.ToListAsync();
        }


        public async Task<Ad> GetByIdAsync(
           int id,
           bool trackChanges = false,
           params Expression<Func<Ad, object>>[] includes)
        {
            IQueryable<Ad> ads = _db.Ads.IncludeMultiple(includes);
            ads = trackChanges ? ads : ads.AsNoTracking();
            return await ads.FirstAsync(ad => ad.Id == id);
        }


        public async Task<Ad?> GetByIdOrDefaultAsync(
            int id,
            bool trackChanges = false,
            params Expression<Func<Ad, object>>[] includes)
        {
            IQueryable<Ad> ads = _db.Ads.IncludeMultiple(includes);
            ads = trackChanges ? ads : ads.AsNoTracking();
            return await ads.FirstOrDefaultAsync(ad => ad.Id == id);
        }


        public async Task<int> CreateAsync(Ad ad)
        {
            await _db.Ads.AddAsync(ad);
            return ad.Id;
        }


        public void Update(Ad ad)
        {
            _db.Ads.Update(ad);
        }


        public void Remove(Ad ad)
        {
            _db.Ads.Remove(ad);
        }


        public async Task LoadOrderedImagesAsync(Ad ad)
        {
            await _db.Entry(ad)
                .Collection(ad => ad.NonPreviewImages)
                .Query()
                .OrderBy(img => img.Order)
                .LoadAsync();
        }


        public async Task LoadRequestsAsync(
            Ad ad,
            Expression<Func<Request, object>>? orderBy = null,
            bool orderByDescending = false,
            int skip = 0,
            int? limit = null,
            params Expression<Func<Request, object>>[] includes)
        {
            IQueryable<Request> query = _db.Entry(ad)
                .Collection(ad => ad.Requests).Query();

            if (orderBy is not null)
            {
                if (orderByDescending)
                    query = query.OrderByDescending(orderBy);
                else 
                    query = query.OrderBy(orderBy);
            }

            query = query.IncludeMultiple(includes).Skip(skip);

            query = limit is null ? query : query.Take((int)limit);
                
            await query.LoadAsync();
        }
    }
}
