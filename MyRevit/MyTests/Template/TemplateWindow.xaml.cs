using MyRevit.MyTests.VLBase;

namespace MyRevit.MyTests.Template
{
    /// <summary>
    /// PAAWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TemplateWindow : VLWindow
    {
        public new TemplateViewModel ViewModel { set; get; }

        public TemplateWindow(TemplateViewModel viewModel) : base(viewModel)
        {
            ViewModel = viewModel;
        }
    }
}
