using Autodesk.Revit.DB;
using MyRevit.MyTests.PAA;
using PmSoft.Optimization.DrawingProduction;
using PmSoft.Optimization.DrawingProduction.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.PipeAttributesAnnotation
{
    class PAAContext
    {
        #region Creator
        private static PAACreator _Creator = null;
        public static PAACreator Creator
        {
            get
            {
                return _Creator ?? (_Creator = new PAACreator());
            }
        }
        #endregion


        #region Storage
        public static bool IsEditing;
        private static PAAModelCollection Collection;
        private static CollectionStorageEntity CStorageEntity = new CollectionStorageEntity();
        /// <summary>
        /// 取数据Collection
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static PAAModelCollection GetCollection(Document doc)
        {
            //if (Collection != null)
            //    return Collection;
            Collection = DelegateHelper.DelegateTryCatch(
                () =>
                {
                    string data = ExtensibleStorageHelper.GetData(doc, CStorageEntity, CStorageEntity.FieldOfData);
                    return new PAAModelCollection(data);
                },
                () =>
                {
                    return new PAAModelCollection("");
                }
            );
            return Collection;
        }

        /// <summary>
        /// 保存Collection
        /// </summary>
        /// <param name="doc"></param>
        public static bool Save(Document doc)
        {
            if (Collection == null)
                return false;
            var data = Collection.ToData();
            return DelegateHelper.DelegateTryCatch(
                () =>
                {
                    ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
                    return true;
                },
                () =>
                {
                    ExtensibleStorageHelper.RemoveStorage(doc, CStorageEntity);
                    ExtensibleStorageHelper.SetData(doc, CStorageEntity, CStorageEntity.FieldOfData, data);
                    return false;
                }
            );
        }
        #endregion

    }
}
