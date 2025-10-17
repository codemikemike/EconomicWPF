using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class CustomerView : UserControl
    {
        public CustomerView()
        {
            InitializeComponent();
        }

        public CustomerView(CustomerViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}