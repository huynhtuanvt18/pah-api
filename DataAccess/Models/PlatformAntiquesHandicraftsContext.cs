using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DataAccess.Models
{
    public partial class PlatformAntiquesHandicraftsContext : DbContext
    {
        public PlatformAntiquesHandicraftsContext()
        {
        }

        public PlatformAntiquesHandicraftsContext(DbContextOptions<PlatformAntiquesHandicraftsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ActivationRequest> ActivationRequests { get; set; } = null!;
        public virtual DbSet<Address> Addresses { get; set; } = null!;
        public virtual DbSet<Auction> Auctions { get; set; } = null!;
        public virtual DbSet<Bid> Bids { get; set; } = null!;
        public virtual DbSet<Buyer> Buyers { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Feedback> Feedbacks { get; set; } = null!;
        public virtual DbSet<Material> Materials { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderCancellation> OrderCancellations { get; set; } = null!;
        public virtual DbSet<OrderItem> OrderItems { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ProductImage> ProductImages { get; set; } = null!;
        public virtual DbSet<Response> Responses { get; set; } = null!;
        public virtual DbSet<Seller> Sellers { get; set; } = null!;
        public virtual DbSet<Token> Tokens { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<VerifyToken> VerifyTokens { get; set; } = null!;
        public virtual DbSet<Wallet> Wallets { get; set; } = null!;
        public virtual DbSet<Withdrawal> Withdrawals { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("server=(local);database=PlatformAntiquesHandicrafts;uid=sa;pwd=12345678;TrustServerCertificate=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActivationRequest>(entity =>
            {
                entity.ToTable("ActivationRequest");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Content)
                    .HasMaxLength(500)
                    .HasColumnName("content");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.StaffId).HasColumnName("staffId");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime")
                    .HasColumnName("timestamp");
            });

            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("createdAt");

                entity.Property(e => e.CustomerId).HasColumnName("customerId");

                entity.Property(e => e.District)
                    .HasMaxLength(255)
                    .HasColumnName("district");

                entity.Property(e => e.DistrictId).HasColumnName("districtId");

                entity.Property(e => e.IsDefault).HasColumnName("isDefault");

                entity.Property(e => e.Province)
                    .HasMaxLength(255)
                    .HasColumnName("province");

                entity.Property(e => e.ProvinceId).HasColumnName("provinceId");

                entity.Property(e => e.RecipientName)
                    .HasMaxLength(255)
                    .HasColumnName("recipientName");

                entity.Property(e => e.RecipientPhone)
                    .HasMaxLength(255)
                    .HasColumnName("recipientPhone");

                entity.Property(e => e.Street)
                    .HasMaxLength(255)
                    .HasColumnName("street");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedAt");

                entity.Property(e => e.Ward)
                    .HasMaxLength(255)
                    .HasColumnName("ward");

                entity.Property(e => e.WardCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("wardCode");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK__Address__custome__5CD6CB2B");
            });

            modelBuilder.Entity<Auction>(entity =>
            {
                entity.ToTable("Auction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("createdAt");

                entity.Property(e => e.EndedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("endedAt");

                entity.Property(e => e.EntryFee)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("entryFee");

                entity.Property(e => e.MaxEndedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("maxEndedAt");

                entity.Property(e => e.ProductId).HasColumnName("productId");

                entity.Property(e => e.RegistrationEnd)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationEnd");

                entity.Property(e => e.RegistrationStart)
                    .HasColumnType("datetime")
                    .HasColumnName("registrationStart");

                entity.Property(e => e.StaffId).HasColumnName("staffId");

                entity.Property(e => e.StartedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("startedAt");

                entity.Property(e => e.StartingPrice)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("startingPrice");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Step)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("step");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedAt");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Auctions)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__Auction__product__5DCAEF64");

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Auctions)
                    .HasForeignKey(d => d.StaffId)
                    .HasConstraintName("FK__Auction__staffId__5EBF139D");
            });

            modelBuilder.Entity<Bid>(entity =>
            {
                entity.ToTable("Bid");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AuctionId).HasColumnName("auctionId");

                entity.Property(e => e.BidAmount)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("bidAmount");

                entity.Property(e => e.BidDate)
                    .HasColumnType("datetime")
                    .HasColumnName("bidDate");

                entity.Property(e => e.BidderId).HasColumnName("bidderId");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.HasOne(d => d.Auction)
                    .WithMany(p => p.Bids)
                    .HasForeignKey(d => d.AuctionId)
                    .HasConstraintName("FK__Bid__auctionId__5FB337D6");

                entity.HasOne(d => d.Bidder)
                    .WithMany(p => p.Bids)
                    .HasForeignKey(d => d.BidderId)
                    .HasConstraintName("FK__Bid__bidderId__60A75C0F");
            });

            modelBuilder.Entity<Buyer>(entity =>
            {
                entity.ToTable("Buyer");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.JoinedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("joinedAt");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.Buyer)
                    .HasForeignKey<Buyer>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Buyer__id__619B8048");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("createdAt");

                entity.Property(e => e.ImageUrl).HasColumnName("imageUrl");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedAt");
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("Feedback");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BuyerFeedback).HasColumnName("buyerFeedback");

                entity.Property(e => e.BuyerId).HasColumnName("buyerId");

                entity.Property(e => e.ProductId).HasColumnName("productId");

                entity.Property(e => e.Ratings).HasColumnName("ratings");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime")
                    .HasColumnName("timestamp");

                entity.HasOne(d => d.Buyer)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.BuyerId)
                    .HasConstraintName("FK__Feedback__buyerI__628FA481");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Feedbacks)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__Feedback__produc__6383C8BA");
            });

            modelBuilder.Entity<Material>(entity =>
            {
                entity.ToTable("Material");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("createdAt");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedAt");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BuyerId).HasColumnName("buyerId");

                entity.Property(e => e.OrderDate)
                    .HasColumnType("datetime")
                    .HasColumnName("orderDate");

                entity.Property(e => e.OrderShippingCode)
                    .HasMaxLength(20)
                    .HasColumnName("orderShippingCode");

                entity.Property(e => e.RecipientAddress).HasColumnName("recipientAddress");

                entity.Property(e => e.RecipientName)
                    .HasMaxLength(255)
                    .HasColumnName("recipientName");

                entity.Property(e => e.RecipientPhone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("recipientPhone")
                    .IsFixedLength();

                entity.Property(e => e.SellerId).HasColumnName("sellerId");

                entity.Property(e => e.ShippingCost)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("shippingCost");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("totalAmount");

                entity.HasOne(d => d.Buyer)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.BuyerId)
                    .HasConstraintName("FK__Order__buyerId__6477ECF3");

                entity.HasOne(d => d.Seller)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.SellerId)
                    .HasConstraintName("FK__Order__sellerId__656C112C");
            });

            modelBuilder.Entity<OrderCancellation>(entity =>
            {
                entity.ToTable("OrderCancellation");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Reason)
                    .HasMaxLength(500)
                    .HasColumnName("reason");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.OrderCancellation)
                    .HasForeignKey<OrderCancellation>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__OrderCancell__id__66603565");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.ProductId })
                    .HasName("PK__OrderIte__BAD83E4B11B80E9F");

                entity.ToTable("OrderItem");

                entity.Property(e => e.OrderId).HasColumnName("orderId");

                entity.Property(e => e.ProductId).HasColumnName("productId");

                entity.Property(e => e.ImageUrl).HasColumnName("imageUrl");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("price");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__OrderItem__order__6754599E");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__OrderItem__produ__68487DD7");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CategoryId).HasColumnName("categoryId");

                entity.Property(e => e.Condition).HasColumnName("condition");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("createdAt");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Dimension)
                    .HasMaxLength(255)
                    .HasColumnName("dimension");

                entity.Property(e => e.MaterialId).HasColumnName("materialId");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Origin)
                    .HasMaxLength(255)
                    .HasColumnName("origin");

                entity.Property(e => e.PackageContent)
                    .HasMaxLength(255)
                    .HasColumnName("packageContent");

                entity.Property(e => e.PackageMethod)
                    .HasMaxLength(255)
                    .HasColumnName("packageMethod");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("price");

                entity.Property(e => e.Ratings)
                    .HasColumnType("decimal(2, 1)")
                    .HasColumnName("ratings");

                entity.Property(e => e.SellerId).HasColumnName("sellerId");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedAt");

                entity.Property(e => e.Weight)
                    .HasColumnType("decimal(6, 1)")
                    .HasColumnName("weight");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__Product__categor__693CA210");

                entity.HasOne(d => d.Material)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.MaterialId)
                    .HasConstraintName("FK__Product__materia__6A30C649");

                entity.HasOne(d => d.Seller)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.SellerId)
                    .HasConstraintName("FK__Product__sellerI__6B24EA82");
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImage");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("createdAt");

                entity.Property(e => e.ImageUrl).HasColumnName("imageUrl");

                entity.Property(e => e.ProductId).HasColumnName("productId");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedAt");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(d => d.ProductId)
                    .HasConstraintName("FK__ProductIm__produ__6C190EBB");
            });

            modelBuilder.Entity<Response>(entity =>
            {
                entity.HasKey(e => e.FeedbackId)
                    .HasName("PK__Response__2613FD24CFBA8B18");

                entity.ToTable("Response");

                entity.Property(e => e.FeedbackId)
                    .ValueGeneratedNever()
                    .HasColumnName("feedbackId");

                entity.Property(e => e.SellerId).HasColumnName("sellerId");

                entity.Property(e => e.SellerMessage).HasColumnName("sellerMessage");

                entity.Property(e => e.Timestamp)
                    .HasColumnType("datetime")
                    .HasColumnName("timestamp");

                entity.HasOne(d => d.Feedback)
                    .WithOne(p => p.Response)
                    .HasForeignKey<Response>(d => d.FeedbackId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Response__feedba__6D0D32F4");

                entity.HasOne(d => d.Seller)
                    .WithMany(p => p.Responses)
                    .HasForeignKey(d => d.SellerId)
                    .HasConstraintName("FK__Response__seller__6E01572D");
            });

            modelBuilder.Entity<Seller>(entity =>
            {
                entity.ToTable("Seller");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Phone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("phone")
                    .IsFixedLength();

                entity.Property(e => e.ProfilePicture)
                    .IsUnicode(false)
                    .HasColumnName("profilePicture");

                entity.Property(e => e.Ratings)
                    .HasColumnType("decimal(2, 1)")
                    .HasColumnName("ratings");

                entity.Property(e => e.RegisteredAt)
                    .HasColumnType("datetime")
                    .HasColumnName("registeredAt");

                entity.Property(e => e.ShopId)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("shopId");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.Seller)
                    .HasForeignKey<Seller>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Seller__id__6EF57B66");
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.ToTable("Token");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.ExpiryTime)
                    .HasColumnType("datetime")
                    .HasColumnName("expiryTime");

                entity.Property(e => e.RefreshToken)
                    .HasMaxLength(512)
                    .IsUnicode(false)
                    .HasColumnName("refreshToken");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.Token)
                    .HasForeignKey<Token>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Token__id__6FE99F9F");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Amount)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("amount");

                entity.Property(e => e.Date)
                    .HasColumnType("datetime")
                    .HasColumnName("date");

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");

                entity.Property(e => e.PaymentMethod).HasColumnName("paymentMethod");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.WalletId).HasColumnName("walletId");

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.WalletId)
                    .HasConstraintName("FK__Transacti__walle__70DDC3D8");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("createdAt");

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("dob");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.Gender).HasColumnName("gender");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(60)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.Phone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("phone")
                    .IsFixedLength();

                entity.Property(e => e.ProfilePicture)
                    .IsUnicode(false)
                    .HasColumnName("profilePicture");

                entity.Property(e => e.Role).HasColumnName("role");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("datetime")
                    .HasColumnName("updatedAt");
            });

            modelBuilder.Entity<VerifyToken>(entity =>
            {
                entity.ToTable("VerifyToken");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Code)
                    .HasMaxLength(512)
                    .IsUnicode(false)
                    .HasColumnName("code");

                entity.Property(e => e.ExpirationDate)
                    .HasColumnType("datetime")
                    .HasColumnName("expirationDate");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.VerifyToken)
                    .HasForeignKey<VerifyToken>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__VerifyToken__id__71D1E811");
            });

            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.ToTable("Wallet");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.AvailableBalance)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("availableBalance");

                entity.Property(e => e.LockedBalance)
                    .HasColumnType("decimal(12, 1)")
                    .HasColumnName("lockedBalance");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.HasOne(d => d.IdNavigation)
                    .WithOne(p => p.Wallet)
                    .HasForeignKey<Wallet>(d => d.Id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Wallet__id__72C60C4A");
            });

            modelBuilder.Entity<Withdrawal>(entity =>
            {
                entity.ToTable("Withdrawal");

                entity.Property(e => e.Amount).HasColumnType("decimal(12, 1)");

                entity.Property(e => e.Bank)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.BankNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Manager)
                    .WithMany(p => p.Withdrawals)
                    .HasForeignKey(d => d.ManagerId)
                    .HasConstraintName("FK__Withdrawa__Manag__73BA3083");

                entity.HasOne(d => d.Wallet)
                    .WithMany(p => p.Withdrawals)
                    .HasForeignKey(d => d.WalletId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Withdrawa__Walle__74AE54BC");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
