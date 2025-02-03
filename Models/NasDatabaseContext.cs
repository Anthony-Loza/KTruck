using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using KTruckGui.Models;



namespace KTruckGui.Models
{
    public partial class NasDatabaseContext : DbContext
    {
   

        public NasDatabaseContext(DbContextOptions<NasDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<DriverLocationDatum> DriverLocationData { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }
        public virtual DbSet<Part> Parts { get; set; }
        public virtual DbSet<ServiceHistory> ServiceHistories { get; set; }
        public virtual DbSet<Vehicle> Vehicles { get; set; }
        public virtual DbSet<Workorder> Workorders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory()) // Correct base path
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(8).HasColumnName("ID");
                entity.Property(e => e.FName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Number).IsRequired().HasMaxLength(15);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.Type).HasMaxLength(20);
            });

            modelBuilder.Entity<DriverLocationDatum>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK_DriverLocation");
                entity.Property(e => e.UserId).ValueGeneratedNever().HasColumnName("UserID");
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.FirstName).HasMaxLength(255);
                entity.Property(e => e.LastName).HasMaxLength(255);
                entity.Property(e => e.LocatedAt).HasColumnType("datetime");
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.Role).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(36).HasColumnName("ID");
                entity.Property(e => e.BillTo).IsRequired().HasMaxLength(8);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.HasOne(d => d.BillToNavigation)
                    .WithMany(p => p.Invoices)
                    .HasForeignKey(d => d.BillTo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Invoices_BillTo");
            });

            modelBuilder.Entity<InvoiceItem>(entity =>
            {
                entity.HasKey(e => e.ItemId).HasName("PK_InvoiceItems");
                entity.Property(e => e.ItemId).HasMaxLength(50).HasColumnName("ItemID");
                entity.Property(e => e.InvoiceId).HasMaxLength(36).HasColumnName("InvoiceID");
                entity.Property(e => e.Type).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Rate).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Total).HasColumnType("numeric(27, 6)")
                    .HasComputedColumnSql("CASE WHEN [Type]='part' THEN ([Quantity]*[Rate])*(1.0825) ELSE [Quantity]*[Rate] END", true);
            });

            modelBuilder.Entity<Part>(entity =>
            {
                entity.Property(e => e.ID).ValueGeneratedNever().HasColumnName("ID");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Quantity).HasDefaultValueSql("((0))");
            });

            modelBuilder.Entity<ServiceHistory>(entity =>
            {
                entity.ToTable("ServiceHistory");
                entity.Property(e => e.Id).HasMaxLength(50).HasColumnName("ID");
                entity.Property(e => e.ServiceDate).HasColumnType("date");
                entity.Property(e => e.NextServiceDate).HasColumnType("date");
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("Vehicle");
                entity.Property(e => e.Id).HasMaxLength(4).HasColumnName("ID")
                    .HasComputedColumnSql("(RIGHT([VIN], 4))", true);
                entity.Property(e => e.Vin).IsRequired().HasMaxLength(17).HasColumnName("VIN");
                entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Make).HasMaxLength(50);
                entity.Property(e => e.Model).HasMaxLength(50);
                entity.Property(e => e.Odometer).HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<Workorder>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
                entity.Property(e => e.VehicleId).HasColumnName("VehicleID");
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.Status).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
