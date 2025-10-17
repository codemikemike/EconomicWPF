using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class BankTransactionView : UserControl
    {
        public BankTransactionView()
        {
            InitializeComponent();
        }

        public BankTransactionView(BankTransactionViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}