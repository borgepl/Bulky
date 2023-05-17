using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bulky.Models;

namespace Bulky.DataAccess.Repository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int id, string orderStatus, string paymentStatus = null );

        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId );
    }
}