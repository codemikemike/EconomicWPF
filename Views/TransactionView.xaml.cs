using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class TransactionView : UserControl
    {
        public TransactionView()
        {
            InitializeComponent();
        }

        public TransactionView(TransactionViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}