using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class AccountView : UserControl
    {
        public AccountView()
        {
            InitializeComponent();
            // DataContext sættes fra MainWindow eller via DI
        }

        public AccountView(AccountViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}