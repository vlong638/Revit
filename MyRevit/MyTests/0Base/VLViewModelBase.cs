using System.ComponentModel;
using System.Windows;

namespace MyRevit.MyTests._0Base
{
    public abstract class VLViewModelBase : DependencyObject, INotifyPropertyChanged
    {
        #region ESC退出
        /// <summary>
        /// 界面是否处于ESC可退出的状态
        /// </summary>
        public abstract bool IsIdling { get; } 
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
