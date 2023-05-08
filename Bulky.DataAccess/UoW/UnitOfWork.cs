using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;

namespace Bulky.DataAccess.UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext dbContext;
        public ICategoryRepository Category { get; private set;}
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.Category = new CategoryRepository(dbContext);
        }
        

        public void Save()
        {
            dbContext.SaveChangesAsync();
        }
    }
}