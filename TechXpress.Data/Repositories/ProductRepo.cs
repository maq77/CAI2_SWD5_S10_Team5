using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;

namespace TechXpress.Data.Repositories
{
    public class ProductRepo : Repository<Product>, IProductRepo
    {
        public ProductRepo(AppDbContext dp, ILogger<Repository<Product>> logger) : base(dp, logger)
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
        public async Task<IEnumerable<Product>> GetAllProducts(Expression<Func<Product, bool>>? filter = null, string[]? includes = null)
        {
            IQueryable<Product> query = _dp.Products
                .Include(p => p.Category)   //  Always include Category
                .Include(p => p.Images)     //  Always include Images
                .AsNoTracking();            //

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.ToListAsync();
        }
        
    }
}
