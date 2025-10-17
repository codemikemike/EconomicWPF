using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class ProductView : UserControl
    {
        public ProductView()
        {
            InitializeComponent();
        }

        public ProductView(ProductViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}