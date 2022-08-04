using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class TrailerService : ITrailerService
    {
        private readonly JWLDBContext _jWLDBContext;

        public TrailerService(JWLDBContext jWLDBContext)
        {
            _jWLDBContext = jWLDBContext;
        }
        public async Task AddTrailer(trailer trailer, string UserID)
        {
            await _jWLDBContext.trailer.AddAsync(trailer);
            await _jWLDBContext.SaveChanges(UserID);
        }

        public async Task EditTrailer(trailer trailer, string UserID)
        {
            trailer trailer1 = await _jWLDBContext.trailer.Where(t => t.trailerId == trailer.trailerId).FirstOrDefaultAsync();
            trailer1.trailerId = trailer.trailerId;
            trailer1.trailerName = trailer.trailerName;
            trailer1.isDeleted = trailer.isDeleted;
            trailer1.isActive = trailer.isActive;
            //_jWLDBContext.trailer.Update(trailer);
            await _jWLDBContext.SaveChanges(UserID);
        }

        public async Task<List<trailer>> GetTrailer()
        {
            return await _jWLDBContext.trailer.AsNoTracking().Where(t => t.isDeleted == null || t.isDeleted == false).ToListAsync();
        }
        public async Task<List<trailer>> GetActiveTrailer()
        {
            return await _jWLDBContext.trailer.AsNoTracking().Where(t => (t.isDeleted == null || t.isDeleted == false) && (t.isActive == true)).ToListAsync();
        }
        public async Task<trailer> GetTrailer(int id)
        {
            return await _jWLDBContext.trailer.AsNoTracking().Where(t => t.trailerId == id).FirstOrDefaultAsync();
        }
    }
}
