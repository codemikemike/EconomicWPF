using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class InvoiceView : UserControl
    {
        public InvoiceView()
        {
            InitializeComponent();
        }

        public InvoiceView(InvoiceViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}