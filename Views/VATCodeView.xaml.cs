using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class VATCodeView : UserControl
    {
        public VATCodeView()
        {
            InitializeComponent();
        }

        public VATCodeView(VATCodeViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}