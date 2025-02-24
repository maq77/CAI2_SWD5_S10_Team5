using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Data.Repositories.Base
{
    public interface IProductRepo : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryId(int CategoryId);
    }
}
