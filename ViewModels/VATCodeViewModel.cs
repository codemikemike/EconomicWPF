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
    /// ViewModel for VATCode management
    /// Håndterer momskoder og momssatser
    /// </summary>
    public class VATCodeViewModel : ViewModelBase
    {
        private readonly IVATCodeRepository _vatCodeRepository;

        #region Properties

        private ObservableCollection<VATCode> _vatCodes;
        public ObservableCollection<VATCode> VATCodes
        {
            get => _vatCodes;
            set => SetProperty(ref _vatCodes, value);
        }

        private VATCode _selectedVATCode;
        public VATCode SelectedVATCode
        {
            get => _selectedVATCode;
            set
            {
                if (SetProperty(ref _selectedVATCode, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsVATCodeSelected));
                }
            }
        }

        private VATCode _currentVATCode;
        public VATCode CurrentVATCode
        {
            get => _currentVATCode;
            set => SetProperty(ref _currentVATCode, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterVATCodes();
                }
            }
        }

        private bool _showActiveOnly;
        public bool ShowActiveOnly
        {
            get => _showActiveOnly;
            set
            {
                if (SetProperty(ref _showActiveOnly, value))
                {
                    FilterVATCodes();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsVATCodeSelected => SelectedVATCode != null;

        #endregion

        #region Commands

        public ICommand LoadVATCodesCommand { get; }
        public ICommand AddVATCodeCommand { get; }
        public ICommand EditVATCodeCommand { get; }
        public ICommand SaveVATCodeCommand { get; }
        public ICommand DeleteVATCodeCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewVATCodeCommand { get; }
        public ICommand ActivateVATCodeCommand { get; }
        public ICommand DeactivateVATCodeCommand { get; }

        #endregion

        #region Constructor

        public VATCodeViewModel(IVATCodeRepository vatCodeRepository)
        {
            _vatCodeRepository = vatCodeRepository ?? throw new ArgumentNullException(nameof(vatCodeRepository));

            // Initialize collections
            VATCodes = new ObservableCollection<VATCode>();

            // Initialize commands
            LoadVATCodesCommand = new RelayCommand(async _ => await LoadVATCodesAsync());
            AddVATCodeCommand = new RelayCommand(_ => CreateNewVATCode(), _ => !IsEditMode);
            EditVATCodeCommand = new RelayCommand(_ => EditVATCode(), _ => SelectedVATCode != null);
            SaveVATCodeCommand = new RelayCommand(async _ => await SaveVATCodeAsync(), _ => CanSaveVATCode());
            DeleteVATCodeCommand = new RelayCommand(async _ => await DeleteVATCodeAsync(), _ => SelectedVATCode != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            SearchCommand = new RelayCommand(_ => FilterVATCodes());
            NewVATCodeCommand = new RelayCommand(_ => CreateNewVATCode());
            ActivateVATCodeCommand = new RelayCommand(async _ => await ActivateVATCodeAsync(), _ => SelectedVATCode != null && !SelectedVATCode.IsActive);
            DeactivateVATCodeCommand = new RelayCommand(async _ => await DeactivateVATCodeAsync(), _ => SelectedVATCode != null && SelectedVATCode.IsActive);

            // Load initial data
            LoadVATCodesAsync();
        }

        #endregion

        #region Methods

        private async Task LoadVATCodesAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var vatCodes = await _vatCodeRepository.GetAllAsync();
                VATCodes.Clear();

                foreach (var vatCode in vatCodes)
                {
                    VATCodes.Add(vatCode);
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

        private void CreateNewVATCode()
        {
            CurrentVATCode = new VATCode
            {
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedVATCode = null;
        }

        private void EditVATCode()
        {
            if (SelectedVATCode != null)
            {
                CurrentVATCode = new VATCode
                {
                    VATCodeId = SelectedVATCode.VATCodeId,
                    Code = SelectedVATCode.Code,
                    Description = SelectedVATCode.Description,
                    Rate = SelectedVATCode.Rate,
                    IsActive = SelectedVATCode.IsActive,
                    CreatedDate = SelectedVATCode.CreatedDate
                };
                IsEditMode = true;
            }
        }

        private async Task SaveVATCodeAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateVATCode())
                {
                    return;
                }

                if (CurrentVATCode.VATCodeId == 0)
                {
                    // Ny momskode
                    CurrentVATCode.CreatedDate = DateTime.Now;
                    var id = await _vatCodeRepository.AddAsync(CurrentVATCode);
                    CurrentVATCode.VATCodeId = id;
                    VATCodes.Add(CurrentVATCode);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _vatCodeRepository.UpdateAsync(CurrentVATCode);
                    if (success)
                    {
                        var existing = VATCodes.FirstOrDefault(v => v.VATCodeId == CurrentVATCode.VATCodeId);
                        if (existing != null)
                        {
                            var index = VATCodes.IndexOf(existing);
                            VATCodes[index] = CurrentVATCode;
                        }
                    }
                }

                IsEditMode = false;
                CurrentVATCode = null;
                SelectedVATCode = null;
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

        private async Task DeleteVATCodeAsync()
        {
            if (SelectedVATCode == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Check if VAT code is used in products/invoices before deleting
                // TODO: Tilføj confirmation dialog

                var success = await _vatCodeRepository.DeleteAsync(SelectedVATCode.VATCodeId);
                if (success)
                {
                    VATCodes.Remove(SelectedVATCode);
                    SelectedVATCode = null;
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
            CurrentVATCode = null;
            SelectedVATCode = null;
            ClearError();
        }

        private async Task ActivateVATCodeAsync()
        {
            if (SelectedVATCode == null || SelectedVATCode.IsActive)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                SelectedVATCode.IsActive = true;
                var success = await _vatCodeRepository.UpdateAsync(SelectedVATCode);

                if (success)
                {
                    await LoadVATCodesAsync();
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

        private async Task DeactivateVATCodeAsync()
        {
            if (SelectedVATCode == null || !SelectedVATCode.IsActive)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                SelectedVATCode.IsActive = false;
                var success = await _vatCodeRepository.UpdateAsync(SelectedVATCode);

                if (success)
                {
                    await LoadVATCodesAsync();
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

        private void FilterVATCodes()
        {
            var filtered = VATCodes.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(v =>
                    v.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    v.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (ShowActiveOnly)
            {
                filtered = filtered.Where(v => v.IsActive);
            }

            var result = filtered.ToList();
            VATCodes.Clear();
            foreach (var vatCode in result)
            {
                VATCodes.Add(vatCode);
            }
        }

        private bool ValidateVATCode()
        {
            if (CurrentVATCode == null)
            {
                SetError("Ingen momskode valgt");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentVATCode.Code))
            {
                SetError("Kode er påkrævet");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentVATCode.Description))
            {
                SetError("Beskrivelse er påkrævet");
                return false;
            }

            if (CurrentVATCode.Rate < 0 || CurrentVATCode.Rate > 100)
            {
                SetError("Momssats skal være mellem 0 og 100");
                return false;
            }

            // Check for duplicate code
            if (VATCodes.Any(v => v.Code.Equals(CurrentVATCode.Code, StringComparison.OrdinalIgnoreCase)
                && v.VATCodeId != CurrentVATCode.VATCodeId))
            {
                SetError("En momskode med denne kode eksisterer allerede");
                return false;
            }

            return true;
        }

        private bool CanSaveVATCode()
        {
            return CurrentVATCode != null &&
                   !string.IsNullOrWhiteSpace(CurrentVATCode.Code) &&
                   !string.IsNullOrWhiteSpace(CurrentVATCode.Description) &&
                   CurrentVATCode.Rate >= 0 &&
                   CurrentVATCode.Rate <= 100;
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            VATCodes?.Clear();
            SelectedVATCode = null;
            CurrentVATCode = null;
        }

        #endregion
    }
}