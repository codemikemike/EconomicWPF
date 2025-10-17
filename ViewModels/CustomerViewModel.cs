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
    /// ViewModel for Customer management
    /// Indeholder al forretningslogik for kunde håndtering
    /// Følger MVVM pattern og SOLID principper
    /// </summary>
    public class CustomerViewModel : ViewModelBase
    {
        private readonly ICustomerRepository _customerRepository;

        #region Properties

        private ObservableCollection<Customer> _customers;
        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    // Opdater UI når valgt kunde ændres
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsCustomerSelected));
                }
            }
        }

        private Customer _currentCustomer;
        public Customer CurrentCustomer
        {
            get => _currentCustomer;
            set => SetProperty(ref _currentCustomer, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SearchCustomersAsync();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsCustomerSelected => SelectedCustomer != null;

        #endregion

        #region Commands

        public ICommand LoadCustomersCommand { get; }
        public ICommand AddCustomerCommand { get; }
        public ICommand EditCustomerCommand { get; }
        public ICommand SaveCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewCustomerCommand { get; }

        #endregion

        #region Constructor

        public CustomerViewModel(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));

            // Initialiser collections
            Customers = new ObservableCollection<Customer>();

            // Initialiser commands
            LoadCustomersCommand = new RelayCommand(async _ => await LoadCustomersAsync());
            AddCustomerCommand = new RelayCommand(_ => CreateNewCustomer(), _ => !IsEditMode);
            EditCustomerCommand = new RelayCommand(_ => EditCustomer(), _ => SelectedCustomer != null);
            SaveCustomerCommand = new RelayCommand(async _ => await SaveCustomerAsync(), _ => CanSaveCustomer());
            DeleteCustomerCommand = new RelayCommand(async _ => await DeleteCustomerAsync(), _ => SelectedCustomer != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            SearchCommand = new RelayCommand(async _ => await SearchCustomersAsync());
            NewCustomerCommand = new RelayCommand(_ => CreateNewCustomer());

            // Load initial data
            LoadCustomersAsync();
        }

        #endregion

        #region Methods

        private async Task LoadCustomersAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var customers = await _customerRepository.GetAllAsync();
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
            finally
            {
                IsLoading = false;
            }
        }

        private void CreateNewCustomer()
        {
            CurrentCustomer = new Customer
            {
                CustomerNumber = GenerateCustomerNumber(),
                Country = "Danmark",
                PaymentTermDays = 14,
                IsActive = true,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedCustomer = null;
        }

        private void EditCustomer()
        {
            if (SelectedCustomer != null)
            {
                // ✅ Manuel mapping i stedet for copy constructor
                CurrentCustomer = new Customer
                {
                    CustomerId = SelectedCustomer.CustomerId,
                    CustomerNumber = SelectedCustomer.CustomerNumber,
                    Name = SelectedCustomer.Name,
                    CVR = SelectedCustomer.CVR,
                    Email = SelectedCustomer.Email,
                    Phone = SelectedCustomer.Phone,
                    Address = SelectedCustomer.Address,
                    City = SelectedCustomer.City,
                    ZipCode = SelectedCustomer.ZipCode,
                    Country = SelectedCustomer.Country,
                    CreditLimit = SelectedCustomer.CreditLimit,
                    PaymentTermDays = SelectedCustomer.PaymentTermDays,
                    IsActive = SelectedCustomer.IsActive,
                    CreatedDate = SelectedCustomer.CreatedDate,
                    ModifiedDate = SelectedCustomer.ModifiedDate
                };
                IsEditMode = true;
            }
        }

        private async Task SaveCustomerAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateCustomer())
                {
                    return;
                }

                // Opdater ModifiedDate
                CurrentCustomer.ModifiedDate = DateTime.Now;

                if (CurrentCustomer.CustomerId == 0)
                {
                    // Ny kunde
                    CurrentCustomer.CreatedDate = DateTime.Now;
                    var id = await _customerRepository.AddAsync(CurrentCustomer);
                    CurrentCustomer.CustomerId = id;
                    Customers.Add(CurrentCustomer);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _customerRepository.UpdateAsync(CurrentCustomer);
                    if (success)
                    {
                        // Opdater i collection
                        var existing = Customers.FirstOrDefault(c => c.CustomerId == CurrentCustomer.CustomerId);
                        if (existing != null)
                        {
                            var index = Customers.IndexOf(existing);
                            Customers[index] = CurrentCustomer;
                        }
                    }
                }

                IsEditMode = false;
                CurrentCustomer = null;
                SelectedCustomer = null;
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

        private async Task DeleteCustomerAsync()
        {
            if (SelectedCustomer == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Tilføj confirmation dialog

                var success = await _customerRepository.DeleteAsync(SelectedCustomer.CustomerId);
                if (success)
                {
                    Customers.Remove(SelectedCustomer);
                    SelectedCustomer = null;
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
            CurrentCustomer = null;
            SelectedCustomer = null;
            ClearError();
        }

        private async Task SearchCustomersAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadCustomersAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var results = await _customerRepository.SearchByNameAsync(SearchText);
                Customers.Clear();

                foreach (var customer in results)
                {
                    Customers.Add(customer);
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

        private bool ValidateCustomer()
        {
            if (CurrentCustomer == null)
            {
                SetError("Ingen kunde valgt");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentCustomer.Name))
            {
                SetError("Kundenavn er påkrævet");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentCustomer.CustomerNumber))
            {
                SetError("Kundenummer er påkrævet");
                return false;
            }

            if (CurrentCustomer.PaymentTermDays < 0)
            {
                SetError("Betalingsbetingelser skal være positive");
                return false;
            }

            if (CurrentCustomer.CreditLimit < 0)
            {
                SetError("Kreditgrænse kan ikke være negativ");
                return false;
            }

            // Validér email format hvis angivet
            if (!string.IsNullOrWhiteSpace(CurrentCustomer.Email) && !IsValidEmail(CurrentCustomer.Email))
            {
                SetError("Ugyldig email adresse");
                return false;
            }

            return true;
        }

        private bool CanSaveCustomer()
        {
            return CurrentCustomer != null &&
                   !string.IsNullOrWhiteSpace(CurrentCustomer.Name) &&
                   !string.IsNullOrWhiteSpace(CurrentCustomer.CustomerNumber);
        }

        private string GenerateCustomerNumber()
        {
            // Generer næste kundenummer
            var maxNumber = 1000;

            if (Customers.Any())
            {
                var numbers = Customers
                    .Select(c => c.CustomerNumber)
                    .Where(n => int.TryParse(n, out _))
                    .Select(int.Parse)
                    .DefaultIfEmpty(1000);

                maxNumber = numbers.Max() + 1;
            }

            return maxNumber.ToString();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            Customers?.Clear();
            SelectedCustomer = null;
            CurrentCustomer = null;
        }

        #endregion
    }
}