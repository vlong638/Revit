using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using PmSoft.Common.RevitClass.CommonRevitClass;
using PmSoft.MainModel.EntData;
using PmSoft.MainModel.EntData.PmSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests.MepCurveAvoid
{
    /// <summary>
    /// 扩展Revit中Element类方法
    /// </summary>
    public static class RevitElementCommon
    {
        #region Element
        /// <summary>
        /// 读取构件的算量属性是否根据的判断值
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public static bool ReadIsFollow(this Element fi)
        {
            SchemaEntityOpr opr = new SchemaEntityOpr(fi);
            if (opr.IsValid())
            {
                String str = "";
                //读取comtype
                if (opr.GetParm("IsFollow", ref str))
                    return bool.Parse(str);
                else
                    return true;
            }
            else
                return true;
        }

        /// <summary>
        /// 写入构件的算量属性是否跟随的判断值
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="isFollow"></param>
        /// <returns></returns>
        public static bool WriteIsFollow(this Element fi, bool isFollow)
        {
            SchemaEntityOpr opr = new SchemaEntityOpr(fi);
            if (opr.IsValid())
            {
                opr.SetParm("IsFollow", isFollow.ToString());
                opr.SaveTo(fi);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 给族添加comtype信息
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="type"></param>
        /// <param name="isCheck">对添加是否成功的检查的标志位，默认不检查，并返回true</param>
        /// <returns></returns>
        public static bool WriteParm(this Element fi, PmComTypeEnum type, bool isCheck = false)
        {
            if (fi == null || !fi.IsValidObject)
                return false;

            MepDevParm parm = new MepDevParm() { comtype = 0 };
            parm.comtype = (int)type;
            parm.WriteTo(fi);

            if (isCheck)
            {
                MepDevParm checkParm = new MepDevParm() { comtype = 0 };
                checkParm.ReadFrom(fi);
                return checkParm.comtype == (int)type ? true : false;
            }
            return true;
        }

        /// <summary>
        /// 检测element的comtype
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool CheckTypeParam(this Element fi, int type)
        {
            MepDevParm parm = new MepDevParm() { comtype = 0 };
            parm.ReadFrom(fi);
            return parm.comtype == (int)type ? true : false;
        }

        /// <summary>
        /// 获取element的comtype
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        public static int ReadRarm(this Element fi)
        {
            MepDevParm parm = new MepDevParm() { comtype = 0 };
            parm.ReadFrom(fi);
            return parm.comtype;
        }

        /// <summary>
        /// 为该管件相连的管件添加comtype信息
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="type"></param>
        public static void WriteParm_ConnectedFitting(this FamilyInstance fi, PmComTypeEnum type)
        {
            if (fi == null || !fi.IsValidObject || fi.MEPModel == null || fi.MEPModel.ConnectorManager == null)
                return;

            foreach (Connector con in fi.MEPModel.ConnectorManager.Connectors)
            {
                var linkedCon = con.GetConnectedConnector();
                if (linkedCon != null && linkedCon.Owner is FamilyInstance &&
                    (linkedCon.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_PipeFitting || linkedCon.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_DuctFitting || linkedCon.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_CableTrayFitting || linkedCon.Owner.Category.Id.IntegerValue == (int)BuiltInCategory.OST_ConduitFitting))
                {
                    var linkedFi = linkedCon.Owner as FamilyInstance;
                    linkedFi.WriteParm(type, false);
                }
            }
        }

        /// <summary>
        /// 根据MEPCurve获取要添加的comType信息
        /// </summary>
        /// <param name="mep"></param>
        /// <returns></returns>
        public static PmComTypeEnum GetParmType(this MEPCurve mep)
        {
            if (mep is Duct)
                return PmComTypeEnum.BIMAZ_NT_GJ;
            else if (mep is Pipe)
            {
                string[] firelist = new string[] { "干式消防系统", "湿式消防系统", "其他消防系统", "预作用消防系统" };
                string[] jpslist = new string[] { "卫生设备", "家用冷水", "家用热水", "通气管", "其他", "循环供水", "循环回水" };
                if (firelist.Contains(mep.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString()))
                    return PmComTypeEnum.BIMAZ_XF_GJ;
                else if (jpslist.Contains(mep.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString()))
                    return PmComTypeEnum.BIMAZ_JPS_GJ;

                return PmComTypeEnum.BIMAZ_JPS_GJ;
            }
            else if (mep is CableTray)
            {
                if ((mep as CableTray).IsStrongElectricity())
                    return PmComTypeEnum.BIMAZ_DQ_QJGJ;
                else
                    return PmComTypeEnum.BIMAZ_ZNH_QJGJ;
            }
            else if (mep is Conduit)
            {
                if ((mep as Conduit).IsStrongElectricity())
                    return PmComTypeEnum.BIMAZ_DQ_JXH;
                else
                    return PmComTypeEnum.BIMAZ_ZNH_JXH;
            }
            else
                return PmComTypeEnum.BIMAZ_ZNH_BGSL;//其实只是随便给了个值
        }

        /// <summary>
        /// 根据MEPCurve获取要添加的comType信息
        /// </summary>
        /// <param name="mep"></param>
        /// <returns></returns>
        public static PmComTypeEnum GetMEPCurveComType(this MEPCurve mep)
        {
            if (mep is Duct)
                return PmComTypeEnum.BIMAZ_NT_FG;
            else if (mep is Pipe)
            {
                string[] firelist = new string[] { "干式消防系统", "湿式消防系统", "其他消防系统", "预作用消防系统" };
                string[] jpslist = new string[] { "卫生设备", "家用冷水", "家用热水", "通气管", "其他", "循环供水", "循环回水" };
                if (firelist.Contains(mep.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString()))
                    return PmComTypeEnum.BIMAZ_XF_GD;
                else if (jpslist.Contains(mep.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString()))
                    return PmComTypeEnum.BIMAZ_JPS_GD;
                return PmComTypeEnum.BIMAZ_JPS_GD;
            }
            else if (mep is CableTray)
            {
                if ((mep as CableTray).IsStrongElectricity())
                    return PmComTypeEnum.BIMAZ_DQ_QJ;
                else
                    return PmComTypeEnum.BIMAZ_ZNH_QJ;
            }
            else if (mep is Conduit)
            {
                if ((mep as Conduit).IsStrongElectricity())
                    return PmComTypeEnum.BIMAZ_DQ_DX;
                else
                    return PmComTypeEnum.BIMAZ_ZNH_RDX;
            }
            else
                return PmComTypeEnum.BIMAZ_ZNH_BGSL;//其实只是随便给了个值
        }

        /// <summary>
        /// 根据MEPCurve获取要添加的支吊架的comType信息
        /// </summary>
        /// <param name="mep"></param>
        /// <returns></returns>
        public static PmComTypeEnum GetParmTypeHanger(this MEPCurve mep)
        {
            if (mep is Duct)
                return PmComTypeEnum.BIMAZ_NT_QJZJ;
            else if (mep is Pipe)
            {
                string[] firelist = new string[] { "干式消防系统", "湿式消防系统", "其他消防系统", "预作用消防系统" };
                string[] jpslist = new string[] { "卫生设备", "家用冷水", "家用热水", "通气管", "其他", "循环供水", "循环回水" };
                if (firelist.Contains(mep.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString()))
                    return PmComTypeEnum.BIMAZ_XF_QJZJ;
                else if (jpslist.Contains(mep.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM).AsString()))
                    return PmComTypeEnum.BIMAZ_JPS_QJZJ;
                return PmComTypeEnum.BIMAZ_JPS_QJZJ;
            }
            else if (mep is CableTray)
            {
                if ((mep as CableTray).IsStrongElectricity())
                    return PmComTypeEnum.BIMAZ_DQ_QJZJ;
                else
                    return PmComTypeEnum.BIMAZ_ZNH_QJZJ;
            }
            else if (mep is Conduit)
            {
                if ((mep as Conduit).IsStrongElectricity())
                    return PmComTypeEnum.BIMAZ_DQ_PGZJ;
                else
                    return PmComTypeEnum.BIMAZ_ZNH_PGZJ;
            }
            else
                return PmComTypeEnum.BIMAZ_ZNH_BGSL;//其实只是随便给了个值
        }

        /// <summary>
        /// 给饰面添加hostElementId信息
        /// </summary>
        /// <param name="fi">要添加信息的元素</param>
        /// <param name="hosstId">hostElementId的值</param>
        /// <returns></returns>
        public static bool WriteHostElementId(this Element fi, ElementId hosstId)
        {
            if (fi == null || !fi.IsValidObject)
                return false;
            MepDevParm parm = new MepDevParm() { HostElementId = hosstId };
            parm.WriteHostElementId(fi);
            return true;
        }

        /// <summary>
        /// 获取饰面hostElementId信息
        /// </summary>
        /// <param name="fi">要读取的元素</param>
        /// <returns>对应的hostElementId信息</returns>
        public static ElementId ReadHostElementId(this Element fi)
        {
            MepDevParm parm = new MepDevParm();
            parm.ReadHostElementId(fi);
            return parm.HostElementId;
        }

        /// <summary>
        /// 设置元素包含的元素id列表
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        public static bool WriteOwnedElementIds(this Element ele, List<string> list)
        {
            if (ele == null || !ele.IsValidObject)
                return false;
            MepDevParm parm = new MepDevParm();
            parm.WriteOwnedElementIds(ele, list);
            return true;
        }

        /// <summary>
        /// 读取元素包含的元素id列表
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        public static List<string> ReadOwnedElementIds(this Element ele)
        {
            MepDevParm parm = new MepDevParm();
            return parm.ReadOwnedElementIds(ele);
        }
        #endregion
    }
}
