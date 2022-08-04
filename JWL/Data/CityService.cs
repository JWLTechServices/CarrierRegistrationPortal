using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class CityService : ICityService
    {
        private readonly JWLDBContext _jWLDBContext;

        public CityService(JWLDBContext jWLDBContext)
        {
            _jWLDBContext = jWLDBContext;
        }

        public async Task AddCities(city city, string UserID)
        {
            await _jWLDBContext.city.AddAsync(city);
            await _jWLDBContext.SaveChanges(UserID);
        }

        public async Task EditCity(city city, string UserID)
        {
            //_jWLDBContext.city.Update(city);
            city cities = await _jWLDBContext.city.Where(t => t.cityId == city.cityId).FirstOrDefaultAsync();
            cities.cityId = city.cityId;
            cities.cityName = city.cityName;
            cities.stateId = city.stateId;
            cities.isActive = city.isActive;
            cities.isDeleted = city.isDeleted;
            await _jWLDBContext.SaveChanges(UserID);
        }
        public async Task<city> GetCity(int id)
        {
            return await _jWLDBContext.city.AsNoTracking().Where(t => t.cityId == id).FirstOrDefaultAsync();

        }

        public async Task<List<city>> GetCities()
        {


            return await _jWLDBContext.city.AsNoTracking().Include(t => t.State).Where(t => t.isDeleted == null || t.isDeleted == false).ToListAsync();
        }
        public async Task<List<city>> GetActiveCities()
        {
            return await _jWLDBContext.city.AsNoTracking().Include(t => t.State).Where(t => (t.isDeleted == null || t.isDeleted == false) && (t.isActive == true)).ToListAsync();
        }
        public async Task<List<city>> GetCitiesById(int id)
        {
            return await _jWLDBContext.city.AsNoTracking().Where(t => (t.stateId == id) && (t.isDeleted == null || t.isDeleted.Value == false) && (t.isActive == true)).ToListAsync();
        }
    }
}
