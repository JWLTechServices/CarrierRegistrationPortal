using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class CargoService : ICargoService
    {
        private readonly JWLDBContext _jWLDBContext;
        public CargoService(JWLDBContext jWLDBContext)
        {
            _jWLDBContext = jWLDBContext;
        }
        public async Task<List<cargospecialties>> GetCargoSpecialities()
        {
            return await _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.isDeleted == null || t.isDeleted.Value == false).ToListAsync();
        }
        public async Task<List<cargospecialties>> GetActiveCargoSpecialities()
        {
            return await _jWLDBContext.cargospecialties.AsNoTracking().Where(t => (t.isDeleted == null || t.isDeleted == false) && (t.isActive == true)).ToListAsync();
        }
        public async Task AddCargoSpecialities(cargospecialties cargospecialties, string UserID)
        {
            await _jWLDBContext.cargospecialties.AddAsync(cargospecialties);
            await _jWLDBContext.SaveChanges(UserID);
        }
        public async Task<cargospecialties> GetCargo(int id)
        {
            return await _jWLDBContext.cargospecialties.AsNoTracking().Where(t => t.cargoId == id).FirstOrDefaultAsync();
        }
        public async Task EditCargoSpecialities(cargospecialties cargospecialties, string UserID)
        {
            //_jWLDBContext.cargospecialties.Update(cargospecialties);
            cargospecialties cargospecialty = await _jWLDBContext.cargospecialties.Where(t => t.cargoId == cargospecialties.cargoId).FirstOrDefaultAsync();
            cargospecialty.cargoId = cargospecialties.cargoId;
            cargospecialty.cargoName = cargospecialties.cargoName;
            cargospecialty.isActive = cargospecialties.isActive;
            cargospecialty.isDeleted = cargospecialties.isDeleted;
            await _jWLDBContext.SaveChanges(UserID);
        }
    }
}
