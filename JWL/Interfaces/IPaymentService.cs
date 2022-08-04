using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IPaymentService
    {
        Task<List<paymenttype>> GetPaymentTypes();
        Task<List<paymenttype>> GetActivePaymentTypes();
        Task Addpaymenttype(paymenttype paymenttype, string UserID);
        Task<paymenttype> GetPaymentTypes(int id);
        Task Editpaymenttype(paymenttype paymenttype, string UserID);
    }
}
