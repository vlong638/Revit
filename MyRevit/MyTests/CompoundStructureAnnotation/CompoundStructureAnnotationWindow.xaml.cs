using PmSoft.Optimization.DrawingProduction;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using MyRevit.MyTests.VLBase;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{

    /// <summary>
    /// CompoundStructureAnnotationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CompoundStructureAnnotationWindow : VLWindow
    {
        //不写入基类是由于XAML泛型基类的替换困难...
        public new CSAViewModel ViewModel { set; get; }

        public CompoundStructureAnnotationWindow(CSAViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.ViewType = (int)CSAViewType.Close;
            DialogResult = true;
        }

        private void CommandBinding_Executed_New(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.TextNoteTypeElementId == null)
            {
                MessageBox.Show("请先选择文字样式");
                return;
            }
            ViewModel.ViewType = (int)CSAViewType.Select;
            DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (ViewModel.ViewType!= (int)CSAViewType.Select)
                ViewModel.ViewType = (int)CSAViewType.Close;
            base.OnClosing(e);
        }
    }
}
