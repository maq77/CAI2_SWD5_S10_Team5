using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechXpress.Data.Model;

namespace TechXpress.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<UserImage> UsersImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<WishListItem> WishlistItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<TokenInfo> TokenInfos { get; set; }
        public DbSet<AppSetting> AppSettings { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ConfigureProduct(builder);
            ConfigureReview(builder);
            ConfigureOrder(builder);
            ConfigureOrderDetail(builder);
            ConfigureProductImage(builder);
            ConfigureUserImage(builder);
            ConfigureWishlistItem(builder);
            ConfigureToken(builder);
        }
        private void ConfigureUser(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.FirstName)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(u => u.LastName)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(u => u.Address)
                      .IsRequired()
                      .HasMaxLength(200);
                entity.Property(u => u.City)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(u => u.Country)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(u => u.PostalCode)
                      .HasMaxLength(20);
            });
            builder.Entity<User>()
                .Navigation(p => p.UserImage).AutoInclude();
        }
        private void ConfigureUserImage(ModelBuilder builder)
        {
            builder.Entity<UserImage>(entity =>
            {
                entity.HasKey(ui => ui.Id);
                entity.Property(ui => ui.UserId)
                      .IsRequired();
                entity.Property(ui => ui.ImagePath)
                      .IsRequired();
                entity.HasOne(ui => ui.User)
                      .WithOne(u => u.UserImage)
                      .HasForeignKey<UserImage>(ui => ui.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
        private void ConfigureReview(ModelBuilder builder)
        {
            builder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasAlternateKey(r => new { r.UserId, r.ProductId });
                entity.Property(r => r.UserId)
                      .IsRequired();
                entity.Property(r => r.ProductId)
                      .IsRequired();
                entity.Property(r => r.Rating)
                      .IsRequired();
                entity.Property(r => r.Comment)
                      .HasMaxLength(1000);
                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(r => r.Product)
                      .WithMany()
                      .HasForeignKey(r => r.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(c => c.Rating)
                      .HasColumnType("TINYINT");
            });
        }
        private void ConfigureToken(ModelBuilder builder)
        {
            builder.Entity<TokenInfo>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.UserId)
                      .IsRequired()
                      .HasMaxLength(450);
                entity.Property(t => t.RefreshToken)
                      .IsRequired()
                      .HasMaxLength(256);
                entity.Property(t => t.ExpiryDate)
                      .IsRequired();
                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

            });
        }
        private void ConfigureProduct(ModelBuilder builder)
        {
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Product>()
                .Navigation(p => p.Category).AutoInclude();

            builder.Entity<Product>()
                .Navigation(p => p.Images).AutoInclude();
        }

        private void ConfigureOrder(ModelBuilder builder)
        {
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .Navigation(o => o.OrderDetails).AutoInclude();
        }

        private void ConfigureOrderDetail(ModelBuilder builder)
        {
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureProductImage(ModelBuilder builder)
        {
            builder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureWishlistItem(ModelBuilder builder)
        {
            builder.Entity<WishListItem>(entity =>
            {
                entity.HasKey(w => w.Id);

                entity.Property(w => w.UserId)
                      .IsRequired()
                      .HasMaxLength(450);

                entity.Property(w => w.DateAdded)
                      .IsRequired();

                entity.HasOne(w => w.Product)
                      .WithMany()
                      .HasForeignKey(w => w.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(w => new { w.ProductId, w.UserId })
                      .IsUnique();
            });
        }
    }
}
