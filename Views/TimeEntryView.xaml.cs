using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class TimeEntryView : UserControl
    {
        public TimeEntryView()
        {
            InitializeComponent();
        }

        public TimeEntryView(TimeEntryViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}