using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TrainTray_food_order_booking_system.Models;

namespace TrainTray_food_order_booking_system.Models;

public partial class TblUserContext : DbContext
{
    public TblUserContext()
    {
    }

    public TblUserContext(DbContextOptions<TblUserContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FailedLoginAttempt> FailedLoginAttempts { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderOtp> OrderOtps { get; set; }

    public virtual DbSet<Restaurant> Restaurant { get; set; }

    public virtual DbSet<TblCart> TblCart { get; set; }

    public virtual DbSet<TblCategory> TblCategories { get; set; }

    public virtual DbSet<TblItem> TblItems { get; set; }

    public virtual DbSet<TblState> TblStates { get; set; }

    public virtual DbSet<TblStation> TblStations { get; set; }

    public virtual DbSet<TblTrain> TblTrains { get; set; }

    public virtual DbSet<TempUser> TempUsers { get; set; }

    public virtual DbSet<User> User { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-IEOT67U\\SQLEXPRESS;Database=tblUser;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF07020F21");

            entity.ToTable(tb => tb.HasTrigger("trg_UpdateTotal"));

            entity.HasIndex(e => e.ArrivalTime, "IDX_Orders_Arrival");

            entity.HasIndex(e => e.RestaurantId, "IDX_Orders_Restaurant");

            entity.HasIndex(e => e.StationId, "IDX_Orders_Station");

            entity.HasIndex(e => e.TrainNo, "IDX_Orders_Train");

            entity.HasIndex(e => e.UserId, "IDX_Orders_User");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ArrivalTime)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CoachNo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.RestaurantId).HasColumnName("RestaurantID");
            entity.Property(e => e.SeatNo)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.StationId).HasColumnName("StationID");
            entity.Property(e => e.Total).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TrainName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Item).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Item");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.Orders)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Restaurant");

            entity.HasOne(d => d.Station).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Station");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_User");
        });

        modelBuilder.Entity<OrderOtp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderOtp__3214EC0703317E3D");

            entity.Property(e => e.OtpCode).HasMaxLength(10);
            entity.Property(e => e.OtpGeneratedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderOtps)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderOtps_Orders");
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.RestaurantId).HasName("PK__Restaura__87454CB50AD2A005");

            entity.ToTable("Restaurant");

            entity.HasIndex(e => e.Gst, "UQ__Restaura__C51F7EFC108B795B").IsUnique();

            entity.HasIndex(e => e.Pan, "UQ__Restaura__C57098050DAF0CB0").IsUnique();

            entity.Property(e => e.RestaurantId).HasColumnName("RestaurantID");
            entity.Property(e => e.Address)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Gst)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("GST");
            entity.Property(e => e.Latitude).HasColumnType("decimal(18, 15)");
            entity.Property(e => e.Logo)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Longitude).HasColumnType("decimal(18, 15)");
            entity.Property(e => e.Pan)
                .HasMaxLength(12)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("RName");
            entity.Property(e => e.StationId).HasColumnName("StationID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Station).WithMany(p => p.Restaurants)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Restauran__Stati__32E0915F");

            entity.HasOne(d => d.User).WithMany(p => p.Restaurants)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Restauran__UserI__31EC6D26");
        });

        modelBuilder.Entity<TblCart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__TblCart__51BCD79748762199");

            entity.ToTable("TblCart");

            entity.Property(e => e.CartId).HasColumnName("CartID");
            entity.Property(e => e.ItemId).HasColumnName("ItemID");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Item).WithMany(p => p.TblCart)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCart_Item");

            entity.HasOne(d => d.User).WithMany(p => p.TblCart)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TblCart_User");
        });

        modelBuilder.Entity<TblCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__TblCateg__19093A0B145C0A3F");

            entity.ToTable("TblCategory");

            entity.Property(e => e.CategoryId).ValueGeneratedNever();
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__TblItem__727E838B182C9B23");

            entity.ToTable("TblItem");

            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ItemImage)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ItemStatus).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.TblItems)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_TblItem_Category");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.TblItems)
                .HasForeignKey(d => d.RestaurantId)
                .HasConstraintName("FK_TblItem_Restaurant");
        });

        modelBuilder.Entity<TblState>(entity =>
        {
            entity.HasKey(e => e.StateId).HasName("PK__TblState__C3BA3B5A1BFD2C07");

            entity.ToTable("TblState");

            entity.Property(e => e.StateId)
                .ValueGeneratedNever()
                .HasColumnName("StateID");
            entity.Property(e => e.Name)
                .HasMaxLength(40)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblStation>(entity =>
        {
            entity.HasKey(e => e.StationId).HasName("PK__TblStati__E0D8A6DD1FCDBCEB");

            entity.ToTable("TblStation");

            entity.Property(e => e.StationId)
                .ValueGeneratedNever()
                .HasColumnName("StationID");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.StateId).HasColumnName("StateID");

            entity.HasOne(d => d.State).WithMany(p => p.TblStations)
                .HasForeignKey(d => d.StateId)
                .HasConstraintName("FK__TblStatio__State__35BCFE0A");
        });

        modelBuilder.Entity<TblTrain>(entity =>
        {
            entity.HasKey(e => e.TrainNo).HasName("PK__TblTrain__8ED1D8CD239E4DCF");

            entity.Property(e => e.TrainNo).ValueGeneratedNever();
            entity.Property(e => e.TrainName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TempUser>(entity =>
        {
            entity.Property(e => e.Otp).HasColumnName("OTP");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Mobile).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.Password).HasMaxLength(60);
            entity.Property(e => e.Type).HasColumnName("type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
