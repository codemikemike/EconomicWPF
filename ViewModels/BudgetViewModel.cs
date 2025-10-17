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
    /// ViewModel for Budget management
    /// Håndterer budget oprettelse, redigering og opfølgning
    /// </summary>
    public class BudgetViewModel : ViewModelBase
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IAccountRepository _accountRepository;

        #region Properties

        private ObservableCollection<Budget> _budgets;
        public ObservableCollection<Budget> Budgets
        {
            get => _budgets;
            set => SetProperty(ref _budgets, value);
        }

        private ObservableCollection<Account> _accounts;
        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        private Budget _selectedBudget;
        public Budget SelectedBudget
        {
            get => _selectedBudget;
            set
            {
                if (SetProperty(ref _selectedBudget, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsBudgetSelected));
                }
            }
        }

        private Budget _currentBudget;
        public Budget CurrentBudget
        {
            get => _currentBudget;
            set => SetProperty(ref _currentBudget, value);
        }

        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                {
                    _ = FilterByYearAsync(); // Fire-and-forget
                }
            }
        }

        private int? _selectedMonth;
        public int? SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                {
                    _ = FilterByMonthAsync(); // Fire-and-forget
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsBudgetSelected => SelectedBudget != null;

        private decimal _totalBudgetAmount;
        public decimal TotalBudgetAmount
        {
            get => _totalBudgetAmount;
            set => SetProperty(ref _totalBudgetAmount, value);
        }

        private ObservableCollection<int> _availableYears;
        public ObservableCollection<int> AvailableYears
        {
            get => _availableYears;
            set => SetProperty(ref _availableYears, value);
        }

        #endregion

        #region Commands

        public ICommand LoadBudgetsCommand { get; }
        public ICommand AddBudgetCommand { get; }
        public ICommand EditBudgetCommand { get; }
        public ICommand SaveBudgetCommand { get; }
        public ICommand DeleteBudgetCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand NewBudgetCommand { get; }
        public ICommand CopyBudgetCommand { get; }
        public ICommand CompareBudgetCommand { get; }

        #endregion

        #region Constructor

        public BudgetViewModel(
            IBudgetRepository budgetRepository,
            IAccountRepository accountRepository)
        {
            _budgetRepository = budgetRepository ?? throw new ArgumentNullException(nameof(budgetRepository));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));

            // Initialize collections
            Budgets = new ObservableCollection<Budget>();
            Accounts = new ObservableCollection<Account>();
            AvailableYears = new ObservableCollection<int>();

            // Set default year
            SelectedYear = DateTime.Now.Year;

            // Populate years (current year +/- 2 years)
            for (int i = -2; i <= 2; i++)
            {
                AvailableYears.Add(DateTime.Now.Year + i);
            }

            // Initialize commands
            LoadBudgetsCommand = new RelayCommand(async _ => await LoadBudgetsAsync());
            AddBudgetCommand = new RelayCommand(_ => CreateNewBudget(), _ => !IsEditMode);
            EditBudgetCommand = new RelayCommand(_ => EditBudget(), _ => SelectedBudget != null);
            SaveBudgetCommand = new RelayCommand(async _ => await SaveBudgetAsync(), _ => CanSaveBudget());
            DeleteBudgetCommand = new RelayCommand(async _ => await DeleteBudgetAsync(), _ => SelectedBudget != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            NewBudgetCommand = new RelayCommand(_ => CreateNewBudget());
            CopyBudgetCommand = new RelayCommand(async _ => await CopyBudgetAsync(), _ => SelectedBudget != null);
            CompareBudgetCommand = new RelayCommand(_ => CompareBudget());

            // Load initial data
            _ = InitializeAsync();
        }

        #endregion

        #region Methods

        private async Task InitializeAsync()
        {
            await LoadAccountsAsync();
            await LoadBudgetsAsync();
        }

        private async Task LoadBudgetsAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var budgets = await _budgetRepository.GetAllAsync();
                Budgets.Clear();

                foreach (var budget in budgets)
                {
                    Budgets.Add(budget);
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

        private void CreateNewBudget()
        {
            CurrentBudget = new Budget
            {
                Year = SelectedYear,
                Month = DateTime.Now.Month,
                CreatedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedBudget = null;
        }

        private void EditBudget()
        {
            if (SelectedBudget != null)
            {
                CurrentBudget = new Budget
                {
                    BudgetId = SelectedBudget.BudgetId,
                    BudgetName = SelectedBudget.BudgetName,
                    AccountId = SelectedBudget.AccountId,
                    Year = SelectedBudget.Year,
                    Month = SelectedBudget.Month,
                    Amount = SelectedBudget.Amount,
                    Notes = SelectedBudget.Notes,
                    CreatedDate = SelectedBudget.CreatedDate
                };
                IsEditMode = true;
            }
        }

        private async Task SaveBudgetAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateBudget())
                {
                    return;
                }

                if (CurrentBudget.BudgetId == 0)
                {
                    // Nyt budget
                    CurrentBudget.CreatedDate = DateTime.Now;
                    CurrentBudget.ModifiedDate = DateTime.Now;
                    var id = await _budgetRepository.AddAsync(CurrentBudget);
                    CurrentBudget.BudgetId = id;
                    Budgets.Add(CurrentBudget);
                }
                else
                {
                    // Opdater eksisterende
                    CurrentBudget.ModifiedDate = DateTime.Now;
                    var success = await _budgetRepository.UpdateAsync(CurrentBudget);
                    if (success)
                    {
                        var existing = Budgets.FirstOrDefault(b => b.BudgetId == CurrentBudget.BudgetId);
                        if (existing != null)
                        {
                            var index = Budgets.IndexOf(existing);
                            Budgets[index] = CurrentBudget;
                        }
                    }
                }

                IsEditMode = false;
                CurrentBudget = null;
                SelectedBudget = null;

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

        private async Task DeleteBudgetAsync()
        {
            if (SelectedBudget == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Tilføj confirmation dialog

                var success = await _budgetRepository.DeleteAsync(SelectedBudget.BudgetId);
                if (success)
                {
                    Budgets.Remove(SelectedBudget);
                    SelectedBudget = null;
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
            CurrentBudget = null;
            SelectedBudget = null;
            ClearError();
        }

        private async Task CopyBudgetAsync()
        {
            if (SelectedBudget == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                var newBudget = new Budget
                {
                    BudgetName = $"{SelectedBudget.BudgetName} (Kopi)",
                    AccountId = SelectedBudget.AccountId,
                    Year = SelectedYear,
                    Month = SelectedBudget.Month,
                    Amount = SelectedBudget.Amount,
                    Notes = SelectedBudget.Notes,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                var id = await _budgetRepository.AddAsync(newBudget);
                newBudget.BudgetId = id;
                Budgets.Add(newBudget);

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

        private void CompareBudget()
        {
            // TODO: Implementer sammenligning af budget vs. faktiske tal
            System.Diagnostics.Debug.WriteLine("Compare budget with actuals");
        }

        private async Task FilterByYearAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var budgets = await _budgetRepository.GetByYearAsync(SelectedYear);
                Budgets.Clear();

                foreach (var budget in budgets)
                {
                    Budgets.Add(budget);
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

        private async Task FilterByMonthAsync()
        {
            if (!SelectedMonth.HasValue)
            {
                await FilterByYearAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var budgets = await _budgetRepository.GetByYearAndMonthAsync(SelectedYear, SelectedMonth.Value);
                Budgets.Clear();

                foreach (var budget in budgets)
                {
                    Budgets.Add(budget);
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
            TotalBudgetAmount = Budgets.Sum(b => b.Amount);
        }

        private bool ValidateBudget()
        {
            if (CurrentBudget == null)
            {
                SetError("Intet budget valgt");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentBudget.BudgetName))
            {
                SetError("Budgetnavn er påkrævet");
                return false;
            }

            if (CurrentBudget.AccountId == 0)
            {
                SetError("Vælg en konto");
                return false;
            }

            if (CurrentBudget.Year < 2000 || CurrentBudget.Year > 2100)
            {
                SetError("Ugyldigt år");
                return false;
            }

            if (CurrentBudget.Month < 1 || CurrentBudget.Month > 12)
            {
                SetError("Ugyldig måned");
                return false;
            }

            if (CurrentBudget.Amount < 0)
            {
                SetError("Beløb kan ikke være negativt");
                return false;
            }

            return true;
        }

        private bool CanSaveBudget()
        {
            return CurrentBudget != null &&
                   !string.IsNullOrWhiteSpace(CurrentBudget.BudgetName) &&
                   CurrentBudget.AccountId > 0 &&
                   CurrentBudget.Amount >= 0;
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            Budgets?.Clear();
            Accounts?.Clear();
            SelectedBudget = null;
            CurrentBudget = null;
        }

        #endregion
    }
}