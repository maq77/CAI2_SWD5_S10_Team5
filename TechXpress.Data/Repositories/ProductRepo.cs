using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;

namespace TechXpress.Data.Repositories
{
    public class ProductRepo : Repository<Product>, IProductRepo
    {
        public ProductRepo(AppDbContext dp)  : base(dp)
        {
            _dp = dp;
        }
        private readonly AppDbContext _dp;
        public async Task<IEnumerable<Product>> GetProductsByCategoryId(int CategoryId)
        {
            return await _dp.Products
                            .Include(p=>p.Category) ///eager loading
                            .Where(p=>p.CategoryId == CategoryId)
                            .ToListAsync();
        }
    }
}
