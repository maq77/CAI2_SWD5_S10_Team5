﻿using System;
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
        public UnitOfWork(AppDbContext dp)
        {
            _dp = dp;
            Products = new ProductRepo(dp);
            Categories = new Repository<Category>(dp);
            Orders = new OrderRepo(dp);
            OrderDetails = new Repository<OrderDetail>(dp);
            ProductImages = new Repository<ProductImage>(dp);

        }
        private readonly AppDbContext _dp;
        public IProductRepo Products { get; private set; }
        public IRepository<Category> Categories { get; private set; }
        public IOrderRepo Orders { get; private set; }
        public IRepository<ProductImage> ProductImages { get; private set; }
        public IRepository<OrderDetail> OrderDetails { get; private set; }

        public void Dispose()
        {
            _dp.Dispose();
        }

        public async Task<bool> SaveAsync()
        {
            return await _dp.SaveChangesAsync() > 0;
        }
    }
}
