using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace MyRevit.Entities
{
    class MyExternalApplication : IExternalApplication
    {
        /// <summary>
        /// 主要负责资源释放
        /// </summary>
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// <summary>
        /// 主要负责启动时的定制化处理
        /// </summary>
        public Result OnStartup(UIControlledApplication application)
        {
            #region 添加定制的按钮
            //RibbonPanel panel = application.CreateRibbonPanel("VL Ribbon Panel");
            //PushButton button = panel.AddItem(new PushButtonData("vlName", "vlText", @"E:\WorkingSpace\Repository\Git\Revit\MyRevit\bin\Debug\MyRevit.dll"
            //    , "MyRevit.Entities.MyExternalCommand")) as PushButton;
            //button.LargeImage = new BitmapImage(new Uri(@"C:\Users\Administrator\Documents\Visual Studio 2015\Projects\MyRevit\MyRevit\Images\logoko.png"));
            //return Result.Succeeded; 
            #endregion

            #region 加密解密
            application.ControlledApplication.DocumentOpening += ControlledApplication_DocumentOpening;
            application.ControlledApplication.DocumentClosing += ControlledApplication_DocumentClosing;
            application.ControlledApplication.DocumentClosed += ControlledApplication_DocumentClosed;
            return Result.Succeeded;
            #endregion

        }

        byte[] EncrptionKey = new byte[23] { 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49 };
        private void ControlledApplication_DocumentOpening(object sender, Autodesk.Revit.DB.Events.DocumentOpeningEventArgs e)
        {
            var path = e.PathName;
            byte[] rvtInBytes = File.ReadAllBytes(path);
            for (int i = 0; i < EncrptionKey.Length; i++)
            {
                if (rvtInBytes[i] != EncrptionKey[i])
                    return;
            }
            byte[] rvtOutBytes = new byte[rvtInBytes.Length - EncrptionKey.Length];
            Buffer.BlockCopy(rvtInBytes, EncrptionKey.Length, rvtOutBytes, 0, rvtOutBytes.Length);
            File.WriteAllBytes(path, rvtOutBytes);
        }
        static string ClosingDocumentPath{set;get;}
        private void ControlledApplication_DocumentClosing(object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs e)
        {
            ClosingDocumentPath = e.Document.PathName;
        }
        private void ControlledApplication_DocumentClosed(object sender, Autodesk.Revit.DB.Events.DocumentClosedEventArgs e)
        {
            try
            {
                byte[] rvtInBytes = File.ReadAllBytes(ClosingDocumentPath);
                byte[] rvtOutBytes = new byte[EncrptionKey.Length + rvtInBytes.Length];
                EncrptionKey.CopyTo(rvtOutBytes, 0);
                rvtInBytes.CopyTo(rvtOutBytes, EncrptionKey.Length);
                File.WriteAllBytes(ClosingDocumentPath, rvtOutBytes);
            }
            catch (Exception ex)
            {
                string eee = ex.ToString();
            }
        }
    }
}
