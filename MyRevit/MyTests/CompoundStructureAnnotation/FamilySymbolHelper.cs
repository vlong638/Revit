using Autodesk.Revit.DB;
using PmSoft.Common.RevitClass.Utils;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    public class FamilySymbolHelper
    {
        /// <summary>
        /// 加载族 加载失败或者本文档未找到,返回null 
        /// </summary>
        public static FamilySymbol LoadFamilySymbol(Document doc, string familyName, string name)
        {
            return FamilyLoadUtils.FindFamilySymbol_SubTransaction(doc, familyName, name);
        }
    }
}
