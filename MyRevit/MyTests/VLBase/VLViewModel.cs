using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.ComponentModel;
using System.Windows;
using System;

namespace MyRevit.MyTests.VLBase
{
    public abstract class VLViewModel<TModel, TView, TViewType> : VLViewModel
        where TModel : VLModel where TView : VLWindow
    {
        public VLViewModel(UIApplication app) : base(app)
        {
        }

        //Model
        public TModel Model { set; get; }
        //View
        public TView View { set; get; }
        //ViewType
        public TViewType ViewType { set; get; }
    }

    public abstract class VLViewModel: DependencyObject, INotifyPropertyChanged
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
        /// <summary>
        /// 界面是否处于ESC可退出的状态
        /// </summary>
        public abstract bool IsIdling { get; }
        public abstract void Close();
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
    }
}
