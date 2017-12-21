using MyRevit.MyTests.VLBase;

namespace MyRevit.MyTests.Template
{
    /// <summary>
    /// TemplateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TemplateWindow : VLWindow
    {
        public new TemplateViewModel ViewModel { set; get; }

        public TemplateWindow(TemplateViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
        }

        private void Btn_Single_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = (int)TemplateViewType.PickSinglePipe_Pipe;
            ViewModel.Execute();
        }

        private void Btn_Multiple_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ViewModel.IsIdling)
                return;

            ViewModel.ViewType = (int)TemplateViewType.PickMultiplePipes;
            ViewModel.Execute();
        }
    }
}
