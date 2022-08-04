using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class VehicleTypeService : IVehicleTypeService
    {
        private readonly JWLDBContext _jWLDBContext;

        public VehicleTypeService(JWLDBContext jWLDBContext)
        {
            _jWLDBContext = jWLDBContext;
        }

        public async Task AddVehicleTypes(vehicltype vehicltype, string UserID)
        {
            await _jWLDBContext.vehicltype.AddAsync(vehicltype);
            await _jWLDBContext.SaveChanges(UserID);
        }

        public async Task EditVehicleTypes(vehicltype vehicltype, string UserID)
        {
            //_jWLDBContext.vehicltype.Update(vehicltype);
            vehicltype vehicltypes = await _jWLDBContext.vehicltype.Where(t => t.vehicleId == vehicltype.vehicleId).FirstOrDefaultAsync();
            vehicltypes.vehicleId = vehicltype.vehicleId;
            vehicltypes.vehicleName = vehicltype.vehicleName;
            vehicltypes.isActive = vehicltype.isActive;
            vehicltypes.isDeleted = vehicltype.isDeleted;
            await _jWLDBContext.SaveChanges(UserID);
        }

        public async Task<List<vehicltype>> GetVehicleTypes()
        {
            return await _jWLDBContext.vehicltype.AsNoTracking().Where(t => t.isDeleted == null || t.isDeleted == false).ToListAsync();
        }
        public async Task<List<vehicltype>> GetActiveVehicleTypes()
        {
            return await _jWLDBContext.vehicltype.AsNoTracking().Where(t => (t.isDeleted == null || t.isDeleted == false) && (t.isActive == true)).ToListAsync();
        }
        public async Task<vehicltype> GetVehicleTypes(int id)
        {
            return await _jWLDBContext.vehicltype.AsNoTracking().Where(t => t.vehicleId == id).FirstOrDefaultAsync();
        }
    }
}