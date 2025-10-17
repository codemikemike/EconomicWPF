using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EconomicWPF.Commands;
using EconomicWPF.Models;
using EconomicWPF.Repositories.Interfaces;
using EconomicWPF.ViewModels.Base;

namespace EconomicWPF.ViewModels
{
    /// <summary>
    /// ViewModel for Transaction management (Bogføring)
    /// Håndterer posteringer og bogføring
    /// </summary>
    public class TransactionViewModel : ViewModelBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IDimensionRepository _dimensionRepository;

        #region Properties

        private ObservableCollection<Transaction> _transactions;
        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        private ObservableCollection<Account> _accounts;
        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        private ObservableCollection<Invoice> _invoices;
        public ObservableCollection<Invoice> Invoices
        {
            get => _invoices;
            set => SetProperty(ref _invoices, value);
        }

        private ObservableCollection<Project> _projects;
        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

        private ObservableCollection<Dimension> _dimensions;
        public ObservableCollection<Dimension> Dimensions
        {
            get => _dimensions;
            set => SetProperty(ref _dimensions, value);
        }

        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                if (SetProperty(ref _selectedTransaction, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsTransactionSelected));
                }
            }
        }

        private Transaction _currentTransaction;
        public Transaction CurrentTransaction
        {
            get => _currentTransaction;
            set => SetProperty(ref _currentTransaction, value);
        }

        private Account _selectedAccount;
        public Account SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (SetProperty(ref _selectedAccount, value) && CurrentTransaction != null)
                {
                    CurrentTransaction.AccountId = value?.AccountId ?? 0;
                    CurrentTransaction.Account = value;
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

        private bool _showPostedOnly;
        public bool ShowPostedOnly
        {
            get => _showPostedOnly;
            set
            {
                if (SetProperty(ref _showPostedOnly, value))
                {
                    FilterTransactions();
                }
            }
        }

        private bool _showUnpostedOnly;
        public bool ShowUnpostedOnly
        {
            get => _showUnpostedOnly;
            set
            {
                if (SetProperty(ref _showUnpostedOnly, value))
                {
                    FilterTransactions();
                }
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterTransactions();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsTransactionSelected => SelectedTransaction != null;

        private decimal _totalDebit;
        public decimal TotalDebit
        {
            get => _totalDebit;
            set => SetProperty(ref _totalDebit, value);
        }

        private decimal _totalCredit;
        public decimal TotalCredit
        {
            get => _totalCredit;
            set => SetProperty(ref _totalCredit, value);
        }

        private decimal _balance;
        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        #endregion

        #region Commands

        public ICommand LoadTransactionsCommand { get; }
        public ICommand AddTransactionCommand { get; }
        public ICommand EditTransactionCommand { get; }
        public ICommand SaveTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewTransactionCommand { get; }
        public ICommand PostTransactionCommand { get; }
        public ICommand UnpostTransactionCommand { get; }
        public ICommand ViewJournalCommand { get; }
        public ICommand ExportJournalCommand { get; }

        #endregion

        #region Constructor

        public TransactionViewModel(
            ITransactionRepository transactionRepository,
            IAccountRepository accountRepository,
            IInvoiceRepository invoiceRepository,
            IProjectRepository projectRepository,
            IDimensionRepository dimensionRepository)
        {
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _invoiceRepository = invoiceRepository ?? throw new ArgumentNullException(nameof(invoiceRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _dimensionRepository = dimensionRepository ?? throw new ArgumentNullException(nameof(dimensionRepository));

            // Initialize collections
            Transactions = new ObservableCollection<Transaction>();
            Accounts = new ObservableCollection<Account>();
            Invoices = new ObservableCollection<Invoice>();
            Projects = new ObservableCollection<Project>();
            Dimensions = new ObservableCollection<Dimension>();

            // Initialize commands
            LoadTransactionsCommand = new RelayCommand(async _ => await LoadTransactionsAsync());
            AddTransactionCommand = new RelayCommand(_ => CreateNewTransaction(), _ => !IsEditMode);
            EditTransactionCommand = new RelayCommand(_ => EditTransaction(), _ => SelectedTransaction != null && !SelectedTransaction.IsPosted);
            SaveTransactionCommand = new RelayCommand(async _ => await SaveTransactionAsync(), _ => CanSaveTransaction());
            DeleteTransactionCommand = new RelayCommand(async _ => await DeleteTransactionAsync(), _ => SelectedTransaction != null && !SelectedTransaction.IsPosted);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            SearchCommand = new RelayCommand(_ => FilterTransactions());
            NewTransactionCommand = new RelayCommand(_ => CreateNewTransaction());
            PostTransactionCommand = new RelayCommand(async _ => await PostTransactionAsync(), _ => SelectedTransaction != null && !SelectedTransaction.IsPosted);
            UnpostTransactionCommand = new RelayCommand(async _ => await UnpostTransactionAsync(), _ => SelectedTransaction != null && SelectedTransaction.IsPosted);
            ViewJournalCommand = new RelayCommand(async _ => await ViewJournalAsync());
            ExportJournalCommand = new RelayCommand(_ => ExportJournal());

            // Load initial data
            InitializeAsync();
        }

        #endregion

        #region Methods

        private async Task InitializeAsync()
        {
            await LoadAccountsAsync();
            await LoadInvoicesAsync();
            await LoadProjectsAsync();
            await LoadDimensionsAsync();
            await LoadTransactionsAsync();
        }

        private async Task LoadTransactionsAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var transactions = await _transactionRepository.GetAllAsync();
                Transactions.Clear();

                foreach (var transaction in transactions)
                {
                    Transactions.Add(transaction);
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

        private async Task LoadAccountsAsync()
        {
            try
            {
                var accounts = await _accountRepository.GetAllAsync();
                Accounts.Clear();

                foreach (var account in accounts.Where(a => a.IsActive))
                {
                    Accounts.Add(account);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async Task LoadInvoicesAsync()
        {
            try
            {
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
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                var projects = await _projectRepository.GetAllAsync();
                Projects.Clear();

                foreach (var project in projects.Where(p => p.IsActive))
                {
                    Projects.Add(project);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private async Task LoadDimensionsAsync()
        {
            try
            {
                var dimensions = await _dimensionRepository.GetAllAsync();
                Dimensions.Clear();

                foreach (var dimension in dimensions.Where(d => d.IsActive))
                {
                    Dimensions.Add(dimension);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void CreateNewTransaction()
        {
            CurrentTransaction = new Transaction
            {
                TransactionDate = DateTime.Today,
                IsPosted = false,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedTransaction = null;
        }

        private void EditTransaction()
        {
            if (SelectedTransaction != null && !SelectedTransaction.IsPosted)
            {
                CurrentTransaction = new Transaction
                {
                    TransactionId = SelectedTransaction.TransactionId,
                    TransactionDate = SelectedTransaction.TransactionDate,
                    AccountId = SelectedTransaction.AccountId,
                    DebitAmount = SelectedTransaction.DebitAmount,
                    CreditAmount = SelectedTransaction.CreditAmount,
                    Description = SelectedTransaction.Description,
                    VoucherNumber = SelectedTransaction.VoucherNumber,
                    InvoiceId = SelectedTransaction.InvoiceId,
                    ProjectId = SelectedTransaction.ProjectId,
                    DimensionId = SelectedTransaction.DimensionId,
                    IsPosted = SelectedTransaction.IsPosted,
                    CreatedDate = SelectedTransaction.CreatedDate,
                    ModifiedDate = SelectedTransaction.ModifiedDate
                };

                SelectedAccount = Accounts.FirstOrDefault(a => a.AccountId == CurrentTransaction.AccountId);
                IsEditMode = true;
            }
        }

        private async Task SaveTransactionAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateTransaction())
                {
                    return;
                }

                CurrentTransaction.ModifiedDate = DateTime.Now;

                if (CurrentTransaction.TransactionId == 0)
                {
                    // Ny transaktion
                    CurrentTransaction.CreatedDate = DateTime.Now;
                    CurrentTransaction.VoucherNumber = await GenerateVoucherNumberAsync();
                    var id = await _transactionRepository.AddAsync(CurrentTransaction);
                    CurrentTransaction.TransactionId = id;
                    Transactions.Add(CurrentTransaction);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _transactionRepository.UpdateAsync(CurrentTransaction);
                    if (success)
                    {
                        var existing = Transactions.FirstOrDefault(t => t.TransactionId == CurrentTransaction.TransactionId);
                        if (existing != null)
                        {
                            var index = Transactions.IndexOf(existing);
                            Transactions[index] = CurrentTransaction;
                        }
                    }
                }

                IsEditMode = false;
                CurrentTransaction = null;
                SelectedTransaction = null;
                SelectedAccount = null;

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

        private async Task DeleteTransactionAsync()
        {
            if (SelectedTransaction == null || SelectedTransaction.IsPosted)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Tilføj confirmation dialog

                var success = await _transactionRepository.DeleteAsync(SelectedTransaction.TransactionId);
                if (success)
                {
                    Transactions.Remove(SelectedTransaction);
                    SelectedTransaction = null;
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
            CurrentTransaction = null;
            SelectedTransaction = null;
            SelectedAccount = null;
            ClearError();
        }

        private async Task PostTransactionAsync()
        {
            if (SelectedTransaction == null || SelectedTransaction.IsPosted)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                SelectedTransaction.IsPosted = true;
                SelectedTransaction.ModifiedDate = DateTime.Now;
                var success = await _transactionRepository.UpdateAsync(SelectedTransaction);

                if (success)
                {
                    // Update account balance
                    var netAmount = SelectedTransaction.DebitAmount - SelectedTransaction.CreditAmount;
                    await _accountRepository.UpdateBalanceAsync(SelectedTransaction.AccountId, netAmount);

                    await LoadTransactionsAsync();
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

        private async Task UnpostTransactionAsync()
        {
            if (SelectedTransaction == null || !SelectedTransaction.IsPosted)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Add security check - only certain users should be able to unpost

                SelectedTransaction.IsPosted = false;
                SelectedTransaction.ModifiedDate = DateTime.Now;
                var success = await _transactionRepository.UpdateAsync(SelectedTransaction);

                if (success)
                {
                    // Reverse account balance
                    var netAmount = -(SelectedTransaction.DebitAmount - SelectedTransaction.CreditAmount);
                    await _accountRepository.UpdateBalanceAsync(SelectedTransaction.AccountId, netAmount);

                    await LoadTransactionsAsync();
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

        private async Task ViewJournalAsync()
        {
            // Load transactions for current month
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            DateFrom = firstDayOfMonth;
            DateTo = lastDayOfMonth;

            await FilterByDateRange();
        }

        private void ExportJournal()
        {
            // TODO: Implementer eksport til Excel/CSV
            System.Diagnostics.Debug.WriteLine("Export journal");
        }

        private void FilterTransactions()
        {
            var filtered = Transactions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t =>
                    (t.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.VoucherNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (ShowPostedOnly)
            {
                filtered = filtered.Where(t => t.IsPosted);
            }

            if (ShowUnpostedOnly)
            {
                filtered = filtered.Where(t => !t.IsPosted);
            }

            var result = filtered.ToList();
            Transactions.Clear();
            foreach (var transaction in result)
            {
                Transactions.Add(transaction);
            }

            CalculateTotals();
        }

        private async Task FilterByDateRange()
        {
            if (!DateFrom.HasValue && !DateTo.HasValue)
            {
                await LoadTransactionsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var fromDate = DateFrom ?? DateTime.MinValue;
                var toDate = DateTo ?? DateTime.MaxValue;

                var transactions = await _transactionRepository.GetByDateRangeAsync(fromDate, toDate);
                Transactions.Clear();

                foreach (var transaction in transactions)
                {
                    Transactions.Add(transaction);
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

        private void CalculateTotals()
        {
            TotalDebit = Transactions.Sum(t => t.DebitAmount);
            TotalCredit = Transactions.Sum(t => t.CreditAmount);
            Balance = TotalDebit - TotalCredit;
        }

        private async Task<string> GenerateVoucherNumberAsync()
        {
            var year = DateTime.Now.Year;
            var maxNumber = 1;

            var existingTransactions = await _transactionRepository.GetByYearAsync(year);

            if (existingTransactions.Any())
            {
                var numbers = existingTransactions
                    .Select(t => t.VoucherNumber)
                    .Where(vn => !string.IsNullOrEmpty(vn) && vn.StartsWith($"{year}-"))
                    .Select(vn => int.TryParse(vn.Split('-')[1], out var num) ? num : 0)
                    .DefaultIfEmpty(0);

                maxNumber = numbers.Max() + 1;
            }

            return $"{year}-{maxNumber:D4}";
        }

        private bool ValidateTransaction()
        {
            if (CurrentTransaction == null)
            {
                SetError("Ingen transaktion valgt");
                return false;
            }

            if (CurrentTransaction.AccountId == 0)
            {
                SetError("Vælg en konto");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentTransaction.Description))
            {
                SetError("Beskrivelse er påkrævet");
                return false;
            }

            if (CurrentTransaction.DebitAmount < 0 || CurrentTransaction.CreditAmount < 0)
            {
                SetError("Beløb kan ikke være negative");
                return false;
            }

            if (CurrentTransaction.DebitAmount > 0 && CurrentTransaction.CreditAmount > 0)
            {
                SetError("En transaktion kan ikke have både debet og kredit");
                return false;
            }

            if (CurrentTransaction.DebitAmount == 0 && CurrentTransaction.CreditAmount == 0)
            {
                SetError("Transaktion skal have enten debet eller kredit beløb");
                return false;
            }

            if (CurrentTransaction.TransactionDate > DateTime.Today)
            {
                SetError("Transaktionsdato kan ikke være i fremtiden");
                return false;
            }

            return true;
        }

        private bool CanSaveTransaction()
        {
            return CurrentTransaction != null &&
                   CurrentTransaction.AccountId > 0 &&
                   !string.IsNullOrWhiteSpace(CurrentTransaction.Description) &&
                   (CurrentTransaction.DebitAmount > 0 || CurrentTransaction.CreditAmount > 0);
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            Transactions?.Clear();
            Accounts?.Clear();
            Invoices?.Clear();
            Projects?.Clear();
            Dimensions?.Clear();
            SelectedTransaction = null;
            CurrentTransaction = null;
            SelectedAccount = null;
        }

        #endregion
    }
}