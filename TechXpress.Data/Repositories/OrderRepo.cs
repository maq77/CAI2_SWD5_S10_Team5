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
    public class OrderRepo : Repository<Order>, IOrderRepo
    {
        private readonly AppDbContext _dp;

        public OrderRepo(AppDbContext dp, ILogger<Repository<Order>> logger) : base(dp, logger)
        {
            _dp = dp;
        }
        public async Task<List<Order>> GetAllOrders(Expression<Func<Order, bool>>? filter = null, string[]? includes = null)
        {
            IQueryable<Order> query = _dp.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(d => d.Product)
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Handle includes dynamically
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            var orders = await query.ToListAsync(); // Execute the query

            if (!orders.Any()) // Check if the result set is empty
            {
                return new List<Order>();
            }
            return orders;
        }
    }
}
