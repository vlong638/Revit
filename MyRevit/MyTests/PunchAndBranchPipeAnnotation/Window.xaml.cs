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

        //一键引注
        private void Btn_1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = (int)PBPAViewType.PickSingle_Target;
            ViewModel.Execute();
        }

        //选择引注
        private void Btn_2_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = (int)PBPAViewType.Close;
            ViewModel.Execute();
        }
    }
}
