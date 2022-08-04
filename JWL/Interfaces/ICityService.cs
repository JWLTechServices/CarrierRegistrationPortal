using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ICityService
    {
        Task<List<city>> GetCities();
        Task<List<city>> GetActiveCities();
        Task<List<city>> GetCitiesById(int id);
        Task AddCities(city city, string UserId);
        Task<city> GetCity(int id);
        Task EditCity(city city, string UserId);

    }
}
