using PmSoft.Common.RevitClass.VLUtils;

namespace PMSoft.ConstructionManagementV2
{
    /// <summary>
    /// CMWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CMWindow : VLWindow
    {
        public new CMViewModel ViewModel { set; get; }

        public CMWindow(CMViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
        }

        private void Btn_Single_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = (int)CMViewType.PickSinglePipe_Pipe;
            ViewModel.Execute();
        }

        private void Btn_Multiple_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = (int)CMViewType.PickMultiplePipes;
            ViewModel.Execute();
        }
    }
}
