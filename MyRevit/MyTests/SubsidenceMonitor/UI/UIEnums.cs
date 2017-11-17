namespace MyRevit.SubsidenceMonitor.UI
{
    public enum SubFormType
    {
        None,
        Subsidence,
        SkewBack,
    }
    public enum ShowDialogType
    {
        /// <summary>
        /// 闲置
        /// </summary>
        Idle,
        /// <summary>
        /// 增加构件
        /// </summary>
        AddElements_ForDetail,
        /// <summary>
        /// 删除构件
        /// </summary>
        DeleleElements_ForDetail,
        /// <summary>
        /// 测点查看_查看选中
        /// </summary>
        ViewElementsBySelectedNodes,
        /// <summary>
        /// 测点查看_查看全部
        /// </summary>
        ViewElementsByAllNodes,
        /// <summary>
        /// 本次最大变量查看_红色显示
        /// </summary>
        ViewCurrentMaxByRed,
        /// <summary>
        /// 本次最大变量查看_整体查看
        /// </summary>
        ViewCurrentMaxByAll,
        /// <summary>
        /// 累计最大变量查看_红色显示
        /// </summary>
        ViewTotalMaxByRed,
        /// <summary>
        /// 累计最大变量查看_整体查看
        /// </summary>
        ViewTotalMaxByAll,
        /// <summary>
        /// 接近预警预览
        /// </summary>
        ViewCloseWarn,
        /// <summary>
        /// 超出预警预览
        /// </summary>
        ViewOverWarn,
    }
    public static class ShowDialogTypeEx
    {
        public static string GetViewName(this ShowDialogType showDialogType)
        {
            string result = "默认视图";
            switch (showDialogType)
            {
                case ShowDialogType.AddElements_ForDetail:
                case ShowDialogType.DeleleElements_ForDetail:
                    result = "测点编辑";
                    break;
                case ShowDialogType.ViewElementsBySelectedNodes:
                    result = "测点查看_查看选中";
                    break;
                case ShowDialogType.ViewElementsByAllNodes:
                    result = "测点查看_查看全部";
                    break;
                case ShowDialogType.ViewCurrentMaxByRed:
                    result = "本次最大变量查看_红色显示";
                    break;
                case ShowDialogType.ViewCurrentMaxByAll:
                    result = "本次最大变量查看_整体查看";
                    break;
                case ShowDialogType.ViewTotalMaxByRed:
                    result = "累计最大变量查看_红色显示";
                    break;
                case ShowDialogType.ViewTotalMaxByAll:
                    result = "累计最大变量查看_整体查看";
                    break;
                case ShowDialogType.ViewCloseWarn:
                    result = "接近预警预览";
                    break;
                case ShowDialogType.ViewOverWarn:
                    result = "超出预警预览";
                    break;
            }
            return result;
        }
    }
}
