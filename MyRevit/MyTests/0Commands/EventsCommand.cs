using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Text;
using System.ComponentModel;

namespace MyRevit.Entities
{
    class MyExternalEventHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            TaskDialog.Show(nameof(MyExternalEventHandler), "来自外部事件的消息");
        }

        public string GetName()
        {
            return nameof(MyExternalEventHandler);
        }
    }
    public class ExternalEventSampleForm : System.Windows.Forms.Form
    {
        MyExternalEventHandler MyExternalEventHandler;
        ExternalEvent MyExternalEvent;
        public ExternalEventSampleForm() : base()
        {
            MyExternalEventHandler = new MyExternalEventHandler();
            MyExternalEvent = ExternalEvent.Create(MyExternalEventHandler);
            MyExternalEvent.Raise();
        }

        protected override void OnClosed(EventArgs e)
        {
            //外部事件资源需手动释放
            MyExternalEvent.Dispose();
            base.OnClosed(e);
        }
    }


    [Transaction(TransactionMode.Manual)]
    class EventsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var doc = commandData.Application.ActiveUIDocument.Document;
                //注册事件
                doc.DocumentSaving += Doc_DocumentSaving;
                ShowMessage("注册事件");
                //反注册事件
                doc.DocumentSaving -= Doc_DocumentSaving;
                ShowMessage("反注册事件");
                //可取消的事件
                doc.DocumentSaving += Doc_DocumentSaving;
                doc.Save();
                //闲置事件
                var uiApp = commandData.Application;
                uiApp.Idling += UiApp_Idling;
                //外部事件
                var form = new ExternalEventSampleForm();
                form.Show();
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }

        private void Doc_DocumentSaving(object sender, Autodesk.Revit.DB.Events.DocumentSavingEventArgs e)
        {
            if (e.Cancellable)
            {
                e.Cancel();
                ShowMessage("取消事件");
            }
        }
        int idling = 0;
        private void UiApp_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            if (idling < 3)
            {
                //非默认模式
                e.SetRaiseWithoutDelay();
                idling++;
            }
            ShowMessage("闲置事件:" + idling);
        }

        private static void ShowMessage(string message)
        {
            TaskDialog.Show(nameof(EventsCommand), message);
        }
    }
}
