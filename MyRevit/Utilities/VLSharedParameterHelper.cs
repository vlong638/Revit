using PmSoft.Common.CommonClass;

namespace MyRevit.Utilities
{
    /// <summary>
    /// class 选择过滤器
    /// 支持文档及链接文档内的选择
    /// </summary>
    public class VLSharedParameterHelper
    {
        /// <summary>
        /// 取共享文件的地址
        /// </summary>
        /// <returns></returns>
        public static string GetShareFilePath()
        {
            return ApplicationPath.GetParentPathOfCurrent + @"\sysdata\" + "PMSharedParameters.txt";
        }
    }
}
