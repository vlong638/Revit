using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.ComponentModel;
using System.Windows;
using System;
using System.Windows.Forms;

namespace MyRevit.MyTests.VLBase
{
    public abstract class VLViewModel<TModel, TView> : VLViewModel
        where TModel : VLModel where TView : VLWindow
    {
        public VLViewModel(UIApplication app) : base(app)
        {
        }

        //Model
        public TModel Model { set; get; }
        //View
        public TView View { set; get; }

        public void ShowMessage(string msg)
        {
            TaskDialog.Show("品茗Revit", msg);
        }
    }

    public abstract class VLViewModel : DependencyObject, INotifyPropertyChanged
    {
        //Revit
        public UIApplication UIApplication;
        public UIDocument UIDocument { get { return UIApplication.ActiveUIDocument; } }
        public Document Document { get { return UIApplication.ActiveUIDocument.Document; } }

        public VLViewModel(UIApplication app)
        {
            UIApplication = app;
        }

        #region 基本执行功能
        public abstract void Execute();
        #endregion

        #region ESC退出
        public int ViewType = 1;
        public bool IsIdling { get { return ViewType == 1; } }
        public void Close() { ViewType = 0; Execute(); }
        public void Closing() { ViewType = -1; Execute(); }
        #endregion

        #region INotifyPropertyChanged
        /// <summary>
        /// 实现INPC接口 监控属性改变
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region MessageBox
        public static void ShowMessage(string msg)
        {
            PmSoft.Common.Controls.PMMessageBox.Show(msg);
        }
        public static DialogResult ShowQuestion(string msg)
        {
            return PmSoft.Common.Controls.PMMessageBox.ShowQuestion(msg);
        }
        #endregion
    }
}
