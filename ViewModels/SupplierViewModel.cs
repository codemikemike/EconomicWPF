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
    /// ViewModel for Supplier management (Leverandører)
    /// Håndterer leverandør oprettelse, redigering og styring
    /// </summary>
    public class SupplierViewModel : ViewModelBase
    {
        private readonly ISupplierRepository _supplierRepository;

        #region Properties

        private ObservableCollection<Supplier> _suppliers;
        public ObservableCollection<Supplier> Suppliers
        {
            get => _suppliers;
            set => SetProperty(ref _suppliers, value);
        }

        private Supplier _selectedSupplier;
        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (SetProperty(ref _selectedSupplier, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsSupplierSelected));
                }
            }
        }

        private Supplier _currentSupplier;
        public Supplier CurrentSupplier
        {
            get => _currentSupplier;
            set => SetProperty(ref _currentSupplier, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SearchSuppliersAsync();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsSupplierSelected => SelectedSupplier != null;

        #endregion

        #region Commands

        public ICommand LoadSuppliersCommand { get; }
        public ICommand AddSupplierCommand { get; }
        public ICommand EditSupplierCommand { get; }
        public ICommand SaveSupplierCommand { get; }
        public ICommand DeleteSupplierCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewSupplierCommand { get; }

        #endregion

        #region Constructor

        public SupplierViewModel(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));

            // Initialize collections
            Suppliers = new ObservableCollection<Supplier>();

            // Initialize commands
            LoadSuppliersCommand = new RelayCommand(async _ => await LoadSuppliersAsync());
            AddSupplierCommand = new RelayCommand(_ => CreateNewSupplier(), _ => !IsEditMode);
            EditSupplierCommand = new RelayCommand(_ => EditSupplier(), _ => SelectedSupplier != null);
            SaveSupplierCommand = new RelayCommand(async _ => await SaveSupplierAsync(), _ => CanSaveSupplier());
            DeleteSupplierCommand = new RelayCommand(async _ => await DeleteSupplierAsync(), _ => SelectedSupplier != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            SearchCommand = new RelayCommand(async _ => await SearchSuppliersAsync());
            NewSupplierCommand = new RelayCommand(_ => CreateNewSupplier());

            // Load initial data
            LoadSuppliersAsync();
        }

        #endregion

        #region Methods

        private async Task LoadSuppliersAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var suppliers = await _supplierRepository.GetAllAsync();
                Suppliers.Clear();

                foreach (var supplier in suppliers)
                {
                    Suppliers.Add(supplier);
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

        private void CreateNewSupplier()
        {
            CurrentSupplier = new Supplier
            {
                SupplierNumber = GenerateSupplierNumber(),
                Country = "Danmark",
                PaymentTermDays = 30,
                IsActive = true,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedSupplier = null;
        }

        private void EditSupplier()
        {
            if (SelectedSupplier != null)
            {
                CurrentSupplier = new Supplier
                {
                    SupplierId = SelectedSupplier.SupplierId,
                    SupplierNumber = SelectedSupplier.SupplierNumber,
                    Name = SelectedSupplier.Name,
                    CVR = SelectedSupplier.CVR,
                    Email = SelectedSupplier.Email,
                    Phone = SelectedSupplier.Phone,
                    Address = SelectedSupplier.Address,
                    City = SelectedSupplier.City,
                    ZipCode = SelectedSupplier.ZipCode,
                    Country = SelectedSupplier.Country,
                    PaymentTermDays = SelectedSupplier.PaymentTermDays,
                    IsActive = SelectedSupplier.IsActive,
                    CreatedDate = SelectedSupplier.CreatedDate,
                    ModifiedDate = SelectedSupplier.ModifiedDate
                };
                IsEditMode = true;
            }
        }

        private async Task SaveSupplierAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateSupplier())
                {
                    return;
                }

                // Opdater ModifiedDate
                CurrentSupplier.ModifiedDate = DateTime.Now;

                if (CurrentSupplier.SupplierId == 0)
                {
                    // Ny leverandør
                    CurrentSupplier.CreatedDate = DateTime.Now;
                    var id = await _supplierRepository.AddAsync(CurrentSupplier);
                    CurrentSupplier.SupplierId = id;
                    Suppliers.Add(CurrentSupplier);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _supplierRepository.UpdateAsync(CurrentSupplier);
                    if (success)
                    {
                        var existing = Suppliers.FirstOrDefault(s => s.SupplierId == CurrentSupplier.SupplierId);
                        if (existing != null)
                        {
                            var index = Suppliers.IndexOf(existing);
                            Suppliers[index] = CurrentSupplier;
                        }
                    }
                }

                IsEditMode = false;
                CurrentSupplier = null;
                SelectedSupplier = null;
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

        private async Task DeleteSupplierAsync()
        {
            if (SelectedSupplier == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Check if supplier has purchase orders/invoices before deleting
                // TODO: Tilføj confirmation dialog

                var success = await _supplierRepository.DeleteAsync(SelectedSupplier.SupplierId);
                if (success)
                {
                    Suppliers.Remove(SelectedSupplier);
                    SelectedSupplier = null;
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
            CurrentSupplier = null;
            SelectedSupplier = null;
            ClearError();
        }

        private async Task SearchSuppliersAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadSuppliersAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var results = await _supplierRepository.SearchByNameAsync(SearchText);
                Suppliers.Clear();

                foreach (var supplier in results)
                {
                    Suppliers.Add(supplier);
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

        private bool ValidateSupplier()
        {
            if (CurrentSupplier == null)
            {
                SetError("Ingen leverandør valgt");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentSupplier.Name))
            {
                SetError("Leverandørnavn er påkrævet");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentSupplier.SupplierNumber))
            {
                SetError("Leverandørnummer er påkrævet");
                return false;
            }

            if (CurrentSupplier.PaymentTermDays < 0)
            {
                SetError("Betalingsbetingelser skal være positive");
                return false;
            }

            // Validér email format hvis angivet
            if (!string.IsNullOrWhiteSpace(CurrentSupplier.Email) && !IsValidEmail(CurrentSupplier.Email))
            {
                SetError("Ugyldig email adresse");
                return false;
            }

            return true;
        }

        private bool CanSaveSupplier()
        {
            return CurrentSupplier != null &&
                   !string.IsNullOrWhiteSpace(CurrentSupplier.Name) &&
                   !string.IsNullOrWhiteSpace(CurrentSupplier.SupplierNumber);
        }

        private string GenerateSupplierNumber()
        {
            var maxNumber = 2000;

            if (Suppliers.Any())
            {
                var numbers = Suppliers
                    .Select(s => s.SupplierNumber)
                    .Where(n => int.TryParse(n, out _))
                    .Select(int.Parse)
                    .DefaultIfEmpty(2000);

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
            Suppliers?.Clear();
            SelectedSupplier = null;
            CurrentSupplier = null;
        }

        #endregion
    }
}