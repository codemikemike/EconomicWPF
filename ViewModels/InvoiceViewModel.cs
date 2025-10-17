using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EconomicWPF.Commands;
using EconomicWPF.Models;
using EconomicWPF.Enums;
using EconomicWPF.Repositories.Interfaces;
using EconomicWPF.ViewModels.Base;

namespace EconomicWPF.ViewModels
{
    /// <summary>
    /// ViewModel for Invoice management
    /// Håndterer faktura oprettelse, redigering og visning
    /// </summary>
    public class InvoiceViewModel : ViewModelBase
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;

        #region Properties

        private ObservableCollection<Invoice> _invoices;
        public ObservableCollection<Invoice> Invoices
        {
            get => _invoices;
            set => SetProperty(ref _invoices, value);
        }

        private ObservableCollection<Customer> _customers;
        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        private Invoice _selectedInvoice;
        public Invoice SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                if (SetProperty(ref _selectedInvoice, value))
                {
                    IsEditMode = value != null;
                    LoadInvoiceLinesAsync();
                }
            }
        }

        private Invoice _currentInvoice;
        public Invoice CurrentInvoice
        {
            get => _currentInvoice;
            set => SetProperty(ref _currentInvoice, value);
        }

        private ObservableCollection<InvoiceLine> _invoiceLines;
        public ObservableCollection<InvoiceLine> InvoiceLines
        {
            get => _invoiceLines;
            set => SetProperty(ref _invoiceLines, value);
        }

        private InvoiceLine _selectedLine;
        public InvoiceLine SelectedLine
        {
            get => _selectedLine;
            set => SetProperty(ref _selectedLine, value);
        }

        private InvoiceStatus? _statusFilter;
        public InvoiceStatus? StatusFilter
        {
            get => _statusFilter;
            set
            {
                if (SetProperty(ref _statusFilter, value))
                {
                    FilterInvoicesAsync();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value) && CurrentInvoice != null)
                {
                    CurrentInvoice.CustomerId = value?.CustomerId ?? 0;
                    CurrentInvoice.Customer = value;
                }
            }
        }

        #endregion

        #region Commands

        public ICommand LoadInvoicesCommand { get; }
        public ICommand CreateInvoiceCommand { get; }
        public ICommand EditInvoiceCommand { get; }
        public ICommand SaveInvoiceCommand { get; }
        public ICommand DeleteInvoiceCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddLineCommand { get; }
        public ICommand RemoveLineCommand { get; }
        public ICommand SendInvoiceCommand { get; }
        public ICommand MarkAsPaidCommand { get; }

        #endregion

        #region Constructor

        public InvoiceViewModel(IInvoiceRepository invoiceRepository,
                               ICustomerRepository customerRepository,
                               IProductRepository productRepository)
        {
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

            // Initialize collections
            Invoices = new ObservableCollection<Invoice>();
            Customers = new ObservableCollection<Customer>();
            Products = new ObservableCollection<Product>();
            InvoiceLines = new ObservableCollection<InvoiceLine>();

            // Initialize commands
            LoadInvoicesCommand = new RelayCommand(async _ => await LoadInvoicesAsync());
            CreateInvoiceCommand = new RelayCommand(_ => CreateNewInvoice(), _ => !IsEditMode);
            EditInvoiceCommand = new RelayCommand(_ => EditInvoice(), _ => SelectedInvoice != null);
            SaveInvoiceCommand = new RelayCommand(async _ => await SaveInvoiceAsync(), _ => CanSaveInvoice());
            DeleteInvoiceCommand = new RelayCommand(async _ => await DeleteInvoiceAsync(), _ => SelectedInvoice != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            AddLineCommand = new RelayCommand(_ => AddInvoiceLine(), _ => IsEditMode);
            RemoveLineCommand = new RelayCommand(_ => RemoveInvoiceLine(), _ => SelectedLine != null);
            SendInvoiceCommand = new RelayCommand(async _ => await SendInvoiceAsync(), _ => SelectedInvoice != null);
            MarkAsPaidCommand = new RelayCommand(async _ => await MarkAsPaidAsync(), _ => SelectedInvoice != null && !SelectedInvoice.IsPaid);

            // Load initial data
            InitializeAsync();
        }

        #endregion

        #region Methods

        private async Task InitializeAsync()
        {
            await LoadCustomersAsync();
            await LoadProductsAsync();
            await LoadInvoicesAsync();
        }

        private async Task LoadInvoicesAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var invoices = await _invoiceRepository.GetAllAsync();
                Invoices.Clear();

                foreach (var invoice in invoices)
                {
                    Invoices.Add(invoice);
                }
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

        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await _customerRepository.GetActiveCustomersAsync();
                Customers.Clear();

                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetActiveProductsAsync();
                Products.Clear();

                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async Task LoadInvoiceLinesAsync()
        {
            if (SelectedInvoice == null)
            {
                InvoiceLines.Clear();
                return;
            }

            try
            {
                var invoice = await _invoiceRepository.GetWithLinesAsync(SelectedInvoice.InvoiceId);

                InvoiceLines.Clear();
                if (invoice?.InvoiceLines != null)
                {
                    foreach (var line in invoice.InvoiceLines)
                    {
                        InvoiceLines.Add(line);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async void CreateNewInvoice()
        {
            try
            {
                var invoiceNumber = await _invoiceRepository.GenerateNextInvoiceNumberAsync();

                CurrentInvoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(14),
                    Currency = Currency.DKK, // ✅ Bruger enum
                    Status = InvoiceStatus.Draft, // ✅ Bruger enum
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                InvoiceLines.Clear();
                IsEditMode = true;
                SelectedInvoice = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void EditInvoice()
        {
            if (SelectedInvoice != null)
            {
                // ✅ Manuel mapping i stedet for copy constructor
                CurrentInvoice = new Invoice
                {
                    InvoiceId = SelectedInvoice.InvoiceId,
                    InvoiceNumber = SelectedInvoice.InvoiceNumber,
                    CustomerId = SelectedInvoice.CustomerId,
                    InvoiceDate = SelectedInvoice.InvoiceDate,
                    DueDate = SelectedInvoice.DueDate,
                    SubTotal = SelectedInvoice.SubTotal,
                    VATAmount = SelectedInvoice.VATAmount,
                    TotalAmount = SelectedInvoice.TotalAmount,
                    Currency = SelectedInvoice.Currency,
                    Status = SelectedInvoice.Status,
                    Notes = SelectedInvoice.Notes,
                    IsPaid = SelectedInvoice.IsPaid,
                    PaidDate = SelectedInvoice.PaidDate,
                    CreatedDate = SelectedInvoice.CreatedDate,
                    ModifiedDate = SelectedInvoice.ModifiedDate
                };

                SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == CurrentInvoice.CustomerId);
                IsEditMode = true;
            }
        }

        private async Task SaveInvoiceAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateInvoice())
                {
                    return;
                }

                // Beregn totaler
                CalculateTotals();

                // Opdater ModifiedDate
                CurrentInvoice.ModifiedDate = DateTime.Now;

                // Tilføj linjer til invoice
                CurrentInvoice.InvoiceLines = InvoiceLines.ToList();

                if (CurrentInvoice.InvoiceId == 0)
                {
                    // Ny faktura
                    CurrentInvoice.CreatedDate = DateTime.Now;
                    var id = await _invoiceRepository.AddAsync(CurrentInvoice);
                    CurrentInvoice.InvoiceId = id;
                    Invoices.Insert(0, CurrentInvoice);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _invoiceRepository.UpdateAsync(CurrentInvoice);
                    if (success)
                    {
                        var existing = Invoices.FirstOrDefault(i => i.InvoiceId == CurrentInvoice.InvoiceId);
                        if (existing != null)
                        {
                            var index = Invoices.IndexOf(existing);
                            Invoices[index] = CurrentInvoice;
                        }
                    }
                }

                IsEditMode = false;
                CurrentInvoice = null;
                InvoiceLines.Clear();
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

        private async Task DeleteInvoiceAsync()
        {
            if (SelectedInvoice == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Tilføj confirmation dialog

                var success = await _invoiceRepository.DeleteAsync(SelectedInvoice.InvoiceId);
                if (success)
                {
                    Invoices.Remove(SelectedInvoice);
                    SelectedInvoice = null;
                }
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

        private void CancelEdit()
        {
            IsEditMode = false;
            CurrentInvoice = null;
            InvoiceLines.Clear();
            SelectedInvoice = null;
            ClearError();
        }

        private void AddInvoiceLine()
        {
            var line = new InvoiceLine
            {
                InvoiceId = CurrentInvoice?.InvoiceId ?? 0,
                Quantity = 1,
                VATRate = 25.00m,
                CreatedDate = DateTime.Now
            };

            InvoiceLines.Add(line);
        }

        private void RemoveInvoiceLine()
        {
            if (SelectedLine != null)
            {
                InvoiceLines.Remove(SelectedLine);
                CalculateTotals();
            }
        }

        private async Task SendInvoiceAsync()
        {
            if (SelectedInvoice == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // Opdater status til "Sent"
                SelectedInvoice.Status = InvoiceStatus.Sent; // ✅ Bruger enum
                SelectedInvoice.ModifiedDate = DateTime.Now;
                await _invoiceRepository.UpdateAsync(SelectedInvoice);

                // TODO: Send email til kunde

                await LoadInvoicesAsync();
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

        private async Task MarkAsPaidAsync()
        {
            if (SelectedInvoice == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                SelectedInvoice.IsPaid = true;
                SelectedInvoice.PaidDate = DateTime.Today;
                SelectedInvoice.Status = InvoiceStatus.Paid; // ✅ Bruger enum
                SelectedInvoice.ModifiedDate = DateTime.Now;

                await _invoiceRepository.UpdateAsync(SelectedInvoice);
                await LoadInvoicesAsync();
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

        private async Task FilterInvoicesAsync()
        {
            try
            {
                IsLoading = true;

                IEnumerable<Invoice> invoices;

                if (StatusFilter == null)
                {
                    invoices = await _invoiceRepository.GetAllAsync();
                }
                else
                {
                    invoices = await _invoiceRepository.GetByStatusAsync(StatusFilter.Value); // ✅ Bruger enum
                }

                Invoices.Clear();
                foreach (var invoice in invoices)
                {
                    Invoices.Add(invoice);
                }
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

        private void CalculateTotals()
        {
            if (CurrentInvoice == null)
                return;

            decimal subtotal = 0;
            decimal vatAmount = 0;

            foreach (var line in InvoiceLines)
            {
                // ✅ Beregn LineTotal direkte i ViewModel
                line.LineTotal = line.Quantity * line.UnitPrice;

                subtotal += line.Quantity * line.UnitPrice;
                vatAmount += (line.Quantity * line.UnitPrice) * (line.VATRate / 100);
            }

            CurrentInvoice.SubTotal = subtotal;
            CurrentInvoice.VATAmount = vatAmount;
            CurrentInvoice.TotalAmount = subtotal + vatAmount;
        }

        private bool ValidateInvoice()
        {
            if (CurrentInvoice == null)
            {
                SetError("Ingen faktura valgt");
                return false;
            }

            if (CurrentInvoice.CustomerId == 0)
            {
                SetError("Vælg en kunde");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentInvoice.InvoiceNumber))
            {
                SetError("Fakturanummer er påkrævet");
                return false;
            }

            if (CurrentInvoice.DueDate < CurrentInvoice.InvoiceDate)
            {
                SetError("Forfaldsdato skal være efter fakturadato");
                return false;
            }

            if (!InvoiceLines.Any())
            {
                SetError("Tilføj mindst én fakturalinje");
                return false;
            }

            return true;
        }

        private bool CanSaveInvoice()
        {
            return CurrentInvoice != null &&
                   CurrentInvoice.CustomerId > 0 &&
                   !string.IsNullOrWhiteSpace(CurrentInvoice.InvoiceNumber) &&
                   InvoiceLines.Any();
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            Invoices?.Clear();
            Customers?.Clear();
            Products?.Clear();
            InvoiceLines?.Clear();
        }

        #endregion
    }
}