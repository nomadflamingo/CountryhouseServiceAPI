using CountryhouseService.API.Models;
using System.Linq.Expressions;

namespace CountryhouseService.API.Interfaces
{
    public interface IRequestsRepository
    {
        Task<Request?> GetByIdOrDefaultAsync(
            int id, 
            bool trackChanges = false, 
            params Expression<Func<Request, object>>[] includes);

        Task<int> CreateAsync(Request request);

        void Update(Request request);

        void Remove(Request request);
    }
}
