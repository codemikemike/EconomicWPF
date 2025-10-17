using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class BudgetView : UserControl
    {
        public BudgetView()
        {
            InitializeComponent();
        }

        public BudgetView(BudgetViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}