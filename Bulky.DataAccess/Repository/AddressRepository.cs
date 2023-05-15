using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bulky.DataAccess.Data;
using Bulky.Models.Identity;

namespace Bulky.DataAccess.Repository
{
    public class AddressRepository : Repository<Address>, IAddressRepository
    {
        public AddressRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}