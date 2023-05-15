using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bulky.DataAccess.Data;
using Bulky.Models.Identity;

namespace Bulky.DataAccess.Repository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext dbContext;
        public ApplicationUserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}