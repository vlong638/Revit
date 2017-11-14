using System;
using Autodesk.Revit.UI;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;

namespace MyRevit.MyTests.Template
{
    public enum TemplateViewType
    {
        Idle,//闲置
        Close,//关闭
        PickSinglePipe_Pipe,//选择单管 管道
        PickSinglePipe_Location,//选择单管 定位
        PickMultiplePipes,//选择多管
    }

    public class TemplateViewModel : VLViewModel<TemplateModel, TemplateWindow>
    {
        public TemplateViewType ViewType { set; get; }

        public TemplateViewModel(UIApplication app) : base(app)
        {
            Model = new TemplateModel("");
            View = new TemplateWindow(this);
        }

        public override bool CanClose { get { return ViewType == TemplateViewType.Idle; } }
        public override void Close() { ViewType = TemplateViewType.Close; }

        public override void Execute()
        {
            switch (ViewType)
            {
                case TemplateViewType.Idle:
                    View = new TemplateWindow(this);
                    View.Relocate();
                    View.ShowDialog();
                    break;
                case TemplateViewType.Close:
                    View.Close();
                    break;
                case TemplateViewType.PickSinglePipe_Pipe:
                    MouseHookHelper.DelegateMouseHook(() =>
                    {
                        //TODO 业务逻辑处理
                        ViewType = TemplateViewType.PickSinglePipe_Location;
                    }, () =>
                    {
                        ViewType = TemplateViewType.Idle;
                    });
                    Execute();
                    break;
                case TemplateViewType.PickSinglePipe_Location:
                    MouseHookHelper.DelegateMouseHook(() =>
                    {
                        //TODO 业务逻辑处理
                        ViewType = TemplateViewType.PickSinglePipe_Pipe;
                    }, () =>
                    {
                        ViewType = TemplateViewType.Idle;
                    });
                    Execute();
                    break;
                case TemplateViewType.PickMultiplePipes:
                    //TODO 业务逻辑处理
                    Execute();
                    break;
                default:
                    break;
            }
        }
    }
}
