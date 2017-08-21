using Autodesk.Revit.DB;
using System;
using PmSoft.Common.CommonClass;
using System.Linq;
using Autodesk.Revit.DB.ExtensibleStorage;
using PmSoft.Optimization.DrawingProduction.Utils;
using System.Collections;
using System.Collections.Generic;

namespace PmSoft.Optimization.DrawingProduction
{
    /// <summary>
    /// 上下文
    /// </summary>
    public class PipeAnnotationContext
    {
        public static bool IsEditing;
        public static AnnotationCreater Creater = new AnnotationCreater();

        #region Collection
        private static PipeAnnotationEntityCollection Collection;
        private static CollectionStorageEntity CStorageEntity = new CollectionStorageEntity();

        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static PipeAnnotationEntityCollection GetCollection(Document doc)
        {
            if (Collection != null)
                return Collection;
            Collection = DelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, CStorageEntity, CStorageEntity.FieldOfData);
                    return new PipeAnnotationEntityCollection(data);
                },
                () =>
                {
                    return null;
                }
            );
            return Collection;
        }

        /// <summary>
        /// 保存Collection
        /// </summary>
        /// <param name="doc"></param>
        public static void SaveCollection(Document doc, bool isSecondTry = false)
        {
            if (Collection == null)
                return;
            var data = Collection.ToData();
            DelegateHelper.DelegateTryCatch(
               () =>
               {
                   ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
               },
               () =>
               {
                   ExtensibleStorageHelper.RemoveStorage(doc, CStorageEntity);
                   ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
               }
           );
        }


        /// <summary>
        /// 获取 Setting
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static string GetSetting(Document doc, bool isSecondTry = false)
        {
            return DelegateHelper.DelegateTryCatch(
                () =>
                {
                    return ExtensibleStorageHelper.GetData(doc, CStorageEntity, CStorageEntity.FieldOfSetting);
                },
                () =>
                {
                    return null;
                }
            );
        }

        /// <summary>
        /// 保存 Setting
        /// </summary>
        /// <param name="doc"></param>
        public static void SaveSetting(Document doc, string data)
        {
            DelegateHelper.DelegateTryCatch(
                () =>
                {
                    ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfSetting, data);
                },
                () =>
                {
                    ExtensibleStorageHelper.RemoveStorage(doc, CStorageEntity);
                    ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfSetting, data);
                }
            );
        }
        #endregion

        #region SharedSettings
        private static FamilySymbol SingleTagSymbol { set; get; }
        public static FamilySymbol GetSingleTagSymbol(Document doc)
        {
            if (SingleTagSymbol == null || !SingleTagSymbol.IsValidObject)
                LoadFamilySymbols(doc);
            return SingleTagSymbol;
        }

        private static FamilySymbol MultipleTagSymbol { set; get; }
        public static FamilySymbol GetMultipleTagSymbol(Document doc)
        {
            if (MultipleTagSymbol == null || !MultipleTagSymbol.IsValidObject)
                LoadFamilySymbols(doc);
            return MultipleTagSymbol;
        }

        /// <summary>
        /// 获取标注族
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static bool LoadFamilySymbols(Document doc)
        {
            SingleTagSymbol = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\Work\last\2016\SysFamily\出图深化\管道尺寸标记.rfa", "管道尺寸标记", "管道尺寸标记");
            MultipleTagSymbol = FamilySymbolHelper.LoadFamilySymbol(doc, @"E:\Work\last\2016\SysFamily\出图深化\多管直径标注.rfa", "多管直径标注", "引线标注_文字在右端");
            return true;
        }

        public static double TextSize = 2.5;
        public static double WidthScale = 1;
        public static double TextHeight = double.NaN;
        public static void LoadTextHeight(Document doc)
        {
            if (double.IsNaN(TextHeight))
            {
                var familyDoc = doc.EditFamily(SingleTagSymbol.Family);
                var textElement = new FilteredElementCollector(familyDoc).OfClass(typeof(TextElement)).First(c => c.Name == "2.5") as TextElement;
                var textSizeStr = textElement.Symbol.get_Parameter(BuiltInParameter.TEXT_SIZE).AsValueString();
                var textSize = double.Parse(textSizeStr.Substring(0, textSizeStr.IndexOf(" mm")));
                TextHeight = UnitHelper.ConvertToInch(48, UnitType.millimeter) * textSize;//48是字体大小1mm在Revit中对应的高度
            }
        }

        /// <summary>
        /// 获取族文件
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        private static string GetFamilyFile(string tagName)
        {
            string symbolFile;
            ApplicationPath.GetSysDataPath(out symbolFile);
            symbolFile += @"\SysFamily\出图深化\" + tagName + ".rfa";
            return symbolFile;
        }
        #endregion
    }

    /// <summary>
    /// PipeAnnotationEntityCollection扩展存储对象
    /// </summary>
    class CollectionStorageEntity : IExtensibleStorageEntity
    {
        public List<string> FieldNames { get { return new List<string>() { FieldOfData, FieldOfSetting }; } }
        public Guid SchemaId { get { return new Guid("D5874EC4-BAA8-45B0-988E-0776607CED39"); } }
        public string StorageName { get { return "PipeAnnotation_Schema"; } }
        public string SchemaName { get { return "PipeAnnotation_Schema"; } }
        public string FieldOfData { get { return "PipeAnnotation_Collection"; } }
        public string FieldOfSetting { get { return "PipeAnnotation_Settings"; } }
    }
}
