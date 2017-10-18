using PmSoft.Optimization.DrawingProduction;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{

    /// <summary>
    /// CompoundStructureAnnotationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CompoundStructureAnnotationWindow : Window
    {
        CompoundStructureAnnotationSet Set{ set; get; }

        public CompoundStructureAnnotationWindow(CompoundStructureAnnotationSet set)
        {
            InitializeComponent();

            Set = set;
            DataContext = set.ViewModel;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            Set.ViewModel.ViewType = CSAViewType.Close;
            DialogResult = true;
        }

        private void CommandBinding_Executed_New(object sender, ExecutedRoutedEventArgs e)
        {
            if (Set.ViewModel.TextNoteTypeElementId == null)
            {
                MessageBox.Show("请先选择文字样式");
                return;
            }
            Set.ViewModel.ViewType = CSAViewType.Select;
            DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Set.ViewModel.ViewType!=CSAViewType.Select)
                Set.ViewModel.ViewType = CSAViewType.Close;
            base.OnClosing(e);
        }
    }
}
