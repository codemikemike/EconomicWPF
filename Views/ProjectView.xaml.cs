using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();
        }

        public ProjectView(ProjectViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}