using PmSoft.Common.CommonClass;
using System;

namespace MyRevit.Utilities
{
    class VLHookHelper
    {
        #region Sample
        //KeyBoardHook KeyBoarHook;
        //void Unhook()
        //{
        //    if (KeyBoarHook != null)
        //    {
        //        KeyBoarHook.UnHook();
        //        KeyBoarHook.OnKeyDownEvent -= KeyBoarHook_OnKeyDownEvent;
        //        GC.Collect();//增加GC防止窗体关闭后,钩子未被卸载.
        //    }
        //}

        //void Hook()
        //{
        //    KeyBoarHook = new KeyBoardHook();
        //    KeyBoarHook.SetHook();
        //    KeyBoarHook.OnKeyDownEvent += KeyBoarHook_OnKeyDownEvent;
        //}

        ///// <summary>
        ///// 钩子事件,监听ESC关闭窗体
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        ///// <returns></returns>
        //int KeyBoarHook_OnKeyDownEvent(object sender, System.Windows.Forms.KeyEventArgs e)
        //{
        //    if (ViewModel != null && ViewModel.IsIdling && e.KeyData == System.Windows.Forms.Keys.Escape)
        //    {
        //        ViewModel.Close();
        //        return 1;
        //    }
        //    return 0;
        //} 
        #endregion

        #region Mouse
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
                    MouseHook.UninstallHook();
                    return false;
                }
            }
        } 
        #endregion

        #region KeyBoard
        /// <summary>
        ///  Try Catch 流程模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static bool DelegateKeyBoardHook(Action action)
        {
            KeyBoardHook hook = new KeyBoardHook();
            hook.SetHook();
            try
            {
                action();
                Unhook(hook);
                return true;
            }
            catch (Exception ex)
            {
                Unhook(hook);
                return false;
            }
        }
        private static void Unhook(KeyBoardHook hook)
        {
            if (hook != null)
            {
                hook.UnHook();
                GC.Collect();
            }
        } 
        #endregion

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
