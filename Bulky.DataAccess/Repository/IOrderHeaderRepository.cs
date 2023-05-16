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
    }
}