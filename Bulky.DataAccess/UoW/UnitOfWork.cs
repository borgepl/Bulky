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

        public IProductRepository Product { get; private set;}
        public IAddressRepository Address { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
            this.Category = new CategoryRepository(dbContext);
            this.Product = new ProductRepository(dbContext);
            this.Address = new AddressRepository(dbContext);
            this.ShoppingCart = new ShoppingCartRepository(dbContext);
            this.ApplicationUser = new ApplicationUserRepository(dbContext);
        }
        

        public void Save()
        {
            dbContext.SaveChangesAsync();
        }
    }
}