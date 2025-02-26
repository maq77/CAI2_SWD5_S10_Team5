using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Data.Repositories.Base
{
    public interface IOrderRepo : IRepository<Order>
    {
        public Task<IEnumerable<Order>> GetAllOrders(Expression<Func<Order, bool>>? filter = null, string[]? includes = null);
    }
}
