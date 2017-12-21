using MyRevit.MyTests.VLBase;

namespace MyRevit.MyTests.PAA
{
    public partial class PAAWindow : VLWindow
    {
        public new PAAViewModel ViewModel { set; get; }

        public PAAWindow(PAAViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
        }

        private void Btn_Single_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.UpdateViewType(1);
            ViewModel.Execute();
        }

        private void Btn_Multiple_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.UpdateViewType(2);
            ViewModel.Execute();
        }

        private void Btn_3_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.UpdateViewType(3);
            ViewModel.Execute();
        }
    }
}
