using PmSoft.Common.RevitClass.VLUtils;

namespace PmSoft.MepProject.MepWork.FullFunctions.MEPCurveAutomaticTurn
{
    /// <summary>
    /// MATWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MATWindow : VLWindow
    {
        public new MATViewModel ViewModel { set; get; }

        public MATWindow(MATViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
        }

        private void Btn_Single_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = (int)MATViewType.PickSinglePipe_Pipe;
            ViewModel.Execute();
        }

        private void Btn_Multiple_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = (int)MATViewType.PickMultiplePipes;
            ViewModel.Execute();
        }
    }
}
