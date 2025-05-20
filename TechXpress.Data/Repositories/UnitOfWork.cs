using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
            Tokens = new TokenRepo(dp, _loggerFactory.CreateLogger<TokenRepo>());
            OrderDetails = new Repository<OrderDetail>(dp, _loggerFactory.CreateLogger<Repository<OrderDetail>>());
            WishListItems = new Repository<WishListItem>(dp, _loggerFactory.CreateLogger<Repository<WishListItem>>());
            ProductImages = new Repository<ProductImage>(dp, _loggerFactory.CreateLogger<Repository<ProductImage>>());
            Reviews = new Repository<Review>(dp, _loggerFactory.CreateLogger<Repository<Review>>());

            _logger.LogInformation("UnitOfWork initialized.");
        }
        private readonly AppDbContext _dp;
        private readonly ILogger<UnitOfWork> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private IDbContextTransaction _transaction;
        private bool _disposed = false;
        public IProductRepo Products { get; private set; }
        public IRepository<Category> Categories { get; private set; }
        public IOrderRepo Orders { get; private set; }
        public ITokenRepo Tokens { get; private set; }
        public IRepository<ProductImage> ProductImages { get; private set; }
        public IRepository<OrderDetail> OrderDetails { get; private set; }
        public IRepository<WishListItem> WishListItems { get; private set; }
        public IRepository<Review> Reviews { get; private set; }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _transaction = await _dp.Database.BeginTransactionAsync();
            return _transaction;
        }
        public void Dispose()
        {
            _logger.LogInformation("Disposing UnitOfWork.");
            //_dp.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                _logger.LogInformation("Saving changes to the database.");

                foreach (var entry in _dp.ChangeTracker.Entries())
                {
                    _logger.LogInformation($"Entity: {entry.Entity.GetType().Name}, State: {entry.State}");
                }

                var result = await _dp.SaveChangesAsync();
                _logger.LogInformation($"SaveChangesAsync result: {result}");

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in SaveAsync.");
                throw new Exception($"Error Saving Order in SaveAsync(): {ex.Message}", ex);
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _dp.Dispose();
                }
            }
            _disposed = true;
        }
    }
}
