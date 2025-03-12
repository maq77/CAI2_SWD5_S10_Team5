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
        public async Task<IEnumerable<Order>> GetAllOrders(Expression<Func<Order, bool>>? filter = null, string[]? includes = null)
        {
            IQueryable<Order> query = _dp.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(d => d.Product!)
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }
            if(query is null){
                throw new InvalidOperationException("There is no Orders!");
            }

            var orders = await query.ToListAsync();

            /*if (orders.Count == 0) // Check if the result set is empty
            {
                throw new InvalidOperationException("There are no orders available!");
            }*/
            return orders ?? new List<Order>();
        }
    }
}
