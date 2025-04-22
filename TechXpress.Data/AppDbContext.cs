using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Data.Model;

namespace TechXpress.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        public DbSet<Product>  Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<WishListItem> WishlistItems { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId) 
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Product>()
                 .Navigation(p => p.Category).AutoInclude();

            
            builder.Entity<Product>()
                .Navigation(p => p.Images).AutoInclude();

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .Navigation(p => p.OrderDetails).AutoInclude();

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductImage>()
                .HasOne(p => p.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            //builder.Entity<WishListItem>()
            //    .HasOne(w => w.Product)
            //    .WithMany() 
            //    .HasForeignKey(w => w.ProductId)
            //    .OnDelete(DeleteBehavior.Cascade); 

            builder.Entity<WishListItem>(entity =>
            {
                
                entity.HasKey(w => w.Id); 

                entity.Property(w => w.UserId)
                      .IsRequired()
                      .HasMaxLength(450); // Same as Identity UserId

                entity.Property(w => w.DateAdded)
                      .IsRequired();

                entity.HasOne(w => w.Product)
                      .WithMany() // No navigation back to WishlistItem
                      .HasForeignKey(w => w.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(w => new { w.ProductId, w.UserId })
                      .IsUnique(); // Prevent duplicate Wishlist entries
            });

        }

    }
}
