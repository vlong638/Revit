using PmSoft.Common.CommonClass;
using System;

namespace MyRevit.Utilities
{
    class VLMouseHookHelper
    {
        /// <summary>
        ///  Try Catch 流程模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static bool DelegateMouseHook(Action action)
        {
            using (PmSoft.Common.RevitClass.PickObjectsMouseHook MouseHook = new PmSoft.Common.RevitClass.PickObjectsMouseHook())
            {
                MouseHook.InstallHook(PmSoft.Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
                try
                {
                    action();
                    MouseHook.UninstallHook();
                    return true;
                }
                catch (Exception ex)
                {
                    //Log(ex);
                    MouseHook.UninstallHook();
                    return false;
                }
            }
        }
        /// <summary>
        ///  Try Catch 流程模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static T DelegateMouseHook<T>(Func<T> func, Func<T> onError)
        {
            using (PmSoft.Common.RevitClass.PickObjectsMouseHook MouseHook = new PmSoft.Common.RevitClass.PickObjectsMouseHook())
            {
                MouseHook.InstallHook(PmSoft.Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
                try
                {
                    var result=  func();
                    MouseHook.UninstallHook();
                    return result;
                }
                catch (Exception ex)
                {
                    //Log(ex);
                    MouseHook.UninstallHook();
                    onError();
                }
            }
            return default(T);
        }
        /// <summary>
        /// 异常记录,有待优化
        /// </summary>
        /// <param name="ex"></param>
        private static void Log(Exception ex)
        {
            LogClass.GetInstance().AddLog(ex);
        }
    }
}
