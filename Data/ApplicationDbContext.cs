using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd_FLOWER_SHOP.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_FLOWER_SHOP.Data
{
      public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, long>
      {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }
            public DbSet<Review> Reviews { get; set; }
            public DbSet<Product> Products { get; set; }
            public DbSet<Order> Orders { get; set; }
            public DbSet<OrderItem> OrderItems { get; set; }
            public DbSet<Cart> Carts { get; set; }
            public DbSet<CartItem> CartItem { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<Address> Addresses { get; set; }
            public DbSet<ImageUpload> ImageUploads { get; set; }
            public DbSet<PricingRule> PricingRules { get; set; }
            public DbSet<ProductPricingRule> ProductPricingRules { get; set; }
            public DbSet<ProductCategory> ProductCategories { get; set; }
            public DbSet<Payment> Payments { get; set; }
            public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
            public DbSet<UserProductView> UserProductViews { get; set; }
            protected override void OnModelCreating(ModelBuilder builder)
            {
                  base.OnModelCreating(builder);

                  // ==================== IDENTITY CONFIGURATION ====================

                  // ApplicationUser Configuration
                  builder.Entity<ApplicationUser>(entity =>
                  {
                        entity.ToTable("Users");

                        // Configure relationships
                        entity.HasOne(u => u.Role)
                        .WithMany()
                        .HasForeignKey(u => u.RoleId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasMany(u => u.Addresses)
                        .WithOne(a => a.User)
                        .HasForeignKey(a => a.ApplicationUserId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasMany(u => u.Orders)
                        .WithOne(o => o.User)
                        .HasForeignKey(o => o.UserId)
                        .OnDelete(DeleteBehavior.Restrict);

                        // Configure properties
                        entity.Property(u => u.FirstName)
                        .HasMaxLength(50)
                        .IsRequired();

                        entity.Property(u => u.LastName)
                        .HasMaxLength(50)
                        .IsRequired();

                        entity.Property(u => u.Email)
                        .HasMaxLength(256)
                        .IsRequired();

                        entity.Property(u => u.UserName)
                        .HasMaxLength(256)
                        .IsRequired();

                        entity.Property(u => u.RefreshToken)
                        .HasMaxLength(500).IsRequired(false);

                        entity.Property(u => u.RefreshTokenExpiryTime)
                        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                  });
                  builder.Entity<LoyaltyTransaction>(entity =>
                  {
                        entity.ToTable("LoyaltyTransactions");
                        entity.HasOne(t => t.User)
                              .WithMany()
                              .HasForeignKey(t => t.UserId)
                              .OnDelete(DeleteBehavior.Cascade);
                        entity.Property(t => t.CreatedAt)
                              .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                  });
                  builder.Entity<Review>(entity =>
                  {
                        entity.ToTable("Reviews");

                        entity.HasOne(r => r.Product)
                              .WithMany(p => p.Reviews)
                              .HasForeignKey(r => r.ProductId)
                              .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(r => r.User)
                              .WithMany()
                              .HasForeignKey(r => r.UserId)
                              .OnDelete(DeleteBehavior.Cascade);
                  });
                  // ApplicationRole Configuration
                  builder.Entity<ApplicationRole>(entity =>
                  {
                        entity.ToTable("Roles");

                        entity.Property(r => r.Name)
                        .HasMaxLength(256)
                        .IsRequired();

                        entity.Property(r => r.CreationDate)
                        .IsRequired();

                        entity.Property(r => r.ModificationDate)
                        .IsRequired();
                  });

                  // ==================== PRODUCT CONFIGURATION ====================

                  // Product Configuration
                  builder.Entity<Product>(entity =>
                  {
                        entity.ToTable("Products");

                        entity.Property(p => p.Name)
                        .HasMaxLength(200)
                        .IsRequired();

                        entity.Property(p => p.flowerstatus)
                        .HasConversion<string>()
                        .HasMaxLength(50)
                        .IsRequired();

                        entity.Property(p => p.Description)
                        .HasMaxLength(1000);

                        entity.Property(p => p.BasePrice)
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();


                        entity.Property(p => p.StockQuantity)
                        .IsRequired();

                        entity.Property(p => p.IsActive)
                        .HasDefaultValue(true);

                        entity.Property(p => p.CreatedAt)
                        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                        entity.Property(p => p.UpdatedAt)
                        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                        // Configure relationships
                        entity.HasMany(p => p.ProductCategories)
                        .WithOne(pc => pc.Product)
                        .HasForeignKey(pc => pc.ProductId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasMany(p => p.ImageUploads)
                        .WithOne(i => i.Product)
                        .HasForeignKey(i => i.ProductId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasMany(p => p.ProductPricingRules)
                        .WithOne(pr => pr.Product)
                        .HasForeignKey(pr => pr.ProductId)
                        .OnDelete(DeleteBehavior.Cascade);
                        entity.HasMany(p => p.Reviews)
                              .WithOne(r => r.Product)
                              .HasForeignKey(r => r.ProductId)
                              .OnDelete(DeleteBehavior.Cascade);
                  });

                  // Category Configuration
                  builder.Entity<Category>(entity =>
                  {
                        entity.ToTable("Categories");

                        entity.Property(c => c.Name)
                        .HasMaxLength(100)
                        .IsRequired();

                        entity.Property(c => c.Description)
                        .HasMaxLength(500);

                        entity.Property(c => c.CreatedAt)
                        .IsRequired();

                        entity.Property(c => c.UpdatedAt)
                        .IsRequired();

                        // Configure relationships
                        entity.HasMany(c => c.ProductCategories)
                        .WithOne(pc => pc.Category)
                        .HasForeignKey(pc => pc.CategoryId)
                        .OnDelete(DeleteBehavior.Cascade);

                        // Create unique index on category name
                        entity.HasIndex(c => c.Name)
                        .IsUnique();
                  });

                  // ProductCategory Configuration (Many-to-Many)
                  builder.Entity<ProductCategory>(entity =>
                  {
                        entity.ToTable("ProductCategories");

                        entity.HasKey(pc => new { pc.ProductId, pc.CategoryId });

                        entity.HasOne(pc => pc.Product)
                        .WithMany(p => p.ProductCategories)
                        .HasForeignKey(pc => pc.ProductId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(pc => pc.Category)
                        .WithMany(c => c.ProductCategories)
                        .HasForeignKey(pc => pc.CategoryId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  // ==================== CART CONFIGURATION ====================

                  // Cart Configuration
                  builder.Entity<Cart>(entity =>
                  {
                        entity.ToTable("Carts");

                        entity.HasOne(c => c.User)
                        .WithMany()
                        .HasForeignKey(c => c.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasMany(c => c.CartItems)
                        .WithOne(ci => ci.Cart)
                        .HasForeignKey(ci => ci.CartId)
                        .OnDelete(DeleteBehavior.Cascade);

                        // Create unique index to ensure one cart per user
                        entity.HasIndex(c => c.UserId)
                        .IsUnique();
                  });

                  // CartItem Configuration
                  builder.Entity<CartItem>(entity =>
                  {
                        entity.ToTable("CartItems");

                        entity.Property(ci => ci.Price)
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();

                        entity.Property(ci => ci.Quantity)
                        .IsRequired();

                        entity.HasOne(ci => ci.Cart)
                        .WithMany(c => c.CartItems)
                        .HasForeignKey(ci => ci.CartId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(ci => ci.Product)
                        .WithMany()
                        .HasForeignKey(ci => ci.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                        // Create unique index to prevent duplicate products in same cart
                        entity.HasIndex(ci => new { ci.CartId, ci.ProductId })
                        .IsUnique();
                  });

                  // ==================== ORDER CONFIGURATION ====================

                  // Order Configuration
                  builder.Entity<Order>(entity =>
    {
          entity.ToTable("Orders");

          // Properties
          entity.Property(o => o.TrackingNumber)
              .HasMaxLength(100);

          entity.Property(o => o.OrderStatus)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          entity.Property(o => o.CreatedAt)
              .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

          entity.Property(o => o.UpdatedAt)
              .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

          entity.Property(o => o.PaymentMethod)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

          // Relationships
          entity.HasOne(o => o.User)
              .WithMany(u => u.Orders)
              .HasForeignKey(o => o.UserId)
              .OnDelete(DeleteBehavior.Restrict);

          entity.HasOne(o => o.Address)
              .WithMany()
              .HasForeignKey(o => o.AddressId)
              .OnDelete(DeleteBehavior.Restrict);

          entity.HasMany(o => o.OrderItems)
              .WithOne(oi => oi.Order)
              .HasForeignKey(oi => oi.OrderId)
              .OnDelete(DeleteBehavior.Cascade);

          entity.HasOne(o => o.Payment)
              .WithOne(p => p.Order)
              .HasForeignKey<Payment>(p => p.OrderId)
              .OnDelete(DeleteBehavior.Cascade);

          // Indexes
          entity.HasIndex(o => o.TrackingNumber)
              .IsUnique();
    });

                  // Payment configuration
                  builder.Entity<Payment>(entity =>
                  {
                        entity.ToTable("Payments");

                        // Properties
                        entity.Property(p => p.Method)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

                        entity.Property(p => p.Status)
              .HasConversion<string>()
              .HasMaxLength(50)
              .IsRequired();

                        entity.Property(p => p.Amount)
              .HasColumnType("decimal(18,2)");

                        entity.Property(p => p.CreatedAt)
              .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                        entity.Property(p => p.UpdatedAt)
              .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                        entity.Property(p => p.PaymentDetails)
              .HasColumnType("jsonb"); // For PostgreSQL, use "nvarchar(max)" for SQL Server

                        // Relationships
                        entity.HasOne(p => p.Order)
              .WithOne(o => o.Payment)
              .HasForeignKey<Payment>(p => p.OrderId)
              .OnDelete(DeleteBehavior.Cascade);

                        // Indexes
                        entity.HasIndex(p => p.TransactionId)
              .IsUnique();
                  });

                  // OrderItem Configuration
                  builder.Entity<OrderItem>(entity =>
                  {
                        entity.ToTable("OrderItems");

                        entity.Property(oi => oi.Price)
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();

                        entity.Property(oi => oi.Quantity)
                        .IsRequired();

                        entity.Property(oi => oi.Name)
                        .HasMaxLength(200);

                        entity.HasOne(oi => oi.Order)
                        .WithMany(o => o.OrderItems)
                        .HasForeignKey(oi => oi.OrderId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(oi => oi.Product)
                        .WithMany()
                        .HasForeignKey(oi => oi.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasOne(oi => oi.User)
                        .WithMany()
                        .HasForeignKey(oi => oi.UserId)
                        .OnDelete(DeleteBehavior.Restrict);
                  });

                  // ==================== ADDRESS CONFIGURATION ====================

                  builder.Entity<Address>(entity =>
                  {
                        entity.ToTable("Addresses");

                        entity.Property(a => a.FullName)
          .HasMaxLength(100)
          .IsRequired();

                        entity.Property(a => a.PhoneNumber)
          .HasMaxLength(20) // Adjust max length as needed
          .IsRequired();

                        entity.Property(a => a.StreetAddress)
          .HasMaxLength(200)
          .IsRequired();

                        entity.Property(a => a.City)
          .HasMaxLength(100)
          .IsRequired();

                        entity.HasOne(a => a.User)
          .WithMany(u => u.Addresses)
          .HasForeignKey(a => a.ApplicationUserId)
          .OnDelete(DeleteBehavior.Cascade);
                  });

                  // ==================== IMAGE UPLOAD CONFIGURATION ====================

                  // ImageUpload Configuration
                  builder.Entity<ImageUpload>(entity =>
            {
                  entity.ToTable("ImageUploads");

                  entity.Property(i => i.ImageUrl)
                      .HasMaxLength(500)
                      .IsRequired();

                  entity.Property(i => i.PublicId)
                      .HasMaxLength(255)
                      .IsRequired();

                  // Configure relationship with Product
                  entity.HasOne(i => i.Product)
                      .WithMany(p => p.ImageUploads)
                      .HasForeignKey(i => i.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

                  // ==================== PRICING RULE CONFIGURATION ====================
                  builder.Entity<ProductPricingRule>()
        .HasKey(ppr => new { ppr.ProductId, ppr.PricingRuleId });

                  builder.Entity<ProductPricingRule>()
                      .HasOne(ppr => ppr.Product)
                      .WithMany(p => p.ProductPricingRules)
                      .HasForeignKey(ppr => ppr.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                  builder.Entity<ProductPricingRule>()
                      .HasOne(ppr => ppr.PricingRule)
                      .WithMany(pr => pr.ProductPricingRules)
                      .HasForeignKey(ppr => ppr.PricingRuleId)
                      .OnDelete(DeleteBehavior.Cascade);
                  // PricingRule Configuration
                  builder.Entity<PricingRule>(entity =>
                  {
                        entity.ToTable("PricingRules");

                        entity.HasKey(pr => pr.PricingRuleId);

                        entity.Property(pr => pr.FlowerStatus) // Changed from Condition
            .HasConversion<string>() // Convert enum to string for database
            .IsRequired(false)
            .HasMaxLength(50);

                        entity.Property(pr => pr.Description)
                        .HasMaxLength(500);

                        entity.Property(pr => pr.SpecialDay)
                        .IsRequired(false)
                        .HasMaxLength(100);

                        entity.Property(pr => pr.PriceMultiplier)
                        .HasColumnType("decimal(18,4)")
                        .IsRequired();

                        entity.Property(pr => pr.FixedPrice)
                        .HasColumnType("decimal(18,2)");

                        entity.Property(pr => pr.Priority)
                        .IsRequired();

                        entity.Property(pr => pr.CreatedAt)
                        .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

                  });

                  builder.Entity<ProductPricingRule>(entity =>
                        {
                              entity.Property(e => e.CreatedAt)
                                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                        });
                  // ==================== SEED DATA ====================

                  // Seed Roles
                  var roles = new List<ApplicationRole>
                        {
                              new ApplicationRole("Admin")
                              {
                                    Id = 1,
                                    NormalizedName = "ADMIN",
                                    CreationDate = DateTime.UtcNow,
                                    ModificationDate = DateTime.UtcNow,
                                    ConcurrencyStamp = Guid.NewGuid().ToString()
                              },
                              new ApplicationRole("User")
                              {
                                    Id = 2,
                                    NormalizedName = "USER",
                                    CreationDate = DateTime.UtcNow,
                                    ModificationDate = DateTime.UtcNow,
                                    ConcurrencyStamp = Guid.NewGuid().ToString()
                              }
                        };

                  builder.Entity<ApplicationRole>().HasData(roles);

                  builder.Entity<UserProductView>(entity =>
                  {
                        entity.HasKey(e => e.Id);
                        entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
                        entity.HasIndex(e => e.ViewedAt);

                        entity.HasOne(e => e.User)
                              .WithMany()
                              .HasForeignKey(e => e.UserId)
                              .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(e => e.Product)
                              .WithMany()
                              .HasForeignKey(e => e.ProductId)
                              .OnDelete(DeleteBehavior.Cascade);
                  });
            }
      }
}