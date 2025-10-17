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
    /// ViewModel for TimeEntry management
    /// Håndterer tidsregistrering på projekter
    /// </summary>
    public class TimeEntryViewModel : ViewModelBase
    {
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IProjectRepository _projectRepository;

        #region Properties

        private ObservableCollection<TimeEntry> _timeEntries;
        public ObservableCollection<TimeEntry> TimeEntries
        {
            get => _timeEntries;
            set => SetProperty(ref _timeEntries, value);
        }

        private ObservableCollection<Project> _projects;
        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

        private TimeEntry _selectedTimeEntry;
        public TimeEntry SelectedTimeEntry
        {
            get => _selectedTimeEntry;
            set
            {
                if (SetProperty(ref _selectedTimeEntry, value))
                {
                    IsEditMode = value != null;
                    OnPropertyChanged(nameof(IsTimeEntrySelected));
                }
            }
        }

        private TimeEntry _currentTimeEntry;
        public TimeEntry CurrentTimeEntry
        {
            get => _currentTimeEntry;
            set => SetProperty(ref _currentTimeEntry, value);
        }

        private Project _selectedProject;
        public Project SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (SetProperty(ref _selectedProject, value) && CurrentTimeEntry != null)
                {
                    CurrentTimeEntry.ProjectId = value?.ProjectId ?? 0;
                    CurrentTimeEntry.Project = value;
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

        private bool _showBillableOnly;
        public bool ShowBillableOnly
        {
            get => _showBillableOnly;
            set
            {
                if (SetProperty(ref _showBillableOnly, value))
                {
                    FilterTimeEntries();
                }
            }
        }

        private bool _showUnbilledOnly;
        public bool ShowUnbilledOnly
        {
            get => _showUnbilledOnly;
            set
            {
                if (SetProperty(ref _showUnbilledOnly, value))
                {
                    FilterTimeEntries();
                }
            }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public bool IsTimeEntrySelected => SelectedTimeEntry != null;

        private decimal _totalHours;
        public decimal TotalHours
        {
            get => _totalHours;
            set => SetProperty(ref _totalHours, value);
        }

        private decimal _totalBillableHours;
        public decimal TotalBillableHours
        {
            get => _totalBillableHours;
            set => SetProperty(ref _totalBillableHours, value);
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        private int _employeeId;
        public int EmployeeId
        {
            get => _employeeId;
            set => SetProperty(ref _employeeId, value);
        }

        #endregion

        #region Commands

        public ICommand LoadTimeEntriesCommand { get; }
        public ICommand AddTimeEntryCommand { get; }
        public ICommand EditTimeEntryCommand { get; }
        public ICommand SaveTimeEntryCommand { get; }
        public ICommand DeleteTimeEntryCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand NewTimeEntryCommand { get; }
        public ICommand MarkAsBilledCommand { get; }
        public ICommand ViewTimesheetCommand { get; }
        public ICommand ExportTimesheetCommand { get; }

        #endregion

        #region Constructor

        public TimeEntryViewModel(
            ITimeEntryRepository timeEntryRepository,
            IProjectRepository projectRepository)
        {
            _timeEntryRepository = timeEntryRepository ?? throw new ArgumentNullException(nameof(timeEntryRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));

            // Initialize collections
            TimeEntries = new ObservableCollection<TimeEntry>();
            Projects = new ObservableCollection<Project>();

            // TODO: Get current employee ID from authentication/session
            EmployeeId = 1; // Placeholder

            // Initialize commands
            LoadTimeEntriesCommand = new RelayCommand(async _ => await LoadTimeEntriesAsync());
            AddTimeEntryCommand = new RelayCommand(_ => CreateNewTimeEntry(), _ => !IsEditMode);
            EditTimeEntryCommand = new RelayCommand(_ => EditTimeEntry(), _ => SelectedTimeEntry != null);
            SaveTimeEntryCommand = new RelayCommand(async _ => await SaveTimeEntryAsync(), _ => CanSaveTimeEntry());
            DeleteTimeEntryCommand = new RelayCommand(async _ => await DeleteTimeEntryAsync(), _ => SelectedTimeEntry != null);
            CancelCommand = new RelayCommand(_ => CancelEdit());
            NewTimeEntryCommand = new RelayCommand(_ => CreateNewTimeEntry());
            MarkAsBilledCommand = new RelayCommand(async _ => await MarkAsBilledAsync(), _ => SelectedTimeEntry != null && SelectedTimeEntry.IsBillable && !SelectedTimeEntry.IsInvoiced);
            ViewTimesheetCommand = new RelayCommand(async _ => await ViewTimesheetAsync());
            ExportTimesheetCommand = new RelayCommand(_ => ExportTimesheet());

            // Load initial data
            InitializeAsync();
        }

        #endregion

        #region Methods

        private async Task InitializeAsync()
        {
            await LoadProjectsAsync();
            await LoadTimeEntriesAsync();
        }

        private async Task LoadTimeEntriesAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                var timeEntries = await _timeEntryRepository.GetAllAsync();
                TimeEntries.Clear();

                foreach (var entry in timeEntries)
                {
                    TimeEntries.Add(entry);
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

        private async Task LoadProjectsAsync()
        {
            try
            {
                var projects = await _projectRepository.GetActiveProjectsAsync();
                Projects.Clear();

                foreach (var project in projects)
                {
                    Projects.Add(project);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void CreateNewTimeEntry()
        {
            CurrentTimeEntry = new TimeEntry
            {
                EmployeeId = EmployeeId,
                Date = DateTime.Today,
                Hours = 0,
                IsBillable = true,
                IsInvoiced = false,
                CreatedDate = DateTime.Now
            };

            IsEditMode = true;
            SelectedTimeEntry = null;
        }

        private void EditTimeEntry()
        {
            if (SelectedTimeEntry != null)
            {
                CurrentTimeEntry = new TimeEntry
                {
                    TimeEntryId = SelectedTimeEntry.TimeEntryId,
                    ProjectId = SelectedTimeEntry.ProjectId,
                    EmployeeId = SelectedTimeEntry.EmployeeId,
                    Date = SelectedTimeEntry.Date,
                    Hours = SelectedTimeEntry.Hours,
                    Description = SelectedTimeEntry.Description,
                    HourlyRate = SelectedTimeEntry.HourlyRate,
                    IsBillable = SelectedTimeEntry.IsBillable,
                    IsInvoiced = SelectedTimeEntry.IsInvoiced,
                    CreatedDate = SelectedTimeEntry.CreatedDate
                };

                SelectedProject = Projects.FirstOrDefault(p => p.ProjectId == CurrentTimeEntry.ProjectId);
                IsEditMode = true;
            }
        }

        private async Task SaveTimeEntryAsync()
        {
            try
            {
                IsLoading = true;
                ClearError();

                if (!ValidateTimeEntry())
                {
                    return;
                }

                if (CurrentTimeEntry.TimeEntryId == 0)
                {
                    // Ny tidsregistrering
                    CurrentTimeEntry.CreatedDate = DateTime.Now;
                    var id = await _timeEntryRepository.AddAsync(CurrentTimeEntry);
                    CurrentTimeEntry.TimeEntryId = id;
                    TimeEntries.Add(CurrentTimeEntry);
                }
                else
                {
                    // Opdater eksisterende
                    var success = await _timeEntryRepository.UpdateAsync(CurrentTimeEntry);
                    if (success)
                    {
                        var existing = TimeEntries.FirstOrDefault(te => te.TimeEntryId == CurrentTimeEntry.TimeEntryId);
                        if (existing != null)
                        {
                            var index = TimeEntries.IndexOf(existing);
                            TimeEntries[index] = CurrentTimeEntry;
                        }
                    }
                }

                IsEditMode = false;
                CurrentTimeEntry = null;
                SelectedTimeEntry = null;
                SelectedProject = null;

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

        private async Task DeleteTimeEntryAsync()
        {
            if (SelectedTimeEntry == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                // Check if already invoiced
                if (SelectedTimeEntry.IsInvoiced)
                {
                    SetError("Kan ikke slette tidsregistrering der allerede er faktureret");
                    return;
                }

                // TODO: Tilføj confirmation dialog

                var success = await _timeEntryRepository.DeleteAsync(SelectedTimeEntry.TimeEntryId);
                if (success)
                {
                    TimeEntries.Remove(SelectedTimeEntry);
                    SelectedTimeEntry = null;
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
            CurrentTimeEntry = null;
            SelectedTimeEntry = null;
            SelectedProject = null;
            ClearError();
        }

        private async Task MarkAsBilledAsync()
        {
            if (SelectedTimeEntry == null)
                return;

            try
            {
                IsLoading = true;
                ClearError();

                SelectedTimeEntry.IsInvoiced = true;
                var success = await _timeEntryRepository.UpdateAsync(SelectedTimeEntry);

                if (success)
                {
                    await LoadTimeEntriesAsync();
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

        private async Task ViewTimesheetAsync()
        {
            // Load current week's timesheet
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
            var endOfWeek = startOfWeek.AddDays(6);

            DateFrom = startOfWeek;
            DateTo = endOfWeek;

            await FilterByDateRange();
        }

        private void ExportTimesheet()
        {
            // TODO: Implementer eksport til Excel/CSV
            System.Diagnostics.Debug.WriteLine("Export timesheet");
        }

        private void FilterTimeEntries()
        {
            var filtered = TimeEntries.AsEnumerable();

            if (ShowBillableOnly)
            {
                filtered = filtered.Where(te => te.IsBillable);
            }

            if (ShowUnbilledOnly)
            {
                filtered = filtered.Where(te => !te.IsInvoiced);
            }

            var result = filtered.ToList();
            TimeEntries.Clear();
            foreach (var entry in result)
            {
                TimeEntries.Add(entry);
            }

            CalculateTotals();
        }

        private async Task FilterByDateRange()
        {
            if (!DateFrom.HasValue && !DateTo.HasValue)
            {
                await LoadTimeEntriesAsync();
                return;
            }

            try
            {
                IsLoading = true;
                ClearError();

                var fromDate = DateFrom ?? DateTime.MinValue;
                var toDate = DateTo ?? DateTime.MaxValue;

                var timeEntries = await _timeEntryRepository.GetByDateRangeAsync(fromDate, toDate);
                TimeEntries.Clear();

                foreach (var entry in timeEntries)
                {
                    TimeEntries.Add(entry);
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
            TotalHours = TimeEntries.Sum(te => te.Hours);
            TotalBillableHours = TimeEntries.Where(te => te.IsBillable).Sum(te => te.Hours);
            TotalRevenue = TimeEntries.Where(te => te.IsBillable).Sum(te => te.Hours * te.HourlyRate);
        }

        private bool ValidateTimeEntry()
        {
            if (CurrentTimeEntry == null)
            {
                SetError("Ingen tidsregistrering valgt");
                return false;
            }

            if (CurrentTimeEntry.ProjectId == 0)
            {
                SetError("Vælg et projekt");
                return false;
            }

            if (CurrentTimeEntry.Hours <= 0)
            {
                SetError("Timer skal være større end 0");
                return false;
            }

            if (CurrentTimeEntry.Hours > 24)
            {
                SetError("Timer kan ikke overstige 24 på én dag");
                return false;
            }

            if (CurrentTimeEntry.Date > DateTime.Today)
            {
                SetError("Dato kan ikke være i fremtiden");
                return false;
            }

            if (CurrentTimeEntry.IsBillable && CurrentTimeEntry.HourlyRate <= 0)
            {
                SetError("Timesats er påkrævet for fakturerbare timer");
                return false;
            }

            if (string.IsNullOrWhiteSpace(CurrentTimeEntry.Description))
            {
                SetError("Beskrivelse er påkrævet");
                return false;
            }

            return true;
        }

        private bool CanSaveTimeEntry()
        {
            return CurrentTimeEntry != null &&
                   CurrentTimeEntry.ProjectId > 0 &&
                   CurrentTimeEntry.Hours > 0 &&
                   !string.IsNullOrWhiteSpace(CurrentTimeEntry.Description);
        }

        #endregion

        #region Cleanup

        public override void Cleanup()
        {
            base.Cleanup();
            TimeEntries?.Clear();
            Projects?.Clear();
            SelectedTimeEntry = null;
            CurrentTimeEntry = null;
            SelectedProject = null;
        }

        #endregion
    }
}