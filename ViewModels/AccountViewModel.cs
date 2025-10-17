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
    /// ViewModel for Account management (Kontoplan)
    /// Håndterer kontoplan oprettelse, redigering og hierarki
    /// </summary>
    public class AccountViewModel : ViewModelBase
    {
        private readonly IAccountRepository _accountRepository;

        #region Properties

        private ObservableCollection<Account> _accounts;
        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        private Account _selectedAccount;
        public Account SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (SetProperty(ref _selectedAccount, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsAccountSelected));
                }
            }
        }

        private Account _currentAccount;
        public Account CurrentAccount
        {
            get => _currentAccount;
            set => SetProperty(ref _currentAccount, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterAccounts();
                }
            }
        }

        private AccountType? _typeFilter;
        public AccountType? TypeFilter
        {
            get => _typeFilter;
            set
            {
                if (SetProperty(ref _typeFilter, value))
                {
                    FilterAccountsByType();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsAccountSelected => SelectedAccount != null;

        private ObservableCollection<Account> _parentAccounts;
        public ObservableCollection<Account> ParentAccounts
        {
            get => _parentAccounts;
            set => SetProperty(ref _parentAccounts, value);
        }

        #endregion

        #region Commands

        public ICommand LoadAccountsCommand { get; }
        public ICommand AddAccountCommand { get; }
        public ICommand EditAccountCommand { get; }
        public ICommand SaveAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewAccountCommand { get; }
        public ICommand ViewHierarchyCommand { get; }

        #endregion

        #region Constructor

        public AccountViewModel(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));

            // Initialize collections
            Accounts = new ObservableCollection<Account>();
            ParentAccounts = new ObservableCollection<Account>();

            // Initialize commands
            LoadAccountsCommand = new RelayCommand(async _ => await LoadAccountsAsync());
            AddAccountCommand = new RelayCommand(_ => CreateNewAccount(), _ => !IsEditMode);
            EditAccountCommand = new RelayCommand(_ => EditAccount(), _ => SelectedAccount != null);
            SaveAccountCommand = new RelayCommand(async _ => await SaveAccountAsync(), _ => CanSaveAccount());
            DeleteAccountCommand = new RelayCommand(async _ => await DeleteAccountAsync(), _ => SelectedAccount != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            SearchCommand = new RelayCommand(_ => FilterAccounts());
            NewAccountCommand = new RelayCommand(_ => CreateNewAccount());
            ViewHierarchyCommand = new RelayCommand(async _ => await LoadAccountHierarchyAsync());

            // Load initial data
            LoadAccountsAsync();
        }

        #endregion

        #region Methods

        private async Task LoadAccountsAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var accounts = await _accountRepository.GetAllAsync();
                Accounts.Clear();
                ParentAccounts.Clear();

                // Add null option for no parent
                ParentAccounts.Add(new Account { AccountId = 0, AccountName = "(Ingen overordnet konto)" });

                foreach (var account in accounts)
                {
                    Accounts.Add(account);
                    ParentAccounts.Add(account);
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

        private async Task LoadAccountHierarchyAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var accounts = await _accountRepository.GetAccountHierarchyAsync();
                Accounts.Clear();

                foreach (var account in accounts)
                {
                    Accounts.Add(account);
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

        private void CreateNewAccount()
        {
            CurrentAccount = new Account
            {
                AccountNumber = GenerateAccountNumber(),
                AccountType = AccountType.Asset,
                IsActive = true,
                Balance = 0,
                CreatedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedAccount = null;
        }

        private void EditAccount()
        {
            if (SelectedAccount != null)
            {
                CurrentAccount = new Account
                {
                    AccountId = SelectedAccount.AccountId,
                    AccountNumber = SelectedAccount.AccountNumber,
                    AccountName = SelectedAccount.AccountName,
                    AccountType = SelectedAccount.AccountType,
                    ParentAccountId = SelectedAccount.ParentAccountId,
                    Balance = SelectedAccount.Balance,
                    IsActive = SelectedAccount.IsActive,
                    CreatedDate = SelectedAccount.CreatedDate
                };
                IsEditMode = true;
            }
        }

        private async Task SaveAccountAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateAccount())
                {
                    return;
                }

                if (CurrentAccount.AccountId == 0)
                {
                    // Ny konto
                    CurrentAccount.CreatedDate = DateTime.Now;
                    var id = await _accountRepository.AddAsync(CurrentAccount);
                    CurrentAccount.AccountId = id;
                    Accounts.Add(CurrentAccount);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _accountRepository.UpdateAsync(CurrentAccount);
                    if (success)
                    {
                        var existing = Accounts.FirstOrDefault(a => a.AccountId == CurrentAccount.AccountId);
                        if (existing != null)
                        {
                            var index = Accounts.IndexOf(existing);
                            Accounts[index] = CurrentAccount;
                        }
                    }
                }

                IsEditMode = false;
                CurrentAccount = null;
                SelectedAccount = null;

                // Reload to update parent accounts
                await LoadAccountsAsync();
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

        private async Task DeleteAccountAsync()
        {
            if (SelectedAccount == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Check if account has transactions before deleting
                // TODO: Tilføj confirmation dialog

                var success = await _accountRepository.DeleteAsync(SelectedAccount.AccountId);
                if (success)
                {
                    Accounts.Remove(SelectedAccount);
                    SelectedAccount = null;
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
            CurrentAccount = null;
            SelectedAccount = null;
            ClearError();
        }

        private void FilterAccounts()
        {
            // Simple filtering - could be enhanced with search in repository
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadAccountsAsync();
                return;
            }

            var filtered = Accounts.Where(a =>
                a.AccountNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                a.AccountName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            Accounts.Clear();
            foreach (var account in filtered)
            {
                Accounts.Add(account);
            }
        }

        private async void FilterAccountsByType()
        {
            if (TypeFilter == null)
            {
                await LoadAccountsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var accounts = await _accountRepository.GetByTypeAsync(TypeFilter.Value);
                Accounts.Clear();

                foreach (var account in accounts)
                {
                    Accounts.Add(account);
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

        private bool ValidateAccount()
        {
            if (CurrentAccount == null)
            {
                SetError("Ingen konto valgt");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentAccount.AccountNumber))
            {
                SetError("Kontonummer er påkrævet");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentAccount.AccountName))
            {
                SetError("Kontonavn er påkrævet");
                return false;
            }

            // Check for circular reference in parent account
            if (CurrentAccount.ParentAccountId.HasValue &&
                CurrentAccount.ParentAccountId.Value == CurrentAccount.AccountId)
            {
                SetError("En konto kan ikke være sin egen overordnede konto");
                return false;
            }

            return true;
        }

        private bool CanSaveAccount()
        {
            return CurrentAccount != null &&
                   !string.IsNullOrWhiteSpace(CurrentAccount.AccountNumber) &&
                   !string.IsNullOrWhiteSpace(CurrentAccount.AccountName);
        }

        private string GenerateAccountNumber()
        {
            var maxNumber = 1000;

            if (Accounts.Any())
            {
                var numbers = Accounts
                    .Select(a => a.AccountNumber)
                    .Where(n => int.TryParse(n, out _))
                    .Select(int.Parse)
                    .DefaultIfEmpty(1000);

                maxNumber = numbers.Max() + 1;
            }

            return maxNumber.ToString();
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            Accounts?.Clear();
            ParentAccounts?.Clear();
            SelectedAccount = null;
            CurrentAccount = null;
        }

        #endregion
    }
}