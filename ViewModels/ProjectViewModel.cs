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
    /// ViewModel for Project management
    /// Håndterer projekter og projektopfølgning
    /// </summary>
    public class ProjectViewModel : ViewModelBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;

        #region Properties

        private ObservableCollection<Project> _projects;
        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

        private ObservableCollection<Customer> _customers;
        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        private Project _selectedProject;
        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (SetProperty(ref _selectedProject, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsProjectSelected));
                    LoadProjectDetailsAsync();
                }
            }
        }

        private Project _currentProject;
        public Project CurrentProject
        {
            get => _currentProject;
            set => SetProperty(ref _currentProject, value);
        }

        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value) && CurrentProject != null)
                {
                    CurrentProject.CustomerId = value?.CustomerId;
                    CurrentProject.Customer = value;
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
                    FilterProjects();
                }
            }
        }

        private ProjectStatus? _statusFilter;
        public ProjectStatus? StatusFilter
        {
            get => _statusFilter;
            set
            {
                if (SetProperty(ref _statusFilter, value))
                {
                    FilterByStatus();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsProjectSelected => SelectedProject != null;

        private decimal _totalBudget;
        public decimal TotalBudget
        {
            get => _totalBudget;
            set => SetProperty(ref _totalBudget, value);
        }

        private decimal _totalHours;
        public decimal TotalHours
        {
            get => _totalHours;
            set => SetProperty(ref _totalHours, value);
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        #endregion

        #region Commands

        public ICommand LoadProjectsCommand { get; }
        public ICommand AddProjectCommand { get; }
        public ICommand EditProjectCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand DeleteProjectCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewProjectCommand { get; }
        public ICommand ViewProjectDetailsCommand { get; }
        public ICommand CompleteProjectCommand { get; }
        public ICommand ArchiveProjectCommand { get; }

        #endregion

        #region Constructor

        public ProjectViewModel(
            IProjectRepository projectRepository,
            ICustomerRepository customerRepository,
            ITimeEntryRepository timeEntryRepository)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _timeEntryRepository = timeEntryRepository ?? throw new ArgumentNullException(nameof(timeEntryRepository));

            // Initialize collections
            Projects = new ObservableCollection<Project>();
            Customers = new ObservableCollection<Customer>();

            // Initialize commands
            LoadProjectsCommand = new RelayCommand(async _ => await LoadProjectsAsync());
            AddProjectCommand = new RelayCommand(_ => CreateNewProject(), _ => !IsEditMode);
            EditProjectCommand = new RelayCommand(_ => EditProject(), _ => SelectedProject != null);
            SaveProjectCommand = new RelayCommand(async _ => await SaveProjectAsync(), _ => CanSaveProject());
            DeleteProjectCommand = new RelayCommand(async _ => await DeleteProjectAsync(), _ => SelectedProject != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            SearchCommand = new RelayCommand(_ => FilterProjects());
            NewProjectCommand = new RelayCommand(_ => CreateNewProject());
            ViewProjectDetailsCommand = new RelayCommand(_ => ViewProjectDetails(), _ => SelectedProject != null);
            CompleteProjectCommand = new RelayCommand(async _ => await CompleteProjectAsync(), _ => SelectedProject != null && SelectedProject.Status == ProjectStatus.Active);
            ArchiveProjectCommand = new RelayCommand(async _ => await ArchiveProjectAsync(), _ => SelectedProject != null);

            // Load initial data
            InitializeAsync();
        }

        #endregion

        #region Methods

        private async Task InitializeAsync()
        {
            await LoadCustomersAsync();
            await LoadProjectsAsync();
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var projects = await _projectRepository.GetAllAsync();
                Projects.Clear();

                foreach (var project in projects)
                {
                    Projects.Add(project);
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

        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await _customerRepository.GetActiveCustomersAsync();
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
        }

        private async Task LoadProjectDetailsAsync()
        {
            if (SelectedProject == null)
                return;

            try
            {
                // Load time entries for selected project
                var timeEntries = await _timeEntryRepository.GetByProjectIdAsync(SelectedProject.ProjectId);
                TotalHours = timeEntries.Sum(te => te.Hours);
                TotalRevenue = timeEntries.Where(te => te.IsBillable).Sum(te => te.Hours * te.HourlyRate);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void CreateNewProject()
        {
            CurrentProject = new Project
            {
                Status = ProjectStatus.Active,
                IsActive = true,
                StartDate = DateTime.Today,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedProject = null;
        }

        private void EditProject()
        {
            if (SelectedProject != null)
            {
                CurrentProject = new Project
                {
                    ProjectId = SelectedProject.ProjectId,
                    ProjectNumber = SelectedProject.ProjectNumber,
                    Name = SelectedProject.Name,
                    Description = SelectedProject.Description,
                    CustomerId = SelectedProject.CustomerId,
                    StartDate = SelectedProject.StartDate,
                    EndDate = SelectedProject.EndDate,
                    BudgetAmount = SelectedProject.BudgetAmount,
                    Status = SelectedProject.Status,
                    IsActive = SelectedProject.IsActive,
                    CreatedDate = SelectedProject.CreatedDate,
                    ModifiedDate = SelectedProject.ModifiedDate
                };

                SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == CurrentProject.CustomerId);
                IsEditMode = true;
            }
        }

        private async Task SaveProjectAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateProject())
                {
                    return;
                }

                CurrentProject.ModifiedDate = DateTime.Now;

                if (CurrentProject.ProjectId == 0)
                {
                    // Nyt projekt
                    CurrentProject.CreatedDate = DateTime.Now;
                    CurrentProject.ProjectNumber = await GenerateProjectNumberAsync();
                    var id = await _projectRepository.AddAsync(CurrentProject);
                    CurrentProject.ProjectId = id;
                    Projects.Add(CurrentProject);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _projectRepository.UpdateAsync(CurrentProject);
                    if (success)
                    {
                        var existing = Projects.FirstOrDefault(p => p.ProjectId == CurrentProject.ProjectId);
                        if (existing != null)
                        {
                            var index = Projects.IndexOf(existing);
                            Projects[index] = CurrentProject;
                        }
                    }
                }

                IsEditMode = false;
                CurrentProject = null;
                SelectedProject = null;
                SelectedCustomer = null;

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

        private async Task DeleteProjectAsync()
        {
            if (SelectedProject == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // TODO: Check if project has time entries before deleting
                // TODO: Tilføj confirmation dialog

                var success = await _projectRepository.DeleteAsync(SelectedProject.ProjectId);
                if (success)
                {
                    Projects.Remove(SelectedProject);
                    SelectedProject = null;
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
            CurrentProject = null;
            SelectedProject = null;
            SelectedCustomer = null;
            ClearError();
        }

        private async Task CompleteProjectAsync()
        {
            if (SelectedProject == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                SelectedProject.Status = ProjectStatus.Completed;
                SelectedProject.EndDate = DateTime.Today;
                SelectedProject.ModifiedDate = DateTime.Now;

                var success = await _projectRepository.UpdateAsync(SelectedProject);
                if (success)
                {
                    await LoadProjectsAsync();
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

        private async Task ArchiveProjectAsync()
        {
            if (SelectedProject == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                SelectedProject.IsActive = false;
                SelectedProject.ModifiedDate = DateTime.Now;

                var success = await _projectRepository.UpdateAsync(SelectedProject);
                if (success)
                {
                    await LoadProjectsAsync();
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

        private void ViewProjectDetails()
        {
            // TODO: Åbn detaljeret projekt view
            System.Diagnostics.Debug.WriteLine($"View details for project: {SelectedProject?.Name}");
        }

        private void FilterProjects()
        {
            // Simple filtering
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadProjectsAsync();
                return;
            }

            var filtered = Projects.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (p.ProjectNumber?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (p.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            Projects.Clear();
            foreach (var project in filtered)
            {
                Projects.Add(project);
            }

            CalculateTotals();
        }

        private async void FilterByStatus()
        {
            if (StatusFilter == null)
            {
                await LoadProjectsAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var projects = await _projectRepository.GetByStatusAsync(StatusFilter.Value);
                Projects.Clear();

                foreach (var project in projects)
                {
                    Projects.Add(project);
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
            TotalBudget = Projects.Sum(p => p.BudgetAmount);
        }

        private async Task<string> GenerateProjectNumberAsync()
        {
            var maxNumber = 1000;

            if (Projects.Any())
            {
                var numbers = Projects
                    .Select(p => p.ProjectNumber)
                    .Where(n => !string.IsNullOrEmpty(n) && int.TryParse(n.Replace("PRJ", ""), out _))
                    .Select(n => int.Parse(n.Replace("PRJ", "")))
                    .DefaultIfEmpty(1000);

                maxNumber = numbers.Max() + 1;
            }

            return $"PRJ{maxNumber}";
        }

        private bool ValidateProject()
        {
            if (CurrentProject == null)
            {
                SetError("Intet projekt valgt");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentProject.Name))
            {
                SetError("Projektnavn er påkrævet");
                return false;
            }

            if (CurrentProject.BudgetAmount < 0)
            {
                SetError("Budget kan ikke være negativt");
                return false;
            }

            if (CurrentProject.EndDate.HasValue && CurrentProject.EndDate.Value < CurrentProject.StartDate)
            {
                SetError("Slutdato kan ikke være før startdato");
                return false;
            }

            return true;
        }

        private bool CanSaveProject()
        {
            return CurrentProject != null &&
                   !string.IsNullOrWhiteSpace(CurrentProject.Name);
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            Projects?.Clear();
            Customers?.Clear();
            SelectedProject = null;
            CurrentProject = null;
            SelectedCustomer = null;
        }

        #endregion
    }
}