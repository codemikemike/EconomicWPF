using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class DimensionView : UserControl
    {
        public DimensionView()
        {
            InitializeComponent();
        }

        public DimensionView(DimensionViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}