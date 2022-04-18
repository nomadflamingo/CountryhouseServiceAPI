using CountryhouseService.API.Models;
using System.Linq.Expressions;

namespace CountryhouseService.API.Interfaces
{
    public interface IAdsRepository
    {
        Task<IEnumerable<Ad>> GetFixedAmountAsync(
            Expression<Func<Ad, bool>>? searchBy = null,
            bool trackChanges = false,
            int skip = 0,
            int? limit = null,
            params Expression<Func<Ad, object>>[] includes);

        /// <exception cref="InvalidOperationException"></exception>
        Task<Ad> GetByIdAsync(
            int id,
            bool trackChanges = false,
            params Expression<Func<Ad, object>>[] includes);

        Task<Ad?> GetByIdOrDefaultAsync(
            int id,
            bool trackChanges = false,
            params Expression<Func<Ad, object>>[] includes);

        Task<int> CreateAsync(Ad ad);

        void Update(Ad ad);

        void Remove(Ad ad);

        Task LoadOrderedImagesAsync(Ad ad);

        Task LoadRequestsAsync(
            Ad ad,
            Expression<Func<Request, object>>? orderBy = null,
            bool orderByDescending = false,
            int skip = 0,
            int? limit = null,
            params Expression<Func<Request, object>>[] includes);
    }
}
