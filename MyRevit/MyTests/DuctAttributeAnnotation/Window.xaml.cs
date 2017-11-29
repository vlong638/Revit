using MyRevit.MyTests.VLBase;

namespace MyRevit.MyTests.DAA
{
    /// <summary>
    /// DAAWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DAAWindow : VLWindow
    {
        public new DAAViewModel ViewModel { set; get; }

        public DAAWindow(DAAViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
        }

        private void Btn_Single_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = DAAViewType.PickSinglePipe_Pipe;
            ViewModel.Execute();
        }

        private void Btn_Cancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.ViewType = DAAViewType.Close;
            ViewModel.Execute();
        }
    }
}
