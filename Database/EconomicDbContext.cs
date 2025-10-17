using EconomicWPF.Models;
using Microsoft.EntityFrameworkCore;

namespace EconomicWPF.Database
{
    public class EconomicDbContext : DbContext
    {
        public EconomicDbContext(DbContextOptions<EconomicDbContext> options)
            : base(options)
        {
        }

        // DbSets - tabeller i databasen
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Dimension> Dimensions { get; set; }
        public DbSet<VATCode> VATCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer konfiguration
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CustomerNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.CVR).HasMaxLength(20);
                entity.Property(e => e.CreditLimit).HasColumnType("decimal(18,2)");
            });

            // Account konfiguration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.AccountId);
                entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AccountName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            });

            // Product konfiguration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SalesPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VATRate).HasColumnType("decimal(5,2)");
            });

            // Invoice konfiguration
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceId);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VATAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

                // Relation til Customer
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // BankTransaction konfiguration
            modelBuilder.Entity<BankTransaction>(entity =>
            {
                entity.HasKey(e => e.BankTransactionId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Reference).HasMaxLength(100);
            });

            // Seed data - test data til at komme i gang
            SeedInitialData(modelBuilder);
        }

        private void SeedInitialData(ModelBuilder modelBuilder)
        {
            // Seed nogle test kunder
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    CustomerId = 1,
                    CustomerNumber = "C001",
                    Name = "Acme Corporation",
                    Email = "info@acme.dk",
                    Phone = "12345678",
                    CVR = "12345678",
                    Address = "Hovedgaden 1",
                    City = "København",
                    ZipCode = "2100",
                    Country = "Danmark",
                    CreditLimit = 50000,
                    PaymentTermDays = 30,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Customer
                {
                    CustomerId = 2,
                    CustomerNumber = "C002",
                    Name = "TechStart ApS",
                    Email = "kontakt@techstart.dk",
                    Phone = "87654321",
                    CVR = "87654321",
                    Address = "Innovationsvej 10",
                    City = "Aarhus",
                    ZipCode = "8000",
                    Country = "Danmark",
                    CreditLimit = 75000,
                    PaymentTermDays = 14,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                }
            );

            // Seed nogle test produkter
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    ProductId = 1,
                    ProductNumber = "P001",
                    Name = "Konsulenttime",
                    Description = "Timebaseret konsulentydelse",
                    PurchasePrice = 0,
                    SalesPrice = 1200,
                    VATRate = 25,
                    Unit = "Timer",
                    StockQuantity = 0,
                    ReorderLevel = 0,
                    AccountId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                },
                new Product
                {
                    ProductId = 2,
                    ProductNumber = "P002",
                    Name = "Softwarelicens",
                    Description = "Årlig softwarelicens",
                    PurchasePrice = 500,
                    SalesPrice = 1500,
                    VATRate = 25,
                    Unit = "Stk",
                    StockQuantity = 100,
                    ReorderLevel = 20,
                    AccountId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                }
            );

            // Seed en test konto
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    AccountId = 1,
                    AccountNumber = "1000",
                    AccountName = "Salg af varer og ydelser",
                    AccountType = Enums.AccountType.Revenue,
                    Balance = 0,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );
        }
    }
}