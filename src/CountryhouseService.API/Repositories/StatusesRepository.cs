using CountryhouseService.API.Data;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CountryhouseService.API.Repositories
{
    public class StatusesRepository<TStatus> : IStatusesRepository<TStatus> where TStatus : Status
    {
        private readonly AppDbContext _db;


        public StatusesRepository(AppDbContext db)
        {
            _db = db;
        }


        public async Task<TStatus> GetAsync(string statusName)
        {
            var status = await _db.Set<TStatus>()
                .Where(s => s.Name == statusName)
                .FirstAsync();

            return status;
        }


        public async Task<ICollection<TStatus>> GetAllAsync()
        {
            var statuses = await _db.Set<TStatus>().ToListAsync();
            return statuses;
        }
    }
}
