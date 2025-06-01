using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Data.Repositories.Base
{
    public interface IProductRepo : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByIds(IEnumerable<int> ids);
        Task<IEnumerable<Product>> GetAllProducts(Expression<Func<Product, bool>>? filter = null, string[]? includes = null);

        //override Task<IEnumerable<Product>> GetAll(Expression<Func<Product, bool>>? filter = null, string[]? includes = null);
        Task<IEnumerable<Product>> GetProductsByCategoryId(int CategoryId);
    }
}
