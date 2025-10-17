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
    /// ViewModel for BankTransaction management
    /// Håndterer banktransaktioner og afstemning
    /// </summary>
    public class BankTransactionViewModel : ViewModelBase
    {
        private readonly IBankTransactionRepository _bankTransactionRepository;
        private readonly ITransactionRepository _transactionRepository;

        #region Properties

        private ObservableCollection<BankTransaction> _bankTransactions;
        public ObservableCollection<BankTransaction> BankTransactions
        {
            get => _bankTransactions;
            set => SetProperty(ref _bankTransactions, value);
        }

        private BankTransaction _selectedBankTransaction;
        public BankTransaction SelectedBankTransaction
        {
            get => _selectedBankTransaction;
            set
            {
                if (SetProperty(ref _selectedBankTransaction, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsBankTransactionSelected));
                }
            }
        }

        private BankTransaction _currentBankTransaction;
        public BankTransaction CurrentBankTransaction
        {
            get => _currentBankTransaction;
            set => SetProperty(ref _currentBankTransaction, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterBankTransactions();
                }
            }
        }

        private bool _showReconciledOnly;
        public bool ShowReconciledOnly
        {
            get => _showReconciledOnly;
            set
            {
                if (SetProperty(ref _showReconciledOnly, value))
                {
                    FilterBankTransactions();
                }
            }
        }

        private bool _showUnreconciledOnly;
        public bool ShowUnreconciledOnly
        {
            get => _showUnreconciledOnly;
            set
            {
                if (SetProperty(ref _showUnreconciledOnly, value))
                {
                    FilterBankTransactions();
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

        public bool IsBankTransactionSelected => SelectedBankTransaction != null;

        private decimal _totalDeposits;
        public decimal TotalDeposits
        {
            get => _totalDeposits;
            set => SetProperty(ref _totalDeposits, value);
        }

        private decimal _totalWithdrawals;
        public decimal TotalWithdrawals
        {
            get => _totalWithdrawals;
            set => SetProperty(ref _totalWithdrawals, value);
        }

        private decimal _netAmount;
        public decimal NetAmount
        {
            get => _netAmount;
            set => SetProperty(ref _netAmount, value);
        }

        #endregion

        #region Commands

        public ICommand LoadBankTransactionsCommand { get; }
        public ICommand AddBankTransactionCommand { get; }
        public ICommand EditBankTransactionCommand { get; }
        public ICommand SaveBankTransactionCommand { get; }
        public ICommand DeleteBankTransactionCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewBankTransactionCommand { get; }
        public ICommand ReconcileCommand { get; }
        public ICommand UnreconcileCommand { get; }
        public ICommand ImportBankStatementCommand { get; }

        #endregion

        #region Constructor

        public BankTransactionViewModel(
            IBankTransactionRepository bankTransactionRepository,
            ITransactionRepository transactionRepository)
        {
            _bankTransactionRepository = bankTransactionRepository ?? throw new ArgumentNullException(nameof(bankTransactionRepository));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));

            // Initialize collections
            BankTransactions = new ObservableCollection<BankTransaction>();

            // Initialize commands
            LoadBankTransactionsCommand = new RelayCommand(async _ => await LoadBankTransactionsAsync());
            AddBankTransactionCommand = new RelayCommand(_ => CreateNewBankTransaction(), _ => !IsEditMode);
            EditBankTransactionCommand = new RelayCommand(_ => EditBankTransaction(), _ => SelectedBankTransaction != null);
            SaveBankTransactionCommand = new RelayCommand(async _ => await SaveBankTransactionAsync(), _ => CanSaveBankTransaction());
            DeleteBankTransactionCommand = new RelayCommand(async _ => await DeleteBankTransactionAsync(), _ => SelectedBankTransaction != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            SearchCommand = new RelayCommand(_ => FilterBankTransactions());
            NewBankTransactionCommand = new RelayCommand(_ => CreateNewBankTransaction());
            ReconcileCommand = new RelayCommand(async _ => await ReconcileBankTransactionAsync(), _ => SelectedBankTransaction != null && !SelectedBankTransaction.IsReconciled);
            UnreconcileCommand = new RelayCommand(async _ => await UnreconcileBankTransactionAsync(), _ => SelectedBankTransaction != null && SelectedBankTransaction.IsReconciled);
            ImportBankStatementCommand = new RelayCommand(_ => ImportBankStatement());

            // Load initial data
            LoadBankTransactionsAsync();
        }

        #endregion

        #region Methods

        private async Task LoadBankTransactionsAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var transactions = await _bankTransactionRepository.GetAllAsync();
                BankTransactions.Clear();

                foreach (var transaction in transactions)
                {
                    BankTransactions.Add(transaction);
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

        private void CreateNewBankTransaction()
        {
            CurrentBankTransaction = new BankTransaction
            {
                TransactionDate = DateTime.Today,
                IsReconciled = false,
                CreatedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedBankTransaction = null;
        }

        private void EditBankTransaction()
        {
            if (SelectedBankTransaction != null)
            {
                CurrentBankTransaction = new BankTransaction
                {
                    BankTransactionId = SelectedBankTransaction.BankTransactionId,
                    TransactionDate = SelectedBankTransaction.TransactionDate,
                    Description = SelectedBankTransaction.Description,
                    Amount = SelectedBankTransaction.Amount,
                    Reference = SelectedBankTransaction.Reference,
                    IsReconciled = SelectedBankTransaction.IsReconciled,
                    TransactionId = SelectedBankTransaction.TransactionId,
                    CreatedDate = SelectedBankTransaction.CreatedDate
                };
                IsEditMode = true;
            }
        }

        private async Task SaveBankTransactionAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateBankTransaction())
                {
                    return;
                }

                if (CurrentBankTransaction.BankTransactionId == 0)
                {
                    // Ny transaktion
                    CurrentBankTransaction.CreatedDate = DateTime.Now;
                    var id = await _bankTransactionRepository.AddAsync(CurrentBankTransaction);
                    CurrentBankTransaction.BankTransactionId = id;
                    BankTransactions.Add(CurrentBankTransaction);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _bankTransactionRepository.UpdateAsync(CurrentBankTransaction);
                    if (success)
                    {
                        var existing = BankTransactions.FirstOrDefault(bt => bt.BankTransactionId == CurrentBankTransaction.BankTransactionId);
                        if (existing != null)
                        {
                            var index = BankTransactions.IndexOf(existing);
                            BankTransactions[index] = CurrentBankTransaction;
                        }
                    }
                }

                IsEditMode = false;
                CurrentBankTransaction = null;
                SelectedBankTransaction = null;

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

        private async Task DeleteBankTransactionAsync()
        {
            if (SelectedBankTransaction == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Tilføj confirmation dialog

                var success = await _bankTransactionRepository.DeleteAsync(SelectedBankTransaction.BankTransactionId);
                if (success)
                {
                    BankTransactions.Remove(SelectedBankTransaction);
                    SelectedBankTransaction = null;
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
            CurrentBankTransaction = null;
            SelectedBankTransaction = null;
            ClearError();
        }

        private async Task ReconcileBankTransactionAsync()
        {
            if (SelectedBankTransaction == null || SelectedBankTransaction.IsReconciled)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                SelectedBankTransaction.IsReconciled = true;
                var success = await _bankTransactionRepository.UpdateAsync(SelectedBankTransaction);

                if (success)
                {
                    await LoadBankTransactionsAsync();
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

        private async Task UnreconcileBankTransactionAsync()
        {
            if (SelectedBankTransaction == null || !SelectedBankTransaction.IsReconciled)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                SelectedBankTransaction.IsReconciled = false;
                var success = await _bankTransactionRepository.UpdateAsync(SelectedBankTransaction);

                if (success)
                {
                    await LoadBankTransactionsAsync();
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

        private void FilterBankTransactions()
        {
            // Simple filtering - could be enhanced
            var filtered = BankTransactions.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(bt =>
                    (bt.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (bt.Reference?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (ShowReconciledOnly)
            {
                filtered = filtered.Where(bt => bt.IsReconciled);
            }

            if (ShowUnreconciledOnly)
            {
                filtered = filtered.Where(bt => !bt.IsReconciled);
            }

            var result = filtered.ToList();
            BankTransactions.Clear();
            foreach (var transaction in result)
            {
                BankTransactions.Add(transaction);
            }

            CalculateTotals();
        }

        private async void FilterByDateRange()
        {
            if (!DateFrom.HasValue && !DateTo.HasValue)
            {
                await LoadBankTransactionsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var fromDate = DateFrom ?? DateTime.MinValue;
                var toDate = DateTo ?? DateTime.MaxValue;

                var transactions = await _bankTransactionRepository.GetByDateRangeAsync(fromDate, toDate);
                BankTransactions.Clear();

                foreach (var transaction in transactions)
                {
                    BankTransactions.Add(transaction);
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

        private void ImportBankStatement()
        {
            // TODO: Implementer import af kontoudtog (CSV/Excel)
            System.Diagnostics.Debug.WriteLine("Import bank statement");
        }

        private void CalculateTotals()
        {
            TotalDeposits = BankTransactions.Where(bt => bt.Amount > 0).Sum(bt => bt.Amount);
            TotalWithdrawals = BankTransactions.Where(bt => bt.Amount < 0).Sum(bt => Math.Abs(bt.Amount));
            NetAmount = TotalDeposits - TotalWithdrawals;
        }

        private bool ValidateBankTransaction()
        {
            if (CurrentBankTransaction == null)
            {
                SetError("Ingen banktransaktion valgt");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentBankTransaction.Description))
            {
                SetError("Beskrivelse er påkrævet");
                return false;
            }

            if (CurrentBankTransaction.Amount == 0)
            {
                SetError("Beløb kan ikke være 0");
                return false;
            }

            if (CurrentBankTransaction.TransactionDate > DateTime.Today)
            {
                SetError("Transaktionsdato kan ikke være i fremtiden");
                return false;
            }

            return true;
        }

        private bool CanSaveBankTransaction()
        {
            return CurrentBankTransaction != null &&
                   !string.IsNullOrWhiteSpace(CurrentBankTransaction.Description) &&
                   CurrentBankTransaction.Amount != 0;
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            BankTransactions?.Clear();
            SelectedBankTransaction = null;
            CurrentBankTransaction = null;
        }

        #endregion
    }
}