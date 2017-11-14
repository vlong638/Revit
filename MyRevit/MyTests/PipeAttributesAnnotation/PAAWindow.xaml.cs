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
    }
}
