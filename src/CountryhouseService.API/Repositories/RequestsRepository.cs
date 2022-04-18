using CountryhouseService.API.Data;
using CountryhouseService.API.Extensions;
using CountryhouseService.API.Interfaces;
using CountryhouseService.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CountryhouseService.API.Repositories
{
    public class RequestsRepository : IRequestsRepository
    {
        private readonly AppDbContext _db;

        public RequestsRepository(AppDbContext db)
        {
            _db = db;
        }


        public async Task<Request?> GetByIdOrDefaultAsync(
            int id, 
            bool trackChanges = false,
            params Expression<Func<Request, object>>[] includes)
        {
            IQueryable<Request> requests = _db.Requests.IncludeMultiple(includes);

            requests = trackChanges ? requests : requests.AsNoTracking();

            return await requests.FirstOrDefaultAsync(rq => rq.Id == id);
        }


        public async Task<int> CreateAsync(Request request)
        {
            await _db.Requests.AddAsync(request);
            return request.Id;
        }


        public void Remove(Request request)
        {
            _db.Requests.Remove(request);
        }


        public void Update(Request request)
        {
            _db.Requests.Update(request);
        }
    }
}
