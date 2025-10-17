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
    /// ViewModel for Dimension management
    /// Håndterer dimensioner (projekter, afdelinger, kostcentre osv.)
    /// </summary>
    public class DimensionViewModel : ViewModelBase
    {
        private readonly IDimensionRepository _dimensionRepository;

        #region Properties

        private ObservableCollection<Dimension> _dimensions;
        public ObservableCollection<Dimension> Dimensions
        {
            get => _dimensions;
            set => SetProperty(ref _dimensions, value);
        }

        private Dimension _selectedDimension;
        public Dimension SelectedDimension
        {
            get => _selectedDimension;
            set
            {
                if (SetProperty(ref _selectedDimension, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsDimensionSelected));
                    LoadChildDimensionsAsync();
                }
            }
        }

        private Dimension _currentDimension;
        public Dimension CurrentDimension
        {
            get => _currentDimension;
            set => SetProperty(ref _currentDimension, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterDimensions();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsDimensionSelected => SelectedDimension != null;

        private ObservableCollection<Dimension> _parentDimensions;
        public ObservableCollection<Dimension> ParentDimensions
        {
            get => _parentDimensions;
            set => SetProperty(ref _parentDimensions, value);
        }

        private ObservableCollection<Dimension> _childDimensions;
        public ObservableCollection<Dimension> ChildDimensions
        {
            get => _childDimensions;
            set => SetProperty(ref _childDimensions, value);
        }

        #endregion

        #region Commands

        public ICommand LoadDimensionsCommand { get; }
        public ICommand AddDimensionCommand { get; }
        public ICommand EditDimensionCommand { get; }
        public ICommand SaveDimensionCommand { get; }
        public ICommand DeleteDimensionCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewDimensionCommand { get; }
        public ICommand ViewHierarchyCommand { get; }
        public ICommand AddChildDimensionCommand { get; }

        #endregion

        #region Constructor

        public DimensionViewModel(IDimensionRepository dimensionRepository)
        {
            _dimensionRepository = dimensionRepository ?? throw new ArgumentNullException(nameof(dimensionRepository));

            // Initialize collections
            Dimensions = new ObservableCollection<Dimension>();
            ParentDimensions = new ObservableCollection<Dimension>();
            ChildDimensions = new ObservableCollection<Dimension>();

            // Initialize commands
            LoadDimensionsCommand = new RelayCommand(async _ => await LoadDimensionsAsync());
            AddDimensionCommand = new RelayCommand(_ => CreateNewDimension(), _ => !IsEditMode);
            EditDimensionCommand = new RelayCommand(_ => EditDimension(), _ => SelectedDimension != null);
            SaveDimensionCommand = new RelayCommand(async _ => await SaveDimensionAsync(), _ => CanSaveDimension());
            DeleteDimensionCommand = new RelayCommand(async _ => await DeleteDimensionAsync(), _ => SelectedDimension != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            SearchCommand = new RelayCommand(_ => FilterDimensions());
            NewDimensionCommand = new RelayCommand(_ => CreateNewDimension());
            ViewHierarchyCommand = new RelayCommand(async _ => await LoadDimensionHierarchyAsync());
            AddChildDimensionCommand = new RelayCommand(_ => CreateChildDimension(), _ => SelectedDimension != null);

            // Load initial data
            LoadDimensionsAsync();
        }

        #endregion

        #region Methods

        private async Task LoadDimensionsAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var dimensions = await _dimensionRepository.GetAllAsync();
                Dimensions.Clear();
                ParentDimensions.Clear();

                // Add null option for no parent
                ParentDimensions.Add(new Dimension { DimensionId = 0, DimensionName = "(Ingen overordnet dimension)" });

                foreach (var dimension in dimensions)
                {
                    Dimensions.Add(dimension);
                    ParentDimensions.Add(dimension);
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

        private async Task LoadDimensionHierarchyAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var dimensions = await _dimensionRepository.GetDimensionHierarchyAsync();
                Dimensions.Clear();

                foreach (var dimension in dimensions)
                {
                    Dimensions.Add(dimension);
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

        private async Task LoadChildDimensionsAsync()
        {
            ChildDimensions.Clear();

            if (SelectedDimension == null)
                return;

            try
            {
                var children = await _dimensionRepository.GetChildDimensionsAsync(SelectedDimension.DimensionId);

                foreach (var child in children)
                {
                    ChildDimensions.Add(child);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void CreateNewDimension()
        {
            CurrentDimension = new Dimension
            {
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedDimension = null;
        }

        private void CreateChildDimension()
        {
            if (SelectedDimension == null)
                return;

            CurrentDimension = new Dimension
            {
                ParentDimensionId = SelectedDimension.DimensionId,
                IsActive = true,
                CreatedDate = DateTime.Now
            };

            IsEditMode = true;
        }

        private void EditDimension()
        {
            if (SelectedDimension != null)
            {
                CurrentDimension = new Dimension
                {
                    DimensionId = SelectedDimension.DimensionId,
                    DimensionName = SelectedDimension.DimensionName,
                    DimensionValue = SelectedDimension.DimensionValue,
                    ParentDimensionId = SelectedDimension.ParentDimensionId,
                    IsActive = SelectedDimension.IsActive,
                    CreatedDate = SelectedDimension.CreatedDate
                };
                IsEditMode = true;
            }
        }

        private async Task SaveDimensionAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateDimension())
                {
                    return;
                }

                if (CurrentDimension.DimensionId == 0)
                {
                    // Ny dimension
                    CurrentDimension.CreatedDate = DateTime.Now;
                    var id = await _dimensionRepository.AddAsync(CurrentDimension);
                    CurrentDimension.DimensionId = id;
                    Dimensions.Add(CurrentDimension);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _dimensionRepository.UpdateAsync(CurrentDimension);
                    if (success)
                    {
                        var existing = Dimensions.FirstOrDefault(d => d.DimensionId == CurrentDimension.DimensionId);
                        if (existing != null)
                        {
                            var index = Dimensions.IndexOf(existing);
                            Dimensions[index] = CurrentDimension;
                        }
                    }
                }

                IsEditMode = false;
                CurrentDimension = null;
                SelectedDimension = null;

                // Reload to update parent dimensions
                await LoadDimensionsAsync();
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

        private async Task DeleteDimensionAsync()
        {
            if (SelectedDimension == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // Check if dimension has children
                var children = await _dimensionRepository.GetChildDimensionsAsync(SelectedDimension.DimensionId);
                if (children.Any())
                {
                    SetError("Kan ikke slette dimension med underdimensioner");
                    return;
                }

                // TODO: Check if dimension is used in transactions
                // TODO: Tilføj confirmation dialog

                var success = await _dimensionRepository.DeleteAsync(SelectedDimension.DimensionId);
                if (success)
                {
                    Dimensions.Remove(SelectedDimension);
                    SelectedDimension = null;
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
            CurrentDimension = null;
            SelectedDimension = null;
            ClearError();
        }

        private void FilterDimensions()
        {
            // Simple filtering
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadDimensionsAsync();
                return;
            }

            var filtered = Dimensions.Where(d =>
                d.DimensionName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (d.DimensionValue?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            Dimensions.Clear();
            foreach (var dimension in filtered)
            {
                Dimensions.Add(dimension);
            }
        }

        private bool ValidateDimension()
        {
            if (CurrentDimension == null)
            {
                SetError("Ingen dimension valgt");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentDimension.DimensionName))
            {
                SetError("Dimensionsnavn er påkrævet");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentDimension.DimensionValue))
            {
                SetError("Dimensionsværdi er påkrævet");
                return false;
            }

            // Check for circular reference in parent dimension
            if (CurrentDimension.ParentDimensionId.HasValue &&
                CurrentDimension.ParentDimensionId.Value == CurrentDimension.DimensionId)
            {
                SetError("En dimension kan ikke være sin egen overordnede dimension");
                return false;
            }

            return true;
        }

        private bool CanSaveDimension()
        {
            return CurrentDimension != null &&
                   !string.IsNullOrWhiteSpace(CurrentDimension.DimensionName) &&
                   !string.IsNullOrWhiteSpace(CurrentDimension.DimensionValue);
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            Dimensions?.Clear();
            ParentDimensions?.Clear();
            ChildDimensions?.Clear();
            SelectedDimension = null;
            CurrentDimension = null;
        }

        #endregion
    }
}