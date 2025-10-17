using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class SupplierView : UserControl
    {
        public SupplierView()
        {
            InitializeComponent();
        }

        public SupplierView(SupplierViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}