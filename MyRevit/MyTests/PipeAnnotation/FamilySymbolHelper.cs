using Autodesk.Revit.DB;
using PmSoft.Common.RevitClass.Utils;
using System.Linq;

namespace PmSoft.Optimization.DrawingProduction.Utils
{
    public class FamilySymbolHelper
    {
        /// <summary>
        /// 加载族 加载失败或者本文档未找到,返回null 
        /// </summary>
        public static FamilySymbol LoadFamilySymbol(Document doc, string familyFilePath, string familyName, string symbolName)
        {
            //return FamilyLoadUtils.FindFamilySymbol_SubTransaction(doc, familyName, name);

            FamilySymbol symbol = null;
            var symbolFile = familyFilePath;
            Family family;
            if (doc.LoadFamily(symbolFile, out family))
            {
                //获取族类型集合Id
                var familySymbolIds = family.GetFamilySymbolIds();
                foreach (var familySymbolId in familySymbolIds)
                {
                    var element = doc.GetElement(familySymbolId) as FamilySymbol;
                    if (element != null && element.FamilyName == familyName)
                    {
                        symbol = element;
                        break;
                    }
                }
            }
            else
            {
                var symbols = new FilteredElementCollector(doc)
                    .WherePasses(new ElementClassFilter(typeof(FamilySymbol)));
                var targetSymbol = symbols.FirstOrDefault(c => (c as FamilySymbol).FamilyName == familyName && (c as FamilySymbol).Name == symbolName);
                if (targetSymbol != null)
                    symbol = targetSymbol as FamilySymbol;
            }
            return symbol;
        }
    }
}
