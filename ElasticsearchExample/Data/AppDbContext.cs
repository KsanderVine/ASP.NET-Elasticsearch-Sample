using ElasticsearchExample.Models;
using Microsoft.EntityFrameworkCore;

namespace ElasticsearchExample.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<OrderItem> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            CreateCustomerModel(modelBuilder);
            CreateProductModel(modelBuilder);
            CreateOrderItemModel(modelBuilder);

            static void CreateCustomerModel(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>()
                    .Property(e => e.Id)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                modelBuilder.Entity<Customer>()
                    .Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            }

            static void CreateProductModel(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Product>()
                    .Property(e => e.Id)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                modelBuilder.Entity<Product>()
                    .Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                modelBuilder.Entity<Product>()
                    .Property(e => e.Category)
                    .HasConversion<string>();

                modelBuilder.Entity<Product>()
                    .Property(e => e.Price)
                    .HasColumnType<decimal>("money");
            }

            static void CreateOrderItemModel(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<OrderItem>()
                    .Property(e => e.Id)
                    .HasDefaultValueSql("NEWSEQUENTIALID()");

                modelBuilder.Entity<OrderItem>()
                    .Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                modelBuilder.Entity<OrderItem>()
                    .HasOne(e => e.Customer)
                    .WithMany(e => e.OrderItems)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<OrderItem>()
                    .HasOne(e => e.Product)
                    .WithMany(e => e.OrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<OrderItem>()
                    .Property(e => e.OrderPrice)
                    .HasColumnType<decimal>("money");
            }
        }
    }
}
