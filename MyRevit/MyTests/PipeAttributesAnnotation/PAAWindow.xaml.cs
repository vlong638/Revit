using MyRevit.MyTests.VLBase;

namespace MyRevit.MyTests.PAA
{
    /// <summary>
    /// TemplateWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PAAWindow : VLWindow
    {
        public new PAAViewModel ViewModel { set; get; }

        public PAAWindow(PAAViewModel viewModel) : base(viewModel)
        {
            ViewModel = viewModel;
        }

        private void Btn_Single_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Btn_Multiple_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
