using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bulky.DataAccess.Repository;

namespace Bulky.DataAccess.UoW
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        IAddressRepository Address { get; }

        void Save();
    }
}