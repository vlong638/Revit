using MyRevit.MyTests.BeamAlignToFloor;
using PmSoft.Optimization.DrawingProduction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{

    /// <summary>
    /// CompoundStructureAnnotationWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CompoundStructureAnnotationWindow : Window
    {
        public CompoundStructureAnnotationViewModel ViewModel { set; get; }
        CompoundStructureAnnotationSet Set{ set; get; }

        public CompoundStructureAnnotationWindow(CompoundStructureAnnotationSet set)
        {
            InitializeComponent();

            Set = set;
            ViewModel = new CompoundStructureAnnotationViewModel();
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void CommandBinding_Executed_New(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.ViewType = CompoundStructureAnnotationViewType.Select;
        }
    }
}
