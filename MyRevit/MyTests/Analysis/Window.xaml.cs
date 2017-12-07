using MyRevit.MyTests.VLBase;

namespace MyRevit.MyTests.Analysis
{
    /// <summary>
    /// AnalysisWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AnalysisWindow : VLWindow
    {
        public new AnalysisViewModel ViewModel { set; get; }

        public AnalysisWindow(AnalysisViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
        }

        private void Btn_1_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = AnalysisViewType.DisplayGeometry;
            ViewModel.Execute();
        }
    }
}
