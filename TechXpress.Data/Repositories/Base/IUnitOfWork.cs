using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Data.Repositories.Base
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepo Products { get; }
        IRepository<ProductImage> ProductImages { get; }
        IRepository<Category> Categories { get; }
        IRepository<OrderDetail> OrderDetails { get; }
        Task<bool> SaveAsync();

    }
}
