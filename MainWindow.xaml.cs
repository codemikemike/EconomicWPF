using System;
using System.Windows;
using System.Windows.Controls;
using EconomicWPF.ViewModels;
using EconomicWPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace EconomicWPF
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private Button _activeButton;

        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;

            DataContext = new MainViewModel();

            // Navigate to Dashboard by default
            NavigateToDashboard(null, null);
        }

        private void NavigateTo<TView>() where TView : UserControl
        {
            var view = _serviceProvider.GetRequiredService<TView>();
            ContentArea.Content = view;
        }

        private void SetActiveButton(Button button)
        {
            // Reset previous active button
            if (_activeButton != null)
            {
                _activeButton.Style = (Style)FindResource("NavButtonStyle");
            }

            // Set new active button
            _activeButton = button;
            if (_activeButton != null)
            {
                _activeButton.Style = (Style)FindResource("ActiveNavButtonStyle");
            }
        }

        // Navigation Methods
        private void NavigateToDashboard(object sender, RoutedEventArgs e)
        {
            NavigateTo<DashboardView>();
            SetActiveButton(btnDashboard);
        }

        private void NavigateToCustomers(object sender, RoutedEventArgs e)
        {
            NavigateTo<CustomerView>();
            SetActiveButton(btnCustomers);
        }

        private void NavigateToSuppliers(object sender, RoutedEventArgs e)
        {
            NavigateTo<SupplierView>();
            SetActiveButton(btnSuppliers);
        }

        private void NavigateToProducts(object sender, RoutedEventArgs e)
        {
            NavigateTo<ProductView>();
            SetActiveButton(btnProducts);
        }

        private void NavigateToInvoices(object sender, RoutedEventArgs e)
        {
            NavigateTo<InvoiceView>();
            SetActiveButton(btnInvoices);
        }

        private void NavigateToPayments(object sender, RoutedEventArgs e)
        {
            NavigateTo<PaymentView>();
            SetActiveButton(btnPayments);
        }

        private void NavigateToProjects(object sender, RoutedEventArgs e)
        {
            NavigateTo<ProjectView>();
            SetActiveButton(btnProjects);
        }

        private void NavigateToTimeEntries(object sender, RoutedEventArgs e)
        {
            NavigateTo<TimeEntryView>();
            SetActiveButton(btnTimeEntries);
        }

        private void NavigateToAccounts(object sender, RoutedEventArgs e)
        {
            NavigateTo<AccountView>();
            SetActiveButton(btnAccounts);
        }

        private void NavigateToTransactions(object sender, RoutedEventArgs e)
        {
            NavigateTo<TransactionView>();
            SetActiveButton(btnTransactions);
        }

        private void NavigateToBankTransactions(object sender, RoutedEventArgs e)
        {
            NavigateTo<BankTransactionView>();
            SetActiveButton(btnBankTransactions);
        }

        private void NavigateToBudgets(object sender, RoutedEventArgs e)
        {
            NavigateTo<BudgetView>();
            SetActiveButton(btnBudgets);
        }

        private void NavigateToDimensions(object sender, RoutedEventArgs e)
        {
            NavigateTo<DimensionView>();
            SetActiveButton(btnDimensions);
        }

        private void NavigateToVATCodes(object sender, RoutedEventArgs e)
        {
            NavigateTo<VATCodeView>();
            SetActiveButton(btnVATCodes);
        }
    }
}