using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        IOrderRepo Orders { get; }
        IErrorLogRepo ErrorLogs { get; }
        ITokenRepo Tokens { get; }
        IRepository<ProductImage> ProductImages { get; }
        IRepository<UserImage> UsersImages { get; }
        IRepository<Category> Categories { get; }
        IRepository<OrderDetail> OrderDetails { get; }
        IRepository<WishListItem> WishListItems{ get; }
        IRepository<Review> Reviews{ get; }
        IRepository<AppSetting> AppSettings{ get; }


        Task<IDbContextTransaction> BeginTransactionAsync();
        Task BeginTransactionAsync_();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<T> ExecuteWithStrategyAsync_<T>(Func<Task<T>> operation);
        Task<IExecutionStrategy> ExecuteWithStrategyAsync();
        Task<bool> SaveAsync();
        Task<int> SaveChangesCountAsync();
        DbContext GetContext();

    }
}
