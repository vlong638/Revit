using MyRevit.MyTests.VLBase;

namespace MyRevit.MyTests.PBPA
{
    /// <summary>
    /// PBPAWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PBPAWindow : VLWindow
    {
        public new PBPAViewModel ViewModel { set; get; }

        public PBPAWindow(PBPAViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
        }

        private void Btn_Single_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = PBPAViewType.PickSinglePipe_Pipe;
            ViewModel.Execute();
        }

        private void Btn_Multiple_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = PBPAViewType.PickMultiplePipes;
            ViewModel.Execute();
        }
    }
}
