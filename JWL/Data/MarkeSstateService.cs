using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class MarkeSstateService : IMarkeSstateService
    {
        private readonly JWLDBContext _jWLDBContext;

        public MarkeSstateService(JWLDBContext jWLDBContext)
        {
            _jWLDBContext = jWLDBContext;
        }

        public async Task AddMarketState(marketstate marketstate, string UserID)
        {
            await _jWLDBContext.marketstate.AddAsync(marketstate);
            await _jWLDBContext.SaveChanges(UserID);
        }

        public async Task EditMarketstate(marketstate marketstate, string UserID)
        {
            //_jWLDBContext.marketstate.Update(marketstate);
            marketstate marketstate1 = await _jWLDBContext.marketstate.Where(t => t.marketStateID == marketstate.marketStateID).FirstOrDefaultAsync();
            marketstate1.marketStateID = marketstate.marketStateID;
            marketstate1.marketName = marketstate.marketName;
            marketstate1.isActive = marketstate.isActive;
            marketstate1.isDeleted = marketstate.isDeleted;
            await _jWLDBContext.SaveChanges(UserID);
        }

        public async Task<List<marketstate>> GetMarketState()
        {
            return await _jWLDBContext.marketstate.AsNoTracking().Where(t => t.isDeleted == null || t.isDeleted == false).ToListAsync();
        }
        public async Task<List<marketstate>> GetActiveMarketState()
        {
            return await _jWLDBContext.marketstate.AsNoTracking().Where(t => (t.isDeleted == null || t.isDeleted == false) && (t.isActive == true)).ToListAsync();
        }
        public async Task<marketstate> GetMarketstate(int id)
        {
            return await _jWLDBContext.marketstate.AsNoTracking().Where(t => t.marketStateID == id).FirstOrDefaultAsync();
        }
    }
}
