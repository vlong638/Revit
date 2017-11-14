using System;
using Autodesk.Revit.UI;
using MyRevit.MyTests.VLBase;
using MyRevit.Utilities;

namespace MyRevit.MyTests.PipeAttributesAnnotation
{
    public enum PAAViewType
    {
        Idle,//闲置
        Close,//关闭
        PickSinglePipe_Pipe,//选择单管 管道
        PickSinglePipe_Location,//选择单管 定位
        PickMultiplePipes,//选择多管
    }

    public class PAAViewModel : VLViewModel<PAAModel, PAAWindow>
    {
        public PAAViewType ViewType { set; get; }

        public PAAViewModel(UIApplication app) : base(app)
        {
            Model = new PAAModel("");
            View = new PAAWindow(this);
        }

        public override bool CanClose { get { return ViewType == PAAViewType.Idle; } }
        public override void Close() { ViewType = PAAViewType.Close; }

        public override void Execute()
        {
            switch (ViewType)
            {
                case PAAViewType.Idle:
                    View = new PAAWindow(this);
                    View.Relocate();
                    View.ShowDialog();
                    break;
                case PAAViewType.Close:
                    View.Close();
                    break;
                case PAAViewType.PickSinglePipe_Pipe:
                    MouseHookHelper.DelegateMouseHook(() =>
                    {
                        //TODO 业务逻辑处理
                        ViewType = PAAViewType.PickSinglePipe_Location;
                    }, () =>
                    {
                        ViewType = PAAViewType.Idle;
                    });
                    Execute();
                    break;
                case PAAViewType.PickSinglePipe_Location:
                    MouseHookHelper.DelegateMouseHook(() =>
                    {
                        //TODO 业务逻辑处理
                        ViewType = PAAViewType.PickSinglePipe_Pipe;
                    }, () =>
                    {
                        ViewType = PAAViewType.Idle;
                    });
                    Execute();
                    break;
                case PAAViewType.PickMultiplePipes:
                    //TODO 业务逻辑处理
                    Execute();
                    break;
                default:
                    break;
            }
        }
    }
}
