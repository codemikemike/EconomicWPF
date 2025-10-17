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
    /// ViewModel for Product management
    /// Håndterer vare oprettelse, redigering og lagerstyring
    /// </summary>
    public class ProductViewModel : ViewModelBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IAccountRepository _accountRepository;

        #region Properties

        private ObservableCollection<Product> _products;
        public ObservableCollection<Product> Products
        {
            get => _products;
            set => SetProperty(ref _products, value);
        }

        private ObservableCollection<Account> _accounts;
        public ObservableCollection<Account> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsProductSelected));
                }
            }
        }

        private Product _currentProduct;
        public Product CurrentProduct
        {
            get => _currentProduct;
            set => SetProperty(ref _currentProduct, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SearchProductsAsync();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsProductSelected => SelectedProduct != null;

        private ObservableCollection<Product> _lowStockProducts;
        public ObservableCollection<Product> LowStockProducts
        {
            get => _lowStockProducts;
            set => SetProperty(ref _lowStockProducts, value);
        }

        #endregion

        #region Commands

        public ICommand LoadProductsCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand SaveProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewProductCommand { get; }
        public ICommand UpdateStockCommand { get; }
        public ICommand ViewLowStockCommand { get; }

        #endregion

        #region Constructor

        public ProductViewModel(IProductRepository productRepository, IAccountRepository accountRepository)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));

            // Initialize collections
            Products = new ObservableCollection<Product>();
            Accounts = new ObservableCollection<Account>();
            LowStockProducts = new ObservableCollection<Product>();

            // Initialize commands
            LoadProductsCommand = new RelayCommand(async _ => await LoadProductsAsync());
            AddProductCommand = new RelayCommand(_ => CreateNewProduct(), _ => !IsEditMode);
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => SelectedProduct != null);
            SaveProductCommand = new RelayCommand(async _ => await SaveProductAsync(), _ => CanSaveProduct());
            DeleteProductCommand = new RelayCommand(async _ => await DeleteProductAsync(), _ => SelectedProduct != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            SearchCommand = new RelayCommand(async _ => await SearchProductsAsync());
            NewProductCommand = new RelayCommand(_ => CreateNewProduct());
            UpdateStockCommand = new RelayCommand(_ => UpdateStock(), _ => SelectedProduct != null);
            ViewLowStockCommand = new RelayCommand(async _ => await LoadLowStockProductsAsync());

            // Load initial data
            InitializeAsync();
        }

        #endregion

        #region Methods

        private async Task InitializeAsync()
        {
            await LoadAccountsAsync();
            await LoadProductsAsync();
            await LoadLowStockProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var products = await _productRepository.GetAllAsync();
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
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAccountsAsync()
        {
            try
            {
                var accounts = await _accountRepository.GetByTypeAsync(AccountType.Revenue); // ✅ Bruger enum
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
        }

        private async Task LoadLowStockProductsAsync()
        {
            try
            {
                var lowStock = await _productRepository.GetLowStockProductsAsync();
                LowStockProducts.Clear();

                foreach (var product in lowStock)
                {
                    LowStockProducts.Add(product);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void CreateNewProduct()
        {
            CurrentProduct = new Product
            {
                ProductNumber = GenerateProductNumber(),
                Unit = "stk",
                VATRate = 25.00m,
                IsActive = true,
                AccountId = Accounts.FirstOrDefault()?.AccountId ?? 0,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedProduct = null;
        }

        private void EditProduct()
        {
            if (SelectedProduct != null)
            {
                // ✅ Manuel mapping i stedet for copy constructor
                CurrentProduct = new Product
                {
                    ProductId = SelectedProduct.ProductId,
                    ProductNumber = SelectedProduct.ProductNumber,
                    Name = SelectedProduct.Name,
                    Description = SelectedProduct.Description,
                    PurchasePrice = SelectedProduct.PurchasePrice,
                    SalesPrice = SelectedProduct.SalesPrice,
                    VATRate = SelectedProduct.VATRate,
                    Unit = SelectedProduct.Unit,
                    StockQuantity = SelectedProduct.StockQuantity,
                    ReorderLevel = SelectedProduct.ReorderLevel,
                    AccountId = SelectedProduct.AccountId,
                    IsActive = SelectedProduct.IsActive,
                    CreatedDate = SelectedProduct.CreatedDate,
                    ModifiedDate = SelectedProduct.ModifiedDate
                };
                IsEditMode = true;
            }
        }

        private async Task SaveProductAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateProduct())
                {
                    return;
                }

                // Opdater ModifiedDate
                CurrentProduct.ModifiedDate = DateTime.Now;

                if (CurrentProduct.ProductId == 0)
                {
                    // Ny vare
                    CurrentProduct.CreatedDate = DateTime.Now;
                    var id = await _productRepository.AddAsync(CurrentProduct);
                    CurrentProduct.ProductId = id;
                    Products.Add(CurrentProduct);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _productRepository.UpdateAsync(CurrentProduct);
                    if (success)
                    {
                        var existing = Products.FirstOrDefault(p => p.ProductId == CurrentProduct.ProductId);
                        if (existing != null)
                        {
                            var index = Products.IndexOf(existing);
                            Products[index] = CurrentProduct;
                        }
                    }
                }

                IsEditMode = false;
                CurrentProduct = null;
                SelectedProduct = null;

                // Reload low stock products
                await LoadLowStockProductsAsync();
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

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Tilføj confirmation dialog

                var success = await _productRepository.DeleteAsync(SelectedProduct.ProductId);
                if (success)
                {
                    Products.Remove(SelectedProduct);
                    SelectedProduct = null;
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
            CurrentProduct = null;
            SelectedProduct = null;
            ClearError();
        }

        private async Task SearchProductsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadProductsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var results = await _productRepository.SearchByNameAsync(SearchText);
                Products.Clear();

                foreach (var product in results)
                {
                    Products.Add(product);
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

        private void UpdateStock()
        {
            if (SelectedProduct == null)
                return;

            // TODO: Vis dialog for lageropdate
            // Dette kræver en separat dialog/window
        }

        private bool ValidateProduct()
        {
            if (CurrentProduct == null)
            {
                SetError("Ingen vare valgt");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentProduct.Name))
            {
                SetError("Varenavn er påkrævet");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentProduct.ProductNumber))
            {
                SetError("Varenummer er påkrævet");
                return false;
            }

            if (CurrentProduct.SalesPrice < 0)
            {
                SetError("Salgspris kan ikke være negativ");
                return false;
            }

            if (CurrentProduct.PurchasePrice < 0)
            {
                SetError("Indkøbspris kan ikke være negativ");
                return false;
            }

            if (CurrentProduct.VATRate < 0 || CurrentProduct.VATRate > 100)
            {
                SetError("Momssats skal være mellem 0 og 100");
                return false;
            }

            if (CurrentProduct.AccountId == 0)
            {
                SetError("Vælg en konto");
                return false;
            }

            return true;
        }

        private bool CanSaveProduct()
        {
            return CurrentProduct != null &&
                   !string.IsNullOrWhiteSpace(CurrentProduct.Name) &&
                   !string.IsNullOrWhiteSpace(CurrentProduct.ProductNumber) &&
                   CurrentProduct.AccountId > 0;
        }

        private string GenerateProductNumber()
        {
            var maxNumber = 1000;

            if (Products.Any())
            {
                var numbers = Products
                    .Select(p => p.ProductNumber)
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
            Products?.Clear();
            Accounts?.Clear();
            LowStockProducts?.Clear();
        }

        #endregion
    }
}