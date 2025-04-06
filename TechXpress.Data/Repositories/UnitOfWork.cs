using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;
using TechXpress.Data.Repositories.Base;

namespace TechXpress.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(AppDbContext dp, ILogger<UnitOfWork> logger, ILoggerFactory loggerFactory)
        {
            _dp = dp;
            _logger = logger;
            _loggerFactory = loggerFactory;

            Products = new ProductRepo(dp, _loggerFactory.CreateLogger<ProductRepo>());
            Categories = new Repository<Category>(dp, _loggerFactory.CreateLogger<Repository<Category>>());
            Orders = new OrderRepo(dp, _loggerFactory.CreateLogger<OrderRepo>());
            OrderDetails = new Repository<OrderDetail>(dp, _loggerFactory.CreateLogger<Repository<OrderDetail>>());
            WishListItems = new Repository<WishListItem>(dp, _loggerFactory.CreateLogger<Repository<WishListItem>>());
            ProductImages = new Repository<ProductImage>(dp, _loggerFactory.CreateLogger<Repository<ProductImage>>());

            _logger.LogInformation("UnitOfWork initialized.");
        }
        private readonly AppDbContext _dp;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly ILoggerFactory _loggerFactory;
        public IProductRepo Products { get; private set; }
        public IRepository<Category> Categories { get; private set; }
        public IOrderRepo Orders { get; private set; }
        public IRepository<ProductImage> ProductImages { get; private set; }
        public IRepository<OrderDetail> OrderDetails { get; private set; }
        public IRepository<WishListItem> WishListItems { get; private set; }

        public void Dispose()
        {
            _logger.LogInformation("Disposing UnitOfWork.");
            _dp.Dispose();
        }

        public async Task<bool> SaveAsync()
        {
            _logger.LogInformation("Saving changes to the database.");
            return await _dp.SaveChangesAsync() > 0;
        }
    }
}
