using System;
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
    /// ViewModel for Payment management
    /// Håndterer betalinger på fakturaer
    /// </summary>
    public class PaymentViewModel : ViewModelBase
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        #region Properties

        private ObservableCollection<Payment> _payments;
        public ObservableCollection<Payment> Payments
        {
            get => _payments;
            set => SetProperty(ref _payments, value);
        }

        private ObservableCollection<Invoice> _unpaidInvoices;
        public ObservableCollection<Invoice> UnpaidInvoices
        {
            get => _unpaidInvoices;
            set => SetProperty(ref _unpaidInvoices, value);
        }

        private Payment _selectedPayment;
        public Payment SelectedPayment
        {
            get => _selectedPayment;
            set
            {
                if (SetProperty(ref _selectedPayment, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsPaymentSelected));
                }
            }
        }

        private Payment _currentPayment;
        public Payment CurrentPayment
        {
            get => _currentPayment;
            set => SetProperty(ref _currentPayment, value);
        }

        private Invoice _selectedInvoice;
        public Invoice SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                if (SetProperty(ref _selectedInvoice, value) && CurrentPayment != null)
                {
                    CurrentPayment.InvoiceId = value?.InvoiceId ?? 0;
                    CurrentPayment.Invoice = value;

                    // Set amount to remaining amount on invoice
                    if (value != null)
                    {
                        CurrentPayment.Amount = value.TotalAmount;
                    }
                }
            }
        }

        private PaymentMethod? _paymentMethodFilter;
        public PaymentMethod? PaymentMethodFilter
        {
            get => _paymentMethodFilter;
            set
            {
                if (SetProperty(ref _paymentMethodFilter, value))
                {
                    FilterByPaymentMethod();
                }
            }
        }

        private DateTime? _dateFrom;
        public DateTime? DateFrom
        {
            get => _dateFrom;
            set
            {
                if (SetProperty(ref _dateFrom, value))
                {
                    FilterByDateRange();
                }
            }
        }

        private DateTime? _dateTo;
        public DateTime? DateTo
        {
            get => _dateTo;
            set
            {
                if (SetProperty(ref _dateTo, value))
                {
                    FilterByDateRange();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsPaymentSelected => SelectedPayment != null;

        private decimal _totalPayments;
        public decimal TotalPayments
        {
            get => _totalPayments;
            set => SetProperty(ref _totalPayments, value);
        }

        #endregion

        #region Commands

        public ICommand LoadPaymentsCommand { get; }
        public ICommand AddPaymentCommand { get; }
        public ICommand EditPaymentCommand { get; }
        public ICommand SavePaymentCommand { get; }
        public ICommand DeletePaymentCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand NewPaymentCommand { get; }
        public ICommand RegisterPaymentCommand { get; }
        public ICommand ViewPaymentHistoryCommand { get; }

        #endregion

        #region Constructor

        public PaymentViewModel(
            IPaymentRepository paymentRepository,
            IInvoiceRepository invoiceRepository)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));

            // Initialize collections
            Payments = new ObservableCollection<Payment>();
            UnpaidInvoices = new ObservableCollection<Invoice>();

            // Initialize commands
            LoadPaymentsCommand = new RelayCommand(async _ => await LoadPaymentsAsync());
            AddPaymentCommand = new RelayCommand(_ => CreateNewPayment(), _ => !IsEditMode);
            EditPaymentCommand = new RelayCommand(_ => EditPayment(), _ => SelectedPayment != null);
            SavePaymentCommand = new RelayCommand(async _ => await SavePaymentAsync(), _ => CanSavePayment());
            DeletePaymentCommand = new RelayCommand(async _ => await DeletePaymentAsync(), _ => SelectedPayment != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            NewPaymentCommand = new RelayCommand(_ => CreateNewPayment());
            RegisterPaymentCommand = new RelayCommand(async _ => await RegisterQuickPaymentAsync());
            ViewPaymentHistoryCommand = new RelayCommand(async _ => await ViewPaymentHistoryAsync(), _ => SelectedInvoice != null);

            // Load initial data
            InitializeAsync();
        }

        #endregion

        #region Methods

        private async Task InitializeAsync()
        {
            await LoadUnpaidInvoicesAsync();
            await LoadPaymentsAsync();
        }

        private async Task LoadPaymentsAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var payments = await _paymentRepository.GetAllAsync();
                Payments.Clear();

                foreach (var payment in payments)
                {
                    Payments.Add(payment);
                }

                CalculateTotals();
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

        private async Task LoadUnpaidInvoicesAsync()
        {
            try
            {
                var allInvoices = await _invoiceRepository.GetAllAsync();
                var unpaid = allInvoices.Where(i => !i.IsPaid);

                UnpaidInvoices.Clear();
                foreach (var invoice in unpaid)
                {
                    UnpaidInvoices.Add(invoice);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void CreateNewPayment()
        {
            CurrentPayment = new Payment
            {
                PaymentDate = DateTime.Today,
                PaymentMethod = PaymentMethod.BankTransfer,
                CreatedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedPayment = null;
        }

        private void EditPayment()
        {
            if (SelectedPayment != null)
            {
                CurrentPayment = new Payment
                {
                    PaymentId = SelectedPayment.PaymentId,
                    InvoiceId = SelectedPayment.InvoiceId,
                    PaymentDate = SelectedPayment.PaymentDate,
                    Amount = SelectedPayment.Amount,
                    PaymentMethod = SelectedPayment.PaymentMethod,
                    Reference = SelectedPayment.Reference,
                    CreatedDate = SelectedPayment.CreatedDate
                };

                // Set selected invoice
                SelectedInvoice = UnpaidInvoices.FirstOrDefault(i => i.InvoiceId == CurrentPayment.InvoiceId);

                IsEditMode = true;
            }
        }

        private async Task SavePaymentAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidatePayment())
                {
                    return;
                }

                if (CurrentPayment.PaymentId == 0)
                {
                    // Ny betaling
                    CurrentPayment.CreatedDate = DateTime.Now;
                    var id = await _paymentRepository.AddAsync(CurrentPayment);
                    CurrentPayment.PaymentId = id;
                    Payments.Add(CurrentPayment);

                    // Update invoice paid status
                    await UpdateInvoicePaidStatus(CurrentPayment.InvoiceId);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _paymentRepository.UpdateAsync(CurrentPayment);
                    if (success)
                    {
                        var existing = Payments.FirstOrDefault(p => p.PaymentId == CurrentPayment.PaymentId);
                        if (existing != null)
                        {
                            var index = Payments.IndexOf(existing);
                            Payments[index] = CurrentPayment;
                        }
                    }
                }

                IsEditMode = false;
                CurrentPayment = null;
                SelectedPayment = null;
                SelectedInvoice = null;

                // Reload unpaid invoices
                await LoadUnpaidInvoicesAsync();
                CalculateTotals();
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

        private async Task DeletePaymentAsync()
        {
            if (SelectedPayment == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Tilføj confirmation dialog

                var invoiceId = SelectedPayment.InvoiceId;
                var success = await _paymentRepository.DeleteAsync(SelectedPayment.PaymentId);
                if (success)
                {
                    Payments.Remove(SelectedPayment);
                    SelectedPayment = null;

                    // Update invoice paid status
                    await UpdateInvoicePaidStatus(invoiceId);
                    await LoadUnpaidInvoicesAsync();
                    CalculateTotals();
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
            CurrentPayment = null;
            SelectedPayment = null;
            SelectedInvoice = null;
            ClearError();
        }

        private async Task RegisterQuickPaymentAsync()
        {
            // TODO: Implementer hurtig betalingsregistrering dialog
            System.Diagnostics.Debug.WriteLine("Register quick payment");
            await Task.CompletedTask;
        }

        private async Task ViewPaymentHistoryAsync()
        {
            if (SelectedInvoice == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                var payments = await _paymentRepository.GetByInvoiceIdAsync(SelectedInvoice.InvoiceId);
                Payments.Clear();

                foreach (var payment in payments)
                {
                    Payments.Add(payment);
                }

                CalculateTotals();
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

        private async void FilterByPaymentMethod()
        {
            if (PaymentMethodFilter == null)
            {
                await LoadPaymentsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var payments = await _paymentRepository.GetByPaymentMethodAsync(PaymentMethodFilter.Value);
                Payments.Clear();

                foreach (var payment in payments)
                {
                    Payments.Add(payment);
                }

                CalculateTotals();
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

        private async void FilterByDateRange()
        {
            if (!DateFrom.HasValue && !DateTo.HasValue)
            {
                await LoadPaymentsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var fromDate = DateFrom ?? DateTime.MinValue;
                var toDate = DateTo ?? DateTime.MaxValue;

                var payments = await _paymentRepository.GetByDateRangeAsync(fromDate, toDate);
                Payments.Clear();

                foreach (var payment in payments)
                {
                    Payments.Add(payment);
                }

                CalculateTotals();
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

        private async Task UpdateInvoicePaidStatus(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                    return;

                var payments = await _paymentRepository.GetByInvoiceIdAsync(invoiceId);
                var totalPaid = payments.Sum(p => p.Amount);

                if (totalPaid >= invoice.TotalAmount)
                {
                    invoice.IsPaid = true;
                    invoice.PaidDate = DateTime.Today;
                    invoice.Status = InvoiceStatus.Paid;
                    invoice.ModifiedDate = DateTime.Now;
                    await _invoiceRepository.UpdateAsync(invoice);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating invoice status: {ex.Message}");
            }
        }

        private void CalculateTotals()
        {
            TotalPayments = Payments.Sum(p => p.Amount);
        }

        private bool ValidatePayment()
        {
            if (CurrentPayment == null)
            {
                SetError("Ingen betaling valgt");
                return false;
            }

            if (CurrentPayment.InvoiceId == 0)
            {
                SetError("Vælg en faktura");
                return false;
            }

            if (CurrentPayment.Amount <= 0)
            {
                SetError("Beløb skal være større end 0");
                return false;
            }

            if (CurrentPayment.PaymentDate > DateTime.Today)
            {
                SetError("Betalingsdato kan ikke være i fremtiden");
                return false;
            }

            // Check if payment amount exceeds invoice amount
            var invoice = UnpaidInvoices.FirstOrDefault(i => i.InvoiceId == CurrentPayment.InvoiceId);
            if (invoice != null && CurrentPayment.Amount > invoice.TotalAmount)
            {
                SetError($"Betalingsbeløb kan ikke overstige fakturabeløb ({invoice.TotalAmount:C})");
                return false;
            }

            return true;
        }

        private bool CanSavePayment()
        {
            return CurrentPayment != null &&
                   CurrentPayment.InvoiceId > 0 &&
                   CurrentPayment.Amount > 0;
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            Payments?.Clear();
            UnpaidInvoices?.Clear();
            SelectedPayment = null;
            CurrentPayment = null;
            SelectedInvoice = null;
        }

        #endregion
    }
}