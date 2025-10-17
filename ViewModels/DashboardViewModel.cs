using EconomicWPF.Commands;
using EconomicWPF.Enums;
using EconomicWPF.Models;
using EconomicWPF.Repositories.Implementation;
using EconomicWPF.Repositories.Interfaces;
using EconomicWPF.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EconomicWPF.ViewModels
{
    /// <summary>
    /// ViewModel for Dashboard
    /// Viser oversigt over nøgletal og hurtig adgang til vigtige funktioner
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IProductRepository _productRepository;

        #region Properties

        private int _totalCustomers;
        public int TotalCustomers
        {
            get => _totalCustomers;
            set => SetProperty(ref _totalCustomers, value);
        }

        private int _totalInvoices;
        public int TotalInvoices
        {
            get => _totalInvoices;
            set => SetProperty(ref _totalInvoices, value);
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        private int _overdueInvoices;
        public int OverdueInvoices
        {
            get => _overdueInvoices;
            set => SetProperty(ref _overdueInvoices, value);
        }

        private ObservableCollection<Invoice> _recentInvoices;
        public ObservableCollection<Invoice> RecentInvoices
        {
            get => _recentInvoices;
            set => SetProperty(ref _recentInvoices, value);
        }

        private ObservableCollection<Customer> _recentCustomers;
        public ObservableCollection<Customer> RecentCustomers
        {
            get => _recentCustomers;
            set => SetProperty(ref _recentCustomers, value);
        }

        private string _currentDate;
        public string CurrentDate
        {
            get => _currentDate;
            set => SetProperty(ref _currentDate, value);
        }

        private string _greetingMessage;
        public string GreetingMessage
        {
            get => _greetingMessage;
            set => SetProperty(ref _greetingMessage, value);
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand CreateInvoiceCommand { get; }
        public ICommand CreateCustomerCommand { get; }
        public ICommand CreateProductCommand { get; }
        public ICommand ViewReportsCommand { get; }

        #endregion

        #region Constructor

        public DashboardViewModel(
            ICustomerRepository customerRepository,
            IInvoiceRepository invoiceRepository,
            IProductRepository productRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

            // Initialize collections
            RecentInvoices = new ObservableCollection<Invoice>();
            RecentCustomers = new ObservableCollection<Customer>();

            // Set current date
            CurrentDate = DateTime.Now.ToString("dddd, dd. MMMM yyyy",
                new System.Globalization.CultureInfo("da-DK"));

            // Set greeting
            SetGreeting();

            // Initialize commands
            RefreshCommand = new RelayCommand(async _ => await LoadDashboardDataAsync());
            CreateInvoiceCommand = new RelayCommand(_ => CreateInvoice());
            CreateCustomerCommand = new RelayCommand(_ => CreateCustomer());
            CreateProductCommand = new RelayCommand(_ => CreateProduct());
            ViewReportsCommand = new RelayCommand(_ => ViewReports());

            // Load data
            LoadDashboardDataAsync();
        }

        #endregion

        #region Methods

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                // Load statistics
                await LoadStatisticsAsync();

                // Load recent invoices
                await LoadRecentInvoicesAsync();

                // Load recent customers
                await LoadRecentCustomersAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                // Total customers
                TotalCustomers = await _customerRepository.CountAsync();

                // Total invoices
                TotalInvoices = await _invoiceRepository.CountAsync();

                // Total revenue (this month)
                var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var monthlyInvoices = await _invoiceRepository.GetByDateRangeAsync(firstDayOfMonth, lastDayOfMonth);
                TotalRevenue = monthlyInvoices
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Sent) // ✅ Bruger enum
                    .Sum(i => i.TotalAmount);

                // Overdue invoices
                var overdueList = await _invoiceRepository.GetOverdueInvoicesAsync();
                OverdueInvoices = overdueList.Count();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading statistics: {ex.Message}");
            }
        }

        private async Task LoadRecentInvoicesAsync()
        {
            try
            {
                var allInvoices = await _invoiceRepository.GetAllAsync();
                var recent = allInvoices
                    .OrderByDescending(i => i.CreatedDate)
                    .Take(10);

                RecentInvoices.Clear();
                foreach (var invoice in recent)
                {
                    RecentInvoices.Add(invoice);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent invoices: {ex.Message}");
            }
        }

        private async Task LoadRecentCustomersAsync()
        {
            try
            {
                var allCustomers = await _customerRepository.GetAllAsync();
                var recent = allCustomers
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(5);

                RecentCustomers.Clear();
                foreach (var customer in recent)
                {
                    RecentCustomers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent customers: {ex.Message}");
            }
        }

        private void SetGreeting()
        {
            var hour = DateTime.Now.Hour;

            if (hour < 12)
            {
                GreetingMessage = "God morgen! ☀️";
            }
            else if (hour < 18)
            {
                GreetingMessage = "God eftermiddag! 👋";
            }
            else
            {
                GreetingMessage = "God aften! 🌙";
            }
        }

        private void CreateInvoice()
        {
            // TODO: Navigation til Invoice page med ny faktura
            System.Diagnostics.Debug.WriteLine("Navigate to create invoice");
        }

        private void CreateCustomer()
        {
            // TODO: Navigation til Customer page med ny kunde
            System.Diagnostics.Debug.WriteLine("Navigate to create customer");
        }

        private void CreateProduct()
        {
            // TODO: Navigation til Product page med ny vare
            System.Diagnostics.Debug.WriteLine("Navigate to create product");
        }

        private void ViewReports()
        {
            // TODO: Navigation til Reports page
            System.Diagnostics.Debug.WriteLine("Navigate to reports");
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            RecentInvoices?.Clear();
            RecentCustomers?.Clear();
        }

        #endregion
    }
}