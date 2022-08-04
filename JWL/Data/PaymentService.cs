using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class PaymentService : IPaymentService
    {
        private readonly JWLDBContext _jWLDBContext;

        public PaymentService(JWLDBContext jWLDBContext)
        {
            _jWLDBContext = jWLDBContext;
        }

        public async Task Addpaymenttype(paymenttype paymenttype, string UserID)
        {
            await _jWLDBContext.paymenttype.AddAsync(paymenttype);
            await _jWLDBContext.SaveChanges(UserID);
        }

        public async Task Editpaymenttype(paymenttype paymenttype, string UserID)
        {
            paymenttype paymenttype1 = await _jWLDBContext.paymenttype.Where(t => t.paymentTypeId == paymenttype.paymentTypeId).FirstOrDefaultAsync();
            paymenttype1.paymentTypeId = paymenttype.paymentTypeId;
            paymenttype1.paymentName = paymenttype.paymentName;
            paymenttype1.isActive = paymenttype.isActive;
            paymenttype1.isDeleted = paymenttype.isDeleted;
            await _jWLDBContext.SaveChanges(UserID);
        }

        public async Task<List<paymenttype>> GetPaymentTypes()
        {
            return await _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.isDeleted == null || t.isDeleted == false).ToListAsync();

        }
        public async Task<List<paymenttype>> GetActivePaymentTypes()
        {
            return await _jWLDBContext.paymenttype.AsNoTracking().Where(t => (t.isDeleted == null || t.isDeleted == false) && (t.isActive == true)).ToListAsync();
        }
        public async Task<paymenttype> GetPaymentTypes(int id)
        {
            return await _jWLDBContext.paymenttype.AsNoTracking().Where(t => t.paymentTypeId == id).FirstOrDefaultAsync();
        }
    }
}
