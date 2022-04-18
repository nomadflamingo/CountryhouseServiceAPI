using CountryhouseService.API.Models;

namespace CountryhouseService.API.Interfaces
{
    public interface IStatusesRepository<TStatus> where TStatus : Status
    {
        Task<TStatus> GetAsync(string statusName);

        Task<ICollection<TStatus>> GetAllAsync();
    }
}
