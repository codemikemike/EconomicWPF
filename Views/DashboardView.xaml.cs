using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        public DashboardView(DashboardViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}