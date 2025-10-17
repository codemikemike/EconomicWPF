using System.Windows.Controls;
using EconomicWPF.ViewModels;

namespace EconomicWPF.Views
{
    public partial class PaymentView : UserControl
    {
        public PaymentView()
        {
            InitializeComponent();
        }

        public PaymentView(PaymentViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}