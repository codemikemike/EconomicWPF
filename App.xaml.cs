using EconomicWPF.Database;
using EconomicWPF.Repositories.Implementation;
using EconomicWPF.Repositories.Interfaces;
using EconomicWPF.ViewModels;
using EconomicWPF.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace EconomicWPF
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // ========================================
            // Database Context
            // ========================================
            services.AddDbContext<EconomicDbContext>(options =>
            {
                options.UseSqlServer(
                    "Server=(localdb)\\mssqllocaldb;Database=EconomicDB;Trusted_Connection=True;MultipleActiveResultSets=true");
            });

            // ========================================
            // Repositories - MVVM Data Access Layer (Entity Framework)
            // ========================================
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IBankTransactionRepository, BankTransactionRepository>();
            services.AddScoped<IBudgetRepository, BudgetRepository>();
            services.AddScoped<IDimensionRepository, DimensionRepository>();
            services.AddScoped<IVATCodeRepository, VATCodeRepository>();

            // ========================================
            // ViewModels
            // ========================================
            services.AddTransient<MainViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<AccountViewModel>();
            services.AddTransient<CustomerViewModel>();
            services.AddTransient<SupplierViewModel>();
            services.AddTransient<ProductViewModel>();
            services.AddTransient<InvoiceViewModel>();
            services.AddTransient<ProjectViewModel>();
            services.AddTransient<TimeEntryViewModel>();
            services.AddTransient<TransactionViewModel>();
            services.AddTransient<PaymentViewModel>();
            services.AddTransient<BankTransactionViewModel>();
            services.AddTransient<BudgetViewModel>();
            services.AddTransient<DimensionViewModel>();
            services.AddTransient<VATCodeViewModel>();

            // ========================================
            // Views
            // ========================================
            services.AddTransient<MainWindow>();
            services.AddTransient<DashboardView>();
            services.AddTransient<AccountView>();
            services.AddTransient<CustomerView>();
            services.AddTransient<SupplierView>();
            services.AddTransient<ProductView>();
            services.AddTransient<InvoiceView>();
            services.AddTransient<ProjectView>();
            services.AddTransient<TimeEntryView>();
            services.AddTransient<TransactionView>();
            services.AddTransient<PaymentView>();
            services.AddTransient<BankTransactionView>();
            services.AddTransient<BudgetView>();
            services.AddTransient<DimensionView>();
            services.AddTransient<VATCodeView>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Initialiser database
                InitializeDatabase();

                // Vis MainWindow
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Fejl ved opstart af applikationen:\n\n{ex.Message}\n\nSe Output vinduet for flere detaljer.",
                    "Opstartsfejl",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                System.Diagnostics.Debug.WriteLine($"Startup error: {ex}");
                Shutdown();
            }
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<EconomicDbContext>();

                    // Tjek om database kan forbindes
                    var canConnect = dbContext.Database.CanConnect();

                    if (!canConnect)
                    {
                        System.Diagnostics.Debug.WriteLine("Opretter database...");
                        dbContext.Database.EnsureCreated();
                        System.Diagnostics.Debug.WriteLine("Database oprettet succesfuldt!");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Database forbundet!");
                    }

                    // Test database forbindelse
                    var customerCount = dbContext.Customers.Count();
                    System.Diagnostics.Debug.WriteLine($"Database har {customerCount} kunder");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database fejl: {ex}");
                throw new Exception($"Kunne ikke oprette forbindelse til databasen. Sørg for at SQL Server LocalDB er installeret.\n\nFejl: {ex.Message}", ex);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}