using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using PmSoft.Common.RevitClass.CommonRevitClass;
using PmSoft.Common.RevitClass.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.MyTests.MepCurveAvoid
{
    public interface IMEPCurveConnectControlService
    {
        bool IsOnlyUseRevitDefault { get; set; }

        void NewTwoFitting(MEPCurve mep1, MEPCurve mep2, FamilySymbol fs);

        void NewThreeFitting(MEPCurve mep1, MEPCurve mep2, MEPCurve mep3, FamilySymbol fs);

        void NewFourFitting(MEPCurve dst1, MEPCurve dst2, MEPCurve dst3, MEPCurve dst4, FamilySymbol fs);
    }
    public class MEPCurveConnectControlService : BaseControlService, IMEPCurveConnectControlService
    {
        protected enum MEPCurveConnectTypeENUM { MultiShapeTransition, Transition, Elbow, Tee, Cross, TakeOff }

        private enum MEPCurveTeeFilterTypeEnum { Normal, T, Y }

        private enum MEPCurveCrossFilterTypeEnum { Normal, Bevel }

        public bool IsOnlyUseRevitDefault { get; set; }

        public MEPCurveConnectControlService(UIApplication uiApp)
            : base(uiApp)
        {

        }

        #region 通用方法(public)
        public List<IEnumerable<ElementId>> FindAllPaths(MEPCurve startMep, MEPCurve targetMep)
        {
            List<IEnumerable<ElementId>> pathsList = new List<IEnumerable<ElementId>>();

            this.Finding(startMep, targetMep, null, pathsList);

            pathsList.Sort((p1, p2) => { return p1.Count().CompareTo(p2.Count()); });

            return pathsList;
        }

        private void Finding(MEPCurve mep, MEPCurve targetMep, Stack<ElementId> stack, List<IEnumerable<ElementId>> pathsList, Connector beforeCon = null)
        {
            //第一次调用则初始化
            if (stack == null)
                stack = new Stack<ElementId>();

            //队列中已经存在直接返回，无则添加
            if (!stack.Contains(mep.Id))
                stack.Push(mep.Id);
            else
                return;

            //如果找到目标，保存路径，然后出栈一个
            if (mep.Id == targetMep.Id)
            {
                pathsList.Add(stack.ToArray());
                stack.Pop();
                return;
            }

            List<int> ints = new List<int>();
            foreach (Connector con in mep.ConnectorManager.Connectors)
            {
                ints.Add(con.Id);
            }
            ints.Sort();
            foreach (int i in ints)
            {
                var con = mep.ConnectorManager.Lookup(i);
                if (beforeCon != null && con.Id == beforeCon.Id)//已经被使用过了，则直接跳过
                    continue;

                var linkedC = con.GetConnectedConnector();
                if (linkedC != null)
                {
                    if (linkedC.Owner is MEPCurve)
                        this.Finding(linkedC.Owner as MEPCurve, targetMep, stack, pathsList, linkedC);
                    else if (linkedC.Owner is FamilyInstance)
                        this.Finding(linkedC.Owner as FamilyInstance, targetMep, stack, pathsList, linkedC);
                }
            }

            stack.Pop();
        }

        private void Finding(FamilyInstance fi, MEPCurve targetMep, Stack<ElementId> stack, List<IEnumerable<ElementId>> pathsList, Connector beforeCon)
        {
            List<int> ints = new List<int>();
            foreach (Connector con in fi.MEPModel.ConnectorManager.Connectors)
            {
                ints.Add(con.Id);
            }
            ints.Sort();
            foreach (int i in ints)
            {
                var con = fi.MEPModel.ConnectorManager.Lookup(i);
                if (beforeCon != null && con.Id == beforeCon.Id)//已经被使用过了，则直接跳过
                    continue;

                var linkedC = con.GetConnectedConnector();
                if (linkedC != null)
                {
                    if (linkedC == null)
                        continue;

                    if (linkedC.Owner is MEPCurve)
                        this.Finding(linkedC.Owner as MEPCurve, targetMep, stack, pathsList, linkedC);
                    else if (linkedC.Owner is FamilyInstance)
                        this.Finding(linkedC.Owner as FamilyInstance, targetMep, stack, pathsList, linkedC);
                    else
                        continue;
                }
            }
        }

        private List<ElementId> findedIds = null;
        private List<ElementId> findPaths = null;
        public List<ElementId> FindPaths { get { return this.findPaths; } }
        /// <summary>
        /// 查找elem所在管道系统中符合func判断条件的首个管道
        /// 此函数亦可用于对系统中每个管道进行一定处理
        /// connector必须是elem中的连接点
        /// 这是递归函数，connector该端的系统将不会去查找，故第一次调用时推荐传入null
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="connector"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public MEPCurve FindMEPCurve(Element elem, Connector connector, Func<MEPCurve, bool> func)
        {
            if (connector == null)
            {
                findedIds = new List<ElementId>();
                findPaths = new List<ElementId>();
            }

            if (findedIds.FirstOrDefault(p => p == elem.Id) == null)
                findedIds.Add(elem.Id);
            else
                return null;

            if (elem is MEPCurve && findPaths.FirstOrDefault(p => p == elem.Id) == null)
                findPaths.Add(elem.Id);

            ConnectorManager connectorM = null;
            if (elem is MEPCurve)
            {
                var temp = elem as MEPCurve;
                connectorM = temp.ConnectorManager;

                if (func.Invoke(temp))
                    return temp;
            }
            else if (elem is FamilyInstance)
            {
                var temp = elem as FamilyInstance;

                if (temp.MEPModel == null)
                    return null;

                connectorM = temp.MEPModel.ConnectorManager;
            }

            foreach (Connector con in connectorM.Connectors)
            {
                if (connector == null || con.Id != connector.Id)
                {
                    Connector linkedC = con.GetConnectedConnector();
                    if (linkedC == null)
                        continue;

                    if (linkedC.Owner is FamilyInstance || linkedC.Owner is MEPCurve)
                    {
                        MEPCurve temp = FindMEPCurve(linkedC.Owner, linkedC, func);
                        if (temp != null)
                            return temp;
                        else
                            continue;
                    }
                    else
                        continue;
                }
            }

            if (elem is MEPCurve && findPaths.FirstOrDefault(p => p == elem.Id) != null)
                findPaths.Remove(elem.Id);

            return null;
        }
        #endregion

        #region 顶级连接方法(public)
        public void NewTwoFitting(MEPCurve mep1, MEPCurve mep2, FamilySymbol fs)
        {
            if (RevitCommon.IsInXYPlaneAndSame(mep1, mep2))//在同一xy平面
            {
                if (mep1.IsColinearTo(mep2))//共线
                {
                    this.NewTransitionDefault(mep1, mep2, fs);
                }
                else if (mep1.IsParallelTo(mep2))//平行
                {
                    this.NewElbowGoBackDefault(mep1, mep2, fs, this.RevitDoc, 90);
                }
                else if (!mep1.IsIntersectWith(mep2))//不相交
                {
                    this.NewTwoDisjoint(mep1, mep2);
                }
                else if (mep1.IsIntersectWith(mep2))//相交
                {
                    this.NewTwoOverlap(mep1, mep2);
                }
            }
            else if (RevitCommon.IsInXYPlaneAndUnSame(mep1, mep2))//在不同的xy平面
            {
                if (mep1.IsParallelTo(mep2))//平行
                {
                    this.NewTwo_UnSameXYPlane_Parallel(mep1, mep2);
                }
                else if (!mep1.IsIntersectWith_XYPlane_ChangeToSamePlane(mep2))//不相交
                {
                    if (mep1 is CableTray && mep2 is CableTray)
                    {
                        this.NewTwo_CableTray_UnSameXYPlane_Disjoint(mep1, mep2);
                    }
                    else
                    {
                        this.NewTwo_UnSameXYPlane_Disjoint(mep1, mep2);
                    }
                }
                else if (mep1.IsIntersectWith_XYPlane_ChangeToSamePlane(mep2))//相交
                {
                    this.NewTwo_UnSameXYPlane_Overlap(mep1, mep2);
                }
            }
            else if (mep1.IsStand() && mep2.IsStand())//都是立管
            {
                if (mep1.IsColinearTo(mep2))//共线
                {
                    this.NewTransitionDefault(mep1, mep2, fs);
                }
                else
                {
                    throw new Exception("暂不支持两根非共线立管连接");
                }
            }
            else if (RevitCommon.IsOneSamllSlopeAndOneCross(mep1, mep2))//横管 | 小坡度管
            {
                this.NewTwo_OneCrossAndOneStandOrSlope(mep1, mep2);
            }
            else if (RevitCommon.IsOneStandAndOneCrossOrSlope(mep1, mep2))//立管 | 小坡度管或者横管
            {
                this.NewTwo_OneStandAndOneCrossOrSlope(mep1, mep2);
            }
            else if (RevitCommon.IsOneCrossAndOneStandOrSlope(mep1, mep2))
            {
                this.NewTwo_OneCrossAndOneStandOrSlope(mep1, mep2);
            }
            else if (RevitCommon.IsIntersectWithPointLineExpandTwo_ChangeToXYPlane(mep1, mep2))  // 延长小坡度管1并上下平移小坡度管2后相交
            {
                this.NewTwo_TwoSlope(mep1, mep2);
            }
            else
            {
                throw new Exception("暂无法连接该两根管道:只支持小坡度的管道");
            }
        }

        public void NewThreeFitting(MEPCurve mep1, MEPCurve mep2, MEPCurve mep3, FamilySymbol fs)
        {
            this.NewTeePM(mep1, mep2, mep3, fs);
        }

        public void NewFourFitting(MEPCurve dst1, MEPCurve dst2, MEPCurve dst3, MEPCurve dst4, FamilySymbol fs)
        {
            this.NewCrossDefault(dst1, dst2, dst3, dst4, fs, true);
        }

        /// <summary>
        /// 排水倒角连接
        /// FamilySymbol仅用作判断截面类型是否符合，并不修改族类型
        /// Defaut的意义：用doc.Create中的NewFitting相关函数创建管件并自动连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        public FamilyInstance[] NewDrainPipeChamferDefault(MEPCurve src, MEPCurve dst, FamilySymbol fs, bool isStand)
        {

            FamilyInstance[] fis = new FamilyInstance[2];

            //保证src管道的直径大于dst
            Parameter srcLengthParam = src.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
            Parameter dstLengthParam = dst.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
            if (srcLengthParam != null && dstLengthParam != null)
            {
                if (srcLengthParam.AsDouble() < dstLengthParam.AsDouble())
                {
                    MEPCurve temp = src;
                    src = dst;
                    dst = temp;
                }
            }

            try
            {
                double scale = 0.6 / UnitTransUtils.MMToFeet(80) * src.GetWidth();
                XYZ oriXYZ = null, moveXYZ = null;
                Connector connector1 = null, connector2 = null;
                double angle = 0;

                //确定相交点，并移动第二根管道
                if (isStand)
                {
                    src.GetLine().IsIntersectWith(RevitVerUtil.CreateByNormalAndOrigin(dst.GetLine().Direction.GetNormal(XYZ.BasisX), dst.GetLine().Origin), out oriXYZ);
                    moveXYZ = Line.CreateUnbound(dst.GetLine().Origin, dst.GetLine().Direction).GetIntersectWithPoint(Line.CreateUnbound(oriXYZ, XYZ.BasisX));
                    if (moveXYZ == null)
                    {
                        src.GetLine().IsIntersectWith(RevitVerUtil.CreateByNormalAndOrigin(dst.GetLine().Direction.GetNormal(XYZ.BasisY), dst.GetLine().Origin), out oriXYZ);
                        moveXYZ = Line.CreateUnbound(dst.GetLine().Origin, dst.GetLine().Direction).GetIntersectWithPoint(Line.CreateUnbound(oriXYZ, XYZ.BasisY));
                    }
                }
                else
                {
                    src.GetLine().IsIntersectWith(RevitVerUtil.CreateByNormalAndOrigin(dst.GetLine().Direction.GetNormal(XYZ.BasisZ), dst.GetLine().Origin), out oriXYZ);
                    moveXYZ = Line.CreateUnbound(dst.GetLine().Origin, dst.GetLine().Direction).GetIntersectWithPoint(Line.CreateUnbound(oriXYZ, XYZ.BasisZ));
                }
                ElementTransformUtils.MoveElement(this.RevitDoc, dst.Id, oriXYZ - moveXYZ);


                connector1 = src.GetClosestConnector(oriXYZ);
                connector2 = dst.GetClosestConnector(oriXYZ);

                angle = connector1.CoordinateSystem.BasisZ.AngleTo(connector2.CoordinateSystem.BasisZ);
                angle = angle > Math.PI ? angle - Math.PI : angle;
                if (angle < (Math.PI * 90 / 180))
                    throw new Exception("两根管道中心线相交的角度必须大于等于90°");

                if (connector1.IsConnected && connector2.IsConnected)//判断两根管道是否已经通过弯头连接在一起
                {
                    var linkedC = connector1.GetConnectedConnector();
                    var linkedAnthorC = (linkedC.Owner as FamilyInstance).GetAnthorConnector(linkedC);
                    var AnthorLinkedC = linkedAnthorC.GetConnectedConnector();
                    if (AnthorLinkedC != null && AnthorLinkedC.Owner.Id == connector2.Owner.Id)
                    {
                        this.RevitDoc.Delete(linkedC.Owner.Id);
                    }
                }

                List<FamilyInstance> elbowsList = connector1.GetConnectedElbowsByConnector(connector2);
                if (elbowsList != null)
                {
                    //删除原来连接的管件
                    foreach (var elbows in elbowsList)
                    {
                        this.RevitDoc.Delete(elbows.Id);
                    }
                }

                if (connector1.IsConnected)
                    connector1.DisconnectFrom(connector1.GetConnectedConnector());
                if (connector2.IsConnected)
                    connector2.DisconnectFrom(connector2.GetConnectedConnector());


                //创建连接管道，并调整位置
                var copy = src.GetCopy(this.RevitDoc);
                (copy.Location as LocationCurve).Curve = Line.CreateBound(oriXYZ,
                    oriXYZ + scale * connector1.CoordinateSystem.BasisZ.RotateBy(connector2.CoordinateSystem.BasisZ.GetNormal(connector1.CoordinateSystem.BasisZ), 90 - (angle / Math.PI) * 180 / 2));
                ElementTransformUtils.MoveElement(this.RevitDoc, copy.Id, -connector1.CoordinateSystem.BasisZ * Math.Cos((Math.PI / 2) / 2) * scale);

                NewElbowDefault(copy, src, null);
                NewElbowDefault(copy, dst, null);
                return fis;
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionDefault(ex, src, MEPCurveConnectTypeENUM.Elbow);
                return fis;
            }
            finally
            {
                if (fis[0] != null)
                {
                    fis[0].WriteParm(src.GetParmType(), false);
                    fis[0].WriteParm_ConnectedFitting(src.GetParmType());
                }
                if (fis[1] != null)
                {
                    fis[1].WriteParm(src.GetParmType(), false);
                    fis[1].WriteParm_ConnectedFitting(src.GetParmType());
                }
            }
        }
        #endregion

        #region 风管特有的连接处理(public)
        private enum DuctConnectTypeENUM { TopRect, BottomRect, MiddleRect, Round, Unknow }
        private Dictionary<DuctConnectTypeENUM, string> teeDic = new Dictionary<DuctConnectTypeENUM, string>
        {
            { DuctConnectTypeENUM.TopRect,"PM-带过渡件的三通-顶对齐:顶对齐"},
            { DuctConnectTypeENUM.MiddleRect,"PM-矩形圆角三通:PM-矩形圆角三通"},
            { DuctConnectTypeENUM.BottomRect,"PM-带过渡件的三通-底对齐:底对齐"},
            { DuctConnectTypeENUM.Round,"PM-圆形直角三通:标准"},
        };
        //public FamilyInstance NewDuctTransition(Connector fiCon, Connector mepCon, FamilySymbol fs, bool isAline = false)
        //{
        //    if (fs == null)
        //        fs = this.Judge_LoadDefaultFitting(mepCon.Owner as MEPCurve, MEPCurveConnectTypeENUM.Transition);
        //    if (fs == null)
        //        throw new Exception("未能加载到过渡管件族");

        //    FamilyInstance fi = null;

        //    try
        //    {
        //        #region 对齐
        //        if (isAline)
        //        {
        //            XYZ closestXYZ = (mepCon.Owner as MEPCurve).GetClosestPoint(fiCon.Origin);
        //            mepCon.Origin = closestXYZ;
        //            ElementTransformUtils.MoveElement(mepCon.Owner.Document, mepCon.Owner.Id, fiCon.Origin - mepCon.Origin);
        //        }
        //        #endregion

        //        //管件大小相同、形状相同时，连接
        //        if ((fiCon.Shape == ConnectorProfileType.Round && mepCon.Shape == ConnectorProfileType.Round && fiCon.Radius == mepCon.Radius) ||
        //            (fiCon.Shape == ConnectorProfileType.Rectangular && mepCon.Shape == ConnectorProfileType.Rectangular && fiCon.Width.IsAlmostEqualTo(mepCon.Width) && fiCon.Height.IsAlmostEqualTo(mepCon.Height)) ||
        //            (fiCon.Shape == ConnectorProfileType.Rectangular && mepCon.Shape == ConnectorProfileType.Rectangular && fiCon.Width.IsAlmostEqualTo(mepCon.Height) && fiCon.Height.IsAlmostEqualTo(mepCon.Width)))
        //        {
        //            mepCon.Origin = fiCon.Origin;
        //            fiCon.ConnectTo(mepCon);
        //            return null;
        //        }
        //        else
        //        {
        //            Connector fiConnector1 = null, fiConnector2 = null;
        //            Connector startCon = fiCon, endCon = mepCon;
        //            double xDis = 0, yDis = 0;

        //            #region 创建过渡件
        //            var level = this.RevitDoc.GetElement((mepCon.Owner as MEPCurve).GetParameters("参照标高").First().AsElementId()) as Level;
        //            fi = this.RevitDoc.Create.NewFamilyInstance(startCon.Origin.Add(new XYZ(0, 0, -level.Elevation)), fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        //            fiConnector1 = fi.GetConnectorByDirection(new XYZ(-1, 0, 0));
        //            fiConnector2 = fi.GetConnectorByDirection(new XYZ(1, 0, 0));
        //            #endregion

        //            #region 旋转
        //            var rotateAngle = fiConnector1.CoordinateSystem.BasisZ.AngleTo_WithSign(-startCon.CoordinateSystem.BasisZ);
        //            var normal = fiConnector1.CoordinateSystem.BasisZ.GetNormal(-startCon.CoordinateSystem.BasisZ);

        //            //过渡件特有的计算水平垂直偏移的值
        //            var mepOri = Transform.CreateRotationAtPoint(normal, Math.Abs(rotateAngle), startCon.Origin).Inverse.OfPoint(mepCon.Origin);
        //            xDis = mepOri.Y - fiConnector2.Origin.Y;
        //            yDis = mepOri.Z - fiConnector2.Origin.Z;

        //            ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(startCon.Origin, startCon.Origin + normal), Math.Abs(rotateAngle));
        //            #endregion



        //            #region 修改连接点大小
        //            //第一个点
        //            fiConnector1.SetWidth(startCon);
        //            if ((fiConnector1.Shape == ConnectorProfileType.Rectangular || fiConnector1.Shape == ConnectorProfileType.Oval))//天方地圆
        //            {
        //                if (!startCon.CoordinateSystem.BasisX.IsAlmostEqualTo(fiConnector1.CoordinateSystem.BasisX) && !startCon.CoordinateSystem.BasisY.IsAlmostEqualTo(fiConnector1.CoordinateSystem.BasisY))
        //                    ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(fiConnector1.Origin, fiConnector2.Origin), Math.PI / 2);

        //                fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_WIDTH).Set(startCon.Width / 2 + xDis);
        //                fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_HEIGHT).Set(startCon.Height / 2 + yDis);
        //            }
        //            else
        //            {
        //                fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_WIDTH).Set(startCon.Radius + xDis);
        //                fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_HEIGHT).Set(startCon.Radius + yDis);
        //            }

        //            //第二个点
        //            if ((fiConnector2.Shape == ConnectorProfileType.Rectangular || fiConnector2.Shape == ConnectorProfileType.Oval))
        //            {
        //                if (!endCon.CoordinateSystem.BasisX.IsAlmostEqualTo(fiConnector2.CoordinateSystem.BasisX) && !endCon.CoordinateSystem.BasisY.IsAlmostEqualTo(fiConnector2.CoordinateSystem.BasisY))
        //                {
        //                    fiConnector2.Width = endCon.Height;
        //                    fiConnector2.Height = endCon.Width;
        //                }
        //                else
        //                {
        //                    fiConnector2.Width = endCon.Width;
        //                    fiConnector2.Height = endCon.Height;
        //                }
        //            }
        //            else
        //                fiConnector2.SetWidth(endCon);
        //            #endregion

        //            #region 连接
        //            this.RevitDoc.Regenerate();
        //            fiConnector1.ConnectTo(startCon);
        //            endCon.Origin = fiConnector2.Origin;
        //            fiConnector2.ConnectTo(endCon);
        //            #endregion
        //            return fi;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        base.log.AddLog(ex);
        //        throw ex;
        //    }
        //    finally
        //    {
        //        fi.WriteParm((mepCon.Owner as MEPCurve).GetParmType(), true);
        //    }
        //}
        public FamilyInstance NewDuctThreeFitting(Duct src, Duct dst1, Duct dst2)
        {
            FamilyInstance fi = null;
            FamilySymbol fs = null;
            DuctConnectTypeENUM ductConnectType = DuctConnectTypeENUM.Unknow;
            Connector fiConnector1 = null, fiConnector2 = null, fiConnector3 = null, maxHeightCon = null, maxWidthCon = null;
            Connector startCon = null, endCon1 = null, endCon2 = null;
            XYZ intersectWithPoint = null;
            MEPCurveTeeFilterTypeEnum teeType = MEPCurveTeeFilterTypeEnum.Normal;
            double angle1 = 0, angle2 = 0, rotateAngle = 0;

            try
            {
                #region 先初步拿个要用于连接的Connector,同时确定maxHeightCon和maxWidthCon
                startCon = src.GetClosestConnector(dst1);
                endCon1 = dst1.GetClosestConnector(dst2);
                endCon2 = dst2.GetClosestConnector(src);
                //确定maxHeightCon
                maxHeightCon = startCon;
                var maxHeight = startCon.GetHeight();
                if (endCon1.GetHeight().IsGreaterThan(maxHeight))
                {
                    maxHeightCon = endCon1;
                    maxHeight = endCon1.GetHeight();
                }
                if (endCon2.GetHeight().IsGreaterThan(maxHeight))
                {
                    maxHeightCon = endCon2;
                    maxHeight = endCon2.GetHeight();
                }
                //确定maxWidthCon
                if (src.IsParallelTo(dst1))
                {
                    maxWidthCon = startCon;
                    if (endCon1.GetWidth().IsGreaterThan(maxWidthCon.GetWidth()))
                        maxWidthCon = endCon1;
                }
                else if (src.IsParallelTo(dst2))
                {
                    maxWidthCon = startCon;
                    if (endCon2.GetWidth().IsGreaterThan(maxWidthCon.GetWidth()))
                        maxWidthCon = endCon2;
                }
                else
                {
                    maxWidthCon = endCon1;
                    if (endCon2.GetWidth().IsGreaterThan(maxWidthCon.GetWidth()))
                        maxWidthCon = endCon2;
                }
                #endregion

                #region 判断风管形状是否相同,不相同则return null
                if (startCon.Shape != endCon1.Shape ||
                    startCon.Shape != endCon2.Shape ||
                    endCon1.Shape != endCon2.Shape)
                    return null;
                #endregion

                #region 判断风管连接类型DuctConnectTypeENUM,并确定fs
                if (startCon.Shape == ConnectorProfileType.Round)
                {
                    var line = Line.CreateUnbound(maxHeightCon.Origin, maxHeightCon.CoordinateSystem.BasisZ);
                    var line1 = Line.CreateUnbound(startCon.Origin + new XYZ(0, 0, -startCon.Origin.Z + maxHeightCon.Origin.Z), startCon.CoordinateSystem.BasisZ);
                    var line2 = Line.CreateUnbound(endCon1.Origin + new XYZ(0, 0, -endCon1.Origin.Z + maxHeightCon.Origin.Z), endCon1.CoordinateSystem.BasisZ);
                    var line3 = Line.CreateUnbound(endCon2.Origin + new XYZ(0, 0, -endCon2.Origin.Z + maxHeightCon.Origin.Z), endCon2.CoordinateSystem.BasisZ);
                    if (!line.Intersect_PM(line1, out intersectWithPoint))
                        if (!line.Intersect_PM(line2, out intersectWithPoint))
                            line.Intersect_PM(line3, out intersectWithPoint);
                    ductConnectType = DuctConnectTypeENUM.Round;
                }
                else
                {
                    if (startCon.Origin.Z.IsAlmostEqualTo(endCon1.Origin.Z) && startCon.Origin.Z.IsAlmostEqualTo(endCon2.Origin.Z))
                    {
                        var line = Line.CreateUnbound(maxWidthCon.Origin, maxWidthCon.CoordinateSystem.BasisZ);
                        var line1 = Line.CreateUnbound(startCon.Origin, startCon.CoordinateSystem.BasisZ);
                        var line2 = Line.CreateUnbound(endCon1.Origin, endCon1.CoordinateSystem.BasisZ);
                        var line3 = Line.CreateUnbound(endCon2.Origin, endCon2.CoordinateSystem.BasisZ);

                        if (!line.Intersect_PM(line1, out intersectWithPoint))
                            if (!line.Intersect_PM(line2, out intersectWithPoint))
                                line.Intersect_PM(line3, out intersectWithPoint);
                        intersectWithPoint = src.GetIntersectWithPointLineExpandTwo(dst1) != null ? src.GetIntersectWithPointLineExpandTwo(dst1) : src.GetIntersectWithPointLineExpandTwo(dst2);
                        ductConnectType = DuctConnectTypeENUM.MiddleRect;
                    }
                    else if ((startCon.Origin.Z - startCon.Height / 2).IsAlmostEqualTo(endCon1.Origin.Z - endCon1.Height / 2) && (startCon.Origin.Z - startCon.Height / 2).IsAlmostEqualTo(endCon2.Origin.Z - endCon2.Height / 2))
                    {
                        var line = Line.CreateUnbound(maxWidthCon.Origin - new XYZ(0, 0, maxWidthCon.Height / 2), maxWidthCon.CoordinateSystem.BasisZ);
                        var line1 = Line.CreateUnbound(startCon.Origin - new XYZ(0, 0, startCon.Height / 2), startCon.CoordinateSystem.BasisZ);
                        var line2 = Line.CreateUnbound(endCon1.Origin - new XYZ(0, 0, endCon1.Height / 2), endCon1.CoordinateSystem.BasisZ);
                        var line3 = Line.CreateUnbound(endCon2.Origin - new XYZ(0, 0, endCon2.Height / 2), endCon2.CoordinateSystem.BasisZ);

                        if (!line.Intersect_PM(line1, out intersectWithPoint))
                            if (!line.Intersect_PM(line2, out intersectWithPoint))
                                line.Intersect_PM(line3, out intersectWithPoint);
                        ductConnectType = DuctConnectTypeENUM.BottomRect;
                    }
                    else if ((startCon.Origin.Z + startCon.Height / 2).IsAlmostEqualTo(endCon1.Origin.Z + endCon1.Height / 2) && (startCon.Origin.Z + startCon.Height / 2).IsAlmostEqualTo(endCon2.Origin.Z + endCon2.Height / 2))
                    {
                        var line = Line.CreateUnbound(maxWidthCon.Origin - new XYZ(0, 0, maxWidthCon.Height / 2), maxWidthCon.CoordinateSystem.BasisZ);
                        var line1 = Line.CreateUnbound(startCon.Origin + new XYZ(0, 0, startCon.Height / 2), startCon.CoordinateSystem.BasisZ);
                        var line2 = Line.CreateUnbound(endCon1.Origin + new XYZ(0, 0, endCon1.Height / 2), endCon1.CoordinateSystem.BasisZ);
                        var line3 = Line.CreateUnbound(endCon2.Origin + new XYZ(0, 0, endCon2.Height / 2), endCon2.CoordinateSystem.BasisZ);

                        if (!line.Intersect_PM(line1, out intersectWithPoint))
                            if (!line.Intersect_PM(line2, out intersectWithPoint))
                                line.Intersect_PM(line3, out intersectWithPoint);
                        ductConnectType = DuctConnectTypeENUM.BottomRect;
                    }
                    else
                        return null;
                }
                fs = FamilyLoadUtils.FindFamilySymbol_SubTransaction(this.RevitDoc, this.teeDic[ductConnectType].Split(':')[0], this.teeDic[ductConnectType].Split(':')[1]);
                #endregion

                #region 创建管件,修改连接点宽度
                var level = this.RevitDoc.GetElement(src.GetParameters("参照标高").First().AsElementId()) as Level;
                fi = this.RevitDoc.Create.NewFamilyInstance(intersectWithPoint + new XYZ(0, 0, -intersectWithPoint.Z + maxHeightCon.Origin.Z - level.Elevation), fs, level, 0);
                var type = fi.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString();
                fiConnector1 = fi.GetConnectorByDirection(new XYZ(-1, 0, 0));
                if (type.Contains("T 形三通"))
                {
                    if (fi.GetConnectorByDirection(new XYZ(1, 0, 0)) != null)
                    {
                        teeType = MEPCurveTeeFilterTypeEnum.Normal;
                        fiConnector2 = fi.GetConnectorByDirection(new XYZ(1, 0, 0));
                        fiConnector3 = fi.GetTeeThirdConnector();
                    }
                    else if (fi.GetConnectorByDirection(new XYZ(0, 1, 0)) != null)
                    {
                        teeType = MEPCurveTeeFilterTypeEnum.T;
                        fiConnector2 = fi.GetConnectorByDirection(new XYZ(0, 1, 0));
                        fiConnector3 = fi.GetConnectorByDirection(new XYZ(0, -1, 0));
                    }
                    else
                    {
                        teeType = MEPCurveTeeFilterTypeEnum.Y;
                        fiConnector2 = fi.GetTeeSecondConnector();
                        fiConnector3 = fi.GetTeeThirdConnector();
                    }
                }
                else if (type.Contains("Y 形三通"))
                {
                    teeType = MEPCurveTeeFilterTypeEnum.Y;
                    fiConnector2 = fi.GetTeeSecondConnector();
                    fiConnector3 = fi.GetTeeThirdConnector();
                }
                else
                {
                    throw new Exception("管件的类型出错，应为T 形三通或者Y 形三通");
                }
                #endregion

                #region 确定连接点，以及相交点
                if (dst1 == dst2)//这种只会出现teeType=Normal的情况
                {
                    MEPCurve copyCurve = dst1.GetCopy(this.RevitDoc);
                    startCon = dst1.ConnectorManager.Connectors.GetConnectorById(0);
                    endCon1 = copyCurve.ConnectorManager.Connectors.GetConnectorById(1);
                    endCon2 = src.GetClosestConnector(src.GetIntersectWithPointLineExpandTwo(dst1));

                    startCon.ChangeLinkedConnectorTo(copyCurve.ConnectorManager.Connectors.GetConnectorById(startCon.Id));

                    startCon.Origin = dst1.GetClosestPoint(intersectWithPoint);
                    endCon1.Origin = copyCurve.GetClosestPoint(intersectWithPoint);
                    endCon2.Origin = src.GetClosestPoint(intersectWithPoint);

                    this.RevitDoc.Regenerate();
                }
                else
                {
                    if (teeType == MEPCurveTeeFilterTypeEnum.Normal)
                    {
                        if (src.IsParallelTo(dst1))
                        {
                            startCon = src.GetClosestConnector(dst2);
                            endCon1 = dst1.GetClosestConnector(startCon.Origin);
                            endCon2 = dst2.GetClosestConnector(startCon.Origin);
                        }
                        else if (src.IsParallelTo(dst2))
                        {
                            startCon = src.GetClosestConnector(dst1);
                            endCon1 = dst2.GetClosestConnector(startCon.Origin);
                            endCon2 = dst1.GetClosestConnector(startCon.Origin);
                        }
                        else if (dst1.IsParallelTo(dst2))
                        {
                            startCon = dst1.GetClosestConnector(src);
                            endCon1 = dst2.GetClosestConnector(startCon.Origin);
                            endCon2 = src.GetClosestConnector(startCon.Origin);
                        }
                        else
                        {
                            this.RevitDoc.Delete(fi.Id);
                            throw new Exception("当前位置无法用此连接方式连接,需要有两个管道在同一直线上");
                        }
                    }
                    else if (teeType == MEPCurveTeeFilterTypeEnum.T)
                    {
                        if (src.IsParallelTo(dst1))
                        {
                            startCon = dst2.GetClosestConnector(src);
                            endCon1 = src.GetClosestConnector(startCon.Origin);
                            endCon2 = dst1.GetClosestConnector(startCon.Origin);
                        }
                        else if (src.IsParallelTo(dst2))
                        {
                            startCon = dst1.GetClosestConnector(src);
                            endCon1 = src.GetClosestConnector(startCon.Origin);
                            endCon2 = dst2.GetClosestConnector(startCon.Origin);
                        }
                        else if (dst1.IsParallelTo(dst2))
                        {
                            startCon = src.GetClosestConnector(dst2);
                            endCon1 = dst1.GetClosestConnector(startCon.Origin);
                            endCon2 = dst2.GetClosestConnector(startCon.Origin);
                        }
                        else
                        {
                            this.RevitDoc.Delete(fi.Id);
                            throw new Exception("当前位置无法用此连接方式连接,需要有两个管道在同一直线上");
                        }
                    }
                    else
                    {
                        startCon = src.GetClosestConnector(dst1);
                        endCon1 = dst1.GetClosestConnector(startCon.Origin);
                        endCon2 = dst2.GetClosestConnector(startCon.Origin);

                        double a1 = startCon.CoordinateSystem.BasisZ.AngleTo(endCon1.CoordinateSystem.BasisZ);
                        double a2 = startCon.CoordinateSystem.BasisZ.AngleTo(endCon2.CoordinateSystem.BasisZ);
                        if (a1 > Math.PI * 15 / 18 || a1 < Math.PI * 12 / 18)
                            throw new Exception("第一根风管和第二根风管的夹角请在120°到150°之间");

                        if (a2 > Math.PI * 15 / 18 || a2 < Math.PI * 12 / 18)
                            throw new Exception("第一根风管和第三根风管的夹角请在120°到150°之间");
                    }
                }
                JudgeAndThrow(new Connector[] { startCon, endCon1, endCon2 }, fs, src);
                #endregion

                #region 旋转
                //先将三通管件的起点对正，起点方向是(-1,0,0)
                var normal = (fiConnector1.CoordinateSystem.BasisZ.IsAlmostEqualTo(startCon.CoordinateSystem.BasisZ) || fiConnector1.CoordinateSystem.BasisZ.IsAlmostEqualTo(-startCon.CoordinateSystem.BasisZ)) ? XYZ.BasisZ : fiConnector1.CoordinateSystem.BasisZ.GetNormal(startCon.CoordinateSystem.BasisZ);
                rotateAngle = fiConnector1.CoordinateSystem.BasisZ.AngleTo_PM(-startCon.CoordinateSystem.BasisZ, normal);
                ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(intersectWithPoint, intersectWithPoint + normal), rotateAngle);
                this.RevitDoc.Regenerate();

                //在起点对正后，以变换后的起点方向为法线向量，对正第三个点方向
                if (fiConnector3.CoordinateSystem.BasisZ.IsAlmostEqualTo_PM(endCon2.CoordinateSystem.BasisZ))//这种情况下的桥架特殊处理
                {
                    var temp = startCon;
                    startCon = endCon1;
                    endCon1 = temp;
                    ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(intersectWithPoint, intersectWithPoint + normal), Math.PI);
                    this.RevitDoc.Regenerate();
                }
                else
                {
                    rotateAngle = fiConnector3.CoordinateSystem.BasisZ.AngleTo_WithSign(-endCon2.CoordinateSystem.BasisZ, fiConnector1.CoordinateSystem.BasisZ);
                    var normal1 = fiConnector1.CoordinateSystem.BasisZ;
                    ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(intersectWithPoint, intersectWithPoint + normal1), Math.Abs(rotateAngle));
                    this.RevitDoc.Regenerate();
                }
                #endregion

                #region 设置角度
                if (teeType == MEPCurveTeeFilterTypeEnum.Normal)
                {
                    angle2 = fiConnector1.CoordinateSystem.BasisZ.AngleTo(endCon2.CoordinateSystem.BasisZ);
                    if (!angle2.IsAlmostEqualTo(0))
                    {
                        var angles = fi.GetParameters("角度");
                        if (angles.Count != 0)
                            angles[0].Set(angle2);

                    }
                }
                else if (teeType == MEPCurveTeeFilterTypeEnum.Y)
                {
                    angle1 = fiConnector1.CoordinateSystem.BasisZ.AngleTo(endCon1.CoordinateSystem.BasisZ);
                    if (!angle1.IsAlmostEqualTo(0))
                    {
                        var angles = fi.GetParameters("出口角度");
                        if (angles.Count != 0)
                            angles[0].Set(angle1);
                    }

                    angle2 = fiConnector1.CoordinateSystem.BasisZ.AngleTo(endCon2.CoordinateSystem.BasisZ);
                    if (!angle2.IsAlmostEqualTo(0))
                    {
                        var angles = fi.GetParameters("支管角度");
                        if (angles.Count != 0)
                            angles[0].Set(angle2);
                    }
                }
                this.RevitDoc.Regenerate();
                #endregion

                #region 创建连接管件
                if (ductConnectType == DuctConnectTypeENUM.BottomRect || ductConnectType == DuctConnectTypeENUM.TopRect)
                {
                    var con = (startCon.Owner as MEPCurve).GetWidth() > (endCon1.Owner as MEPCurve).GetWidth() ? startCon : endCon1;
                    fiConnector1.SetWidth(con); this.RevitDoc.Regenerate();
                    fiConnector2.SetWidth(con); this.RevitDoc.Regenerate();
                    fiConnector3.SetWidth(endCon2);
                }
                else
                {
                    if ((startCon.Owner as MEPCurve).GetWidth() < (endCon1.Owner as MEPCurve).GetWidth())
                    {
                        if ((endCon1.Owner as MEPCurve).GetWidth() < (endCon2.Owner as MEPCurve).GetWidth())
                        {
                            fiConnector1.SetWidth(startCon); this.RevitDoc.Regenerate();
                            fiConnector2.SetWidth(endCon1); this.RevitDoc.Regenerate();
                            fiConnector3.SetWidth(endCon2);
                        }
                        else
                        {
                            fiConnector1.SetWidth(startCon); this.RevitDoc.Regenerate();
                            fiConnector3.SetWidth(endCon2); this.RevitDoc.Regenerate();
                            fiConnector2.SetWidth(endCon1);
                        }
                    }
                    else
                    {
                        if ((startCon.Owner as MEPCurve).GetWidth() < (endCon2.Owner as MEPCurve).GetWidth())
                        {
                            fiConnector2.SetWidth(endCon1); this.RevitDoc.Regenerate();
                            fiConnector1.SetWidth(startCon); this.RevitDoc.Regenerate();
                            fiConnector3.SetWidth(endCon2);
                        }
                        else
                        {
                            fiConnector3.SetWidth(endCon2); this.RevitDoc.Regenerate();
                            fiConnector2.SetWidth(endCon1); this.RevitDoc.Regenerate();
                            fiConnector1.SetWidth(startCon);
                        }
                    }
                }
                this.RevitDoc.Regenerate();
                this.NewTransitionPM(fiConnector1, startCon, null);
                this.NewTransitionPM(fiConnector2, endCon1, null);
                this.NewTransitionPM(fiConnector3, endCon2, null);
                this.RevitDoc.Regenerate();
                #endregion

                return fi;
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionPM(ex, src, MEPCurveConnectTypeENUM.Tee);
                return fi;
            }
            finally
            {
                fi.WriteParm(src.GetParmType(), true);
                fi.WriteParm_ConnectedFitting(src.GetParmType());
            }
        }
        #endregion

        #region 桥架连接处理(protected)

        /// <summary>
        /// 不同高度延长后相交的桥架连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        protected void NewTwo_CableTray_UnSameXYPlane_Disjoint(MEPCurve src, MEPCurve dst)
        {
            double minDistance = UnitTransUtils.MMToFeet(1200); // 生成竖管与三通（弯头）最小距离
            double minThreeDistance = UnitTransUtils.MMToFeet(500); // 边缘生成三通最小距离

            MEPCurve cTwo = null, cThree = null;
            if (dst.IsIntersectWithByLineExpand_XYPlane_ChangeToSamePlane(src))
            {
                cTwo = dst;
                cThree = src;
            }
            else
            {
                cTwo = src;
                cThree = dst;
            }

            // 获得垂直点坐标
            Connector pCon = cTwo.GetClosestConnector(cThree);
            XYZ pPoint = new XYZ(pCon.Origin.X, pCon.Origin.Y, cThree.GetLine().GetClosestPoint(pCon.Origin).Z);
            // 获得交点坐标
            XYZ iPoint = cTwo.GetIntersectWithPointLineExpandTwo_XYPlane_ChangeToSamePlane(cThree);
            iPoint = new XYZ(iPoint.X, iPoint.Y, cThree.GetLine().GetClosestPoint(iPoint).Z);

            // 判断垂直点距离是否满足生成条件，不满足则移动
            if (pPoint.DistanceTo(iPoint) < minDistance)
            {
                XYZ direction = cTwo.GetLine().Direction;
                direction = new XYZ(direction.X, direction.Y, 0);
                pPoint = pPoint.Add(pCon.Id == 0 ? direction * (minDistance - pPoint.DistanceTo(iPoint)) : -direction * (minDistance - pPoint.DistanceTo(iPoint)));
                pCon.Origin = new XYZ(pPoint.X, pPoint.Y, pCon.Origin.Z);
            }

            // 生成连接桥架
            MEPCurve iMep = src.GetWidth() > dst.GetWidth() ? src.GetCopy(RevitDoc) : dst.GetCopy(RevitDoc);
            (iMep.Location as LocationCurve).Curve = Line.CreateBound(pPoint, iPoint);

            // 生成垂直桥架
            MEPCurve pMep = src.GetWidth() > dst.GetWidth() ? src.GetCopy(RevitDoc) : dst.GetCopy(RevitDoc);
            (pMep.Location as LocationCurve).Curve = Line.CreateBound(new XYZ(pPoint.X, pPoint.Y, cTwo.GetClosestConnector(cThree).Origin.Z), pPoint);

            // 设置垂直桥架的顶部方向
            (pMep as CableTray).CurveNormal = iMep.GetLine().Direction;

            try
            {
                this.NewTwoFitting(cTwo, pMep, null);
                this.NewTwoFitting(pMep, iMep, null);

                if (iPoint.DistanceTo(cThree.GetClosestConnector(iPoint).Origin) < minThreeDistance)
                {
                    this.NewElbowDefault(iMep, cThree, null);
                }
                else
                {
                    this.NewTwoFitting(iMep, cThree, null);
                }
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region 较高级连接方法(protected)
        /// <summary>
        /// 连接在同一平面上的两根管道
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        protected FamilyInstance NewTwoIntersectWith(MEPCurve src, MEPCurve dst)
        {
            if (src.IsColinearTo(dst))
            {
                return this.NewTransitionDefault(dst, src, null);
            }
            else if (src.IsIntersectWith(dst))
            {
                return this.NewTwoOverlap(src, dst);
            }
            else
            {
                return this.NewTwoDisjoint(src, dst);
            }
        }

        /// <summary>
        /// 连接相交的两根管道
        /// 连接方式根据情况区分
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance NewTwoOverlap(MEPCurve src, MEPCurve dst)
        {
            double dis1 = src.GetDistanceFromIntersectWithPoint(dst);
            double dis2 = dst.GetDistanceFromIntersectWithPoint(src);
            if (dis1 < src.GetWidth() && dis2 < src.GetWidth())
            {
                return NewElbowDefault(src, dst, null);
            }
            else if (dis1 < src.GetWidth())
            {
                return NewTeeByTwoMEP(dst, src);
            }
            else if (dis2 < src.GetWidth())
            {
                return NewTeeByTwoMEP(src, dst);
            }
            else
            {
                return NewCrossByTwoMEP(src, dst);
            }
        }

        /// <summary>
        /// 连接不相交的两根管道
        /// 连接方式根据情况区分
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance NewTwoDisjoint(MEPCurve src, MEPCurve dst)
        {
            if (src is CableTray && !src.IsPerpendicularTo(dst))
            {
                var xyz = src.GetIntersectWithPointLineExpandTwo(dst);
                if (src.IsOn(xyz) || dst.IsOn(xyz))
                {
                    var temp = src.IsOn(xyz) ? src : dst;
                    var next = !src.IsOn(xyz) ? src : dst;
                    if (Line.CreateBound(xyz, temp.ConnectorManager.Lookup(0).Origin).Direction.AngleTo(Line.CreateBound(xyz, next.ConnectorManager.Lookup(0).Origin).Direction) > Math.PI / 2)
                        temp.ConnectorManager.Lookup(1).Origin = xyz;
                    else
                        temp.ConnectorManager.Lookup(0).Origin = xyz;
                    this.RevitDoc.Regenerate();
                }
                else
                {
                    src.GetClosestConnector(xyz).Origin = xyz;
                    dst.GetClosestConnector(xyz).Origin = xyz;
                }
                return NewElbowDefault(src, dst, null);
            }

            //----------------
            //
            //       |    
            //       |   
            //       |   
            if (src.IsIntersectWithByLineExpand(dst))
            {
                if (dst.GetDistanceFromIntersectWithPoint(src) < RevitCommon.PipeFittingPlaceMinDis)
                    return NewElbowDefault(src, dst, null);
                else
                    return NewTeeByTwoMEP(dst, src);
            }
            //                  |
            //                  |
            //-------------     |
            //                  |
            //                  |
            else if (dst.IsIntersectWithByLineExpand(src))
            {
                if (src.GetDistanceFromIntersectWithPoint(dst) < RevitCommon.PipeFittingPlaceMinDis)
                    return NewElbowDefault(src, dst, null);
                else
                    return NewTeeByTwoMEP(src, dst);
            }
            //-------------    
            //
            //                 |
            //                 |
            //                 |
            else
                return NewElbowDefault(src, dst, null);
        }

        /// <summary>
        /// 连接不同平面的横管(放置到同一平面后,横管相交)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        protected void NewTwo_UnSameXYPlane_Overlap(MEPCurve src, MEPCurve dst)
        {
            MEPCurve copyCurve = null;
            double dis = 0;

            //创建立管
            XYZ xyz1 = src.GetIntersectWithPoint_XYPlane_ChangeToSamePlane(dst);
            XYZ xyz2 = dst.GetIntersectWithPoint_XYPlane_ChangeToSamePlane(src);
            copyCurve = src.GetCopy((src.GetWidth() < dst.GetWidth() ? src.GetWidth() / 2 : dst.GetWidth() / 2), xyz1, xyz2, 0);

            //根据距离判断是要连接三通还是弯曲
            dis = src.GetDistanceFromIntersectWithPoint(copyCurve);
            if (dis < RevitCommon.PipeFittingPlaceMinDis)
                this.NewElbowDefault(src, copyCurve, null);
            else
                this.NewTeeByTwoMEP(src, copyCurve);

            dis = dst.GetDistanceFromIntersectWithPoint(copyCurve);
            if (dis < RevitCommon.PipeFittingPlaceMinDis)
                this.NewElbowDefault(dst, copyCurve, null);
            else
                this.NewTeeByTwoMEP(dst, copyCurve);
        }

        /// <summary>
        /// 连接不同平面的横管(放置到同一平面后,横管延长线上相交)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="doc"></param>
        protected void NewTwo_UnSameXYPlane_Disjoint(MEPCurve src, MEPCurve dst)
        {
            MEPCurve cTwo = null, cThree = null;
            if (dst.IsIntersectWithByLineExpand_XYPlane_ChangeToSamePlane(src))
            {
                cTwo = dst;
                cThree = src;
            }
            else
            {
                cTwo = src;
                cThree = dst;
            }


            //复制创建管道
            XYZ xyz1 = cTwo.GetIntersectWithPointLineExpandTwo_XYPlane_ChangeToSamePlane(cThree);
            XYZ xyz2 = xyz1.Add(new XYZ(0, 0, cThree.ConnectorManager.Connectors.GetConnectorById(0).Origin.Z - xyz1.Z));
            MEPCurve copyCurve = cTwo.GetCopy((src.GetWidth() < dst.GetWidth() ? src.GetWidth() / 2 : dst.GetWidth() / 2), xyz1, xyz2, 0);

            //连接
            if (cThree.IsOn(cThree.GetIntersectWithPointLineExpandTwo(copyCurve)))
            {
                double distance = cThree.GetDistanceFromIntersectWithPoint(copyCurve);
                if (distance < RevitCommon.PipeFittingPlaceMinDis)
                    this.NewElbowDefault(cThree, copyCurve, null);
                else
                    this.NewTeeByTwoMEP(cThree, copyCurve);
            }
            else
                this.NewElbowDefault(cThree, copyCurve, null);

            this.NewElbowDefault(cTwo, copyCurve, null);
        }

        /// <summary>
        /// 连接不同平面的横管(横管平行)
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        protected void NewTwo_UnSameXYPlane_Parallel(MEPCurve src, MEPCurve dst)
        {
            Connector tempCon = dst.ConnectorManager.Lookup(0);
            XYZ closestXYZ = src.GetClosestPoint(tempCon.Origin);
            if (tempCon.Origin.IsAlmostEqualTo_PM(closestXYZ + new XYZ(0, 0, tempCon.Origin.Z - closestXYZ.Z)))
            {
                this.NewElbowGoBackDefault(src, dst, null, this.RevitDoc, 90);
                return;
            }

            Connector connector1 = null, connector2 = null;
            MEPCurve addedCurve1, addedCurve2;

            connector1 = src.GetClosestConnector(dst.GetMiddleXYZ());
            connector2 = dst.GetClosestConnector(connector1.Origin);

            this.JudgeAndThrow(new Connector[] { connector1, connector2 }, null, src);

            //复制创建两条管道
            addedCurve1 = src.GetCopy(this.RevitDoc);
            addedCurve2 = src.GetCopy(this.RevitDoc);

            var xyzSrc = src.GetClosestPoint(connector2.Origin);
            var xyzDst = dst.GetClosestPoint(connector1.Origin);
            if (dst.IsOn(xyzDst))
            {
                XYZ addP1 = dst.GetClosestPoint(connector1.Origin).Add(new XYZ(0, 0, connector1.Origin.Z - connector2.Origin.Z));
                XYZ addP2 = dst.GetClosestPoint(addP1);

                addedCurve1.ConnectorManager.Connectors.GetConnectorById(0).Origin = connector1.Origin;
                addedCurve1.ConnectorManager.Connectors.GetConnectorById(1).Origin = addP1;

                addedCurve2.ConnectorManager.Connectors.GetConnectorById(0).Origin = addP1;
                addedCurve2.ConnectorManager.Connectors.GetConnectorById(1).Origin = addP2;
            }
            else
            {
                XYZ addP1 = src.GetClosestPoint(connector2.Origin).Add(new XYZ(0, 0, connector2.Origin.Z - connector1.Origin.Z));
                XYZ addP2 = src.GetClosestPoint(addP1);

                addedCurve2.ConnectorManager.Connectors.GetConnectorById(0).Origin = connector2.Origin;
                addedCurve2.ConnectorManager.Connectors.GetConnectorById(1).Origin = addP1;

                addedCurve1.ConnectorManager.Connectors.GetConnectorById(0).Origin = addP1;
                addedCurve1.ConnectorManager.Connectors.GetConnectorById(1).Origin = addP2;
            }

            //连接
            if (src.IsOn(src.GetIntersectWithPointLineExpandTwo(addedCurve1)))
            {
                double distance = src.GetDistanceFromIntersectWithPoint(addedCurve1);
                if (distance < RevitCommon.PipeFittingPlaceMinDis)
                    this.NewElbowDefault(src, addedCurve1, null);
                else
                    this.NewTeeByTwoMEP(src, addedCurve1);
            }
            else
                this.NewElbowDefault(src, addedCurve1, null);

            if (dst.IsOn(dst.GetIntersectWithPointLineExpandTwo(addedCurve2)))
            {
                double distance = dst.GetDistanceFromIntersectWithPoint(addedCurve2);
                if (distance < RevitCommon.PipeFittingPlaceMinDis)
                    this.NewElbowDefault(dst, addedCurve2, null);
                else
                    this.NewTeeByTwoMEP(dst, addedCurve2);
            }
            else
                this.NewElbowDefault(src, addedCurve1, null);

            this.NewElbowDefault(addedCurve1, addedCurve2, null);
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        protected void NewTwo_UnSameXYPlane(MEPCurve src, MEPCurve dst)
        {
            if (src.IsParallelTo(dst))
            {
                this.NewTwo_UnSameXYPlane_Parallel(src, dst);
            }
            else if (src.IsIntersectWith_XYPlane_ChangeToSamePlane(dst))//两条管道相交
            {
                this.NewTwo_UnSameXYPlane_Overlap(src, dst);
            }
            else
            {
                this.NewTwo_UnSameXYPlane_Disjoint(src, dst);
            }
        }

        /// <summary>
        /// 连接一根横管和一根坡度管
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="doc"></param>
        protected void NewTwo_OneCrossAndOneStandOrSlope(MEPCurve src, MEPCurve dst)
        {
            MEPCurve crossMEP = null, standMEP = null;

            //确认横立管，判断相交情况
            crossMEP = src.IsCross() ? src : dst;
            standMEP = src.IsCross() ? dst : src;
            Line lineCross = crossMEP.GetLine();
            Line lineStand = standMEP.GetLine();

            var dis = lineCross.GetDistance(lineStand);

            //判断两根无限长直线上有无交点（即是否在同一平面）
            if (dis.IsAlmostEqualTo(0))
            {
                this.NewTwoIntersectWith(src, dst);
            }
            //比较近，需要移动坡度管
            else if (Math.Abs(dis) < ((src.GetWidth() < dst.GetWidth() ? dst.GetWidth() : src.GetWidth()) * 2))
            {
                XYZ intersectWithXYZ = null;
                Plane xyPlane = RevitVerUtil.CreateByNormalAndOrigin(new XYZ(0, 0, 1), crossMEP.ConnectorManager.Lookup(0).Origin); standMEP.GetLine().IsIntersectWith(xyPlane, out intersectWithXYZ);

                var crossXYZ = crossMEP.GetClosestPoint(intersectWithXYZ);
                var line1 = standMEP.GetLine(); line1.MakeUnbound();
                var line2 = Line.CreateBound(crossXYZ, crossXYZ + XYZ.BasisZ); line2.MakeUnbound();
                var standXYZ = line1.GetIntersectWithPoint(line2);

                ElementTransformUtils.MoveElement(this.RevitDoc, standMEP.Id, crossXYZ - standXYZ);
                this.RevitDoc.Regenerate();
                this.NewTwoIntersectWith(crossMEP, standMEP);
            }
            //在不同平面
            else
            {
                bool isIntersectWith = true;

                if (standMEP.IsStand())
                    isIntersectWith = false;
                else
                {
                    lineStand = Line.CreateBound(lineStand.GetEndPoint(0) + new XYZ(0, 0, lineCross.GetEndPoint(0).Z - lineStand.GetEndPoint(0).Z), lineStand.GetEndPoint(1) + new XYZ(0, 0, lineCross.GetEndPoint(0).Z - lineStand.GetEndPoint(1).Z));
                    if (lineCross.IsParallelTo(lineStand))
                        isIntersectWith = false;
                    else
                        isIntersectWith = true;
                }

                //两条管道转化为同一平面后平行,或者其中一根管道为立管
                if (!isIntersectWith)
                {
                    XYZ intersectWithXYZ = null, closestXYZ = null;
                    Plane xyPlane = RevitVerUtil.CreateByNormalAndOrigin(new XYZ(0, 0, 1), crossMEP.ConnectorManager.Lookup(0).Origin);
                    standMEP.GetLine().IsIntersectWith(xyPlane, out intersectWithXYZ);
                    closestXYZ = crossMEP.GetClosestPoint(intersectWithXYZ);

                    if (closestXYZ.DistanceTo(intersectWithXYZ) < RevitCommon.PipeFittingPlaceMinDis)
                    {
                        throw new Exception("无法连接：两者未在同一平面，但两者距离无法放置管件或管道");
                    }
                    else
                    {
                        MEPCurve copyCurve = crossMEP.GetCopy((src.GetWidth() < dst.GetWidth() ? src.GetWidth() / 2 : dst.GetWidth() / 2), intersectWithXYZ, closestXYZ, 0);
                        this.RevitDoc.Regenerate();

                        this.NewTwoIntersectWith(crossMEP, copyCurve);
                        this.NewTwoIntersectWith(standMEP, copyCurve);
                    }
                }
                //两条管道转化为同一平面后垂直
                else
                {
                    XYZ intersectWithXYZ = null, closestXYZ = null;
                    lineCross.MakeUnbound();
                    lineStand.MakeUnbound();
                    intersectWithXYZ = lineStand.GetIntersectWithPoint(lineCross);
                    var lineTemp1 = Line.CreateUnbound(intersectWithXYZ, XYZ.BasisZ);
                    var lineTemp2 = standMEP.GetLine(); lineTemp2.MakeUnbound();
                    closestXYZ = lineTemp1.GetIntersectWithPoint(lineTemp2);

                    MEPCurve copyCurve = crossMEP.GetCopy((src.GetWidth() < dst.GetWidth() ? src.GetWidth() / 2 : dst.GetWidth() / 2), intersectWithXYZ, closestXYZ, 0);
                    standMEP.GetClosestConnector(closestXYZ).Origin = closestXYZ;
                    this.RevitDoc.Regenerate();

                    this.NewTwoIntersectWith(crossMEP, copyCurve);
                    this.NewTwoIntersectWith(standMEP, copyCurve);
                }
            }
        }

        /// <summary>
        /// 连接一根立管和一根坡度管
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        protected void NewTwo_OneStandAndOneCrossOrSlope(MEPCurve src, MEPCurve dst)
        {
            Line lineSrc = src.GetLine(); lineSrc.MakeUnbound();
            Line lineDst = dst.GetLine(); lineDst.MakeUnbound();

            //判断两根无限长直线上有无交点（即是否在同一平面）
            if (lineSrc.Intersect_PM(lineDst))
            {
                this.NewTwoIntersectWith(src, dst);
            }
            //在不同平面
            else
            {
                XYZ standXYZ = null, crossXYZ = null;
                MEPCurve crossMEP = null, standMEP = null;
                double radius = (src.GetWidth() < dst.GetWidth() ? src.GetWidth() / 2 : dst.GetWidth() / 2);

                //确认横立管，判断相交情况
                crossMEP = src.IsCross() ? src : dst;
                standMEP = src.IsCross() ? dst : src;

                crossXYZ = crossMEP.GetLine().GetClosestPoint(standMEP.GetLine());
                standXYZ = standMEP.GetClosestPoint(crossXYZ);

                //两根管道很近
                if (crossXYZ.DistanceTo(standXYZ) < RevitCommon.PipeFittingPlaceMinDis)
                {
                    throw new Exception("无法连接：两者未在同一平面，但两者距离无法放置管件或管道");
                }

                MEPCurve copyCurve = crossMEP.GetCopy(radius, crossXYZ, standXYZ, 0);
                this.RevitDoc.Regenerate();

                this.NewTwoIntersectWith(crossMEP, copyCurve);
                this.NewTwoIntersectWith(standMEP, copyCurve);
            }
        }

        /// <summary>
        /// 连接两根通过延长或上下平移后相交的坡度管
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        protected void NewTwo_TwoSlope(MEPCurve src, MEPCurve dst)
        {
            XYZ closestPoint1 = src.GetLine().GetClosestPoint(dst.GetLine());
            XYZ closestPoint2 = dst.GetLine().GetClosestPoint(src.GetLine());
            MEPCurve longger = (src.GetWidth() > dst.GetWidth() ? src : dst);
            MEPCurve shorter = (longger == src ? dst : src);
            double maxDistance = longger.GetWidth() * 2 + shorter.GetWidth();

            if (closestPoint1.DistanceTo(closestPoint2) > maxDistance)
            {
                MEPCurve newPipe = shorter.GetCopy(UIApp.ActiveUIDocument.Document);
                (newPipe.Location as LocationCurve).Curve = Line.CreateBound(closestPoint1, closestPoint2);
                this.RevitDoc.Regenerate();

                this.NewTwoFitting(src, newPipe, null);
                this.NewTwoFitting(newPipe, dst, null);
            }
            else
            {
                // 得到xOy平面上的交点
                LocationCurve lCurve1 = src.Location as LocationCurve;
                LocationCurve lCurve2 = dst.Location as LocationCurve;

                XYZ xyz11 = lCurve1.Curve.GetEndPoint(0);
                XYZ xyz12 = lCurve1.Curve.GetEndPoint(1);
                XYZ xyz21 = lCurve2.Curve.GetEndPoint(0);
                XYZ xyz22 = lCurve2.Curve.GetEndPoint(1);

                Line line1 = Line.CreateBound(xyz11.Add(new XYZ(0, 0, -xyz11.Z)), xyz12.Add(new XYZ(0, 0, -xyz12.Z)));
                line1.MakeUnbound();
                Line line2 = Line.CreateBound(xyz21.Add(new XYZ(0, 0, -xyz21.Z)), xyz22.Add(new XYZ(0, 0, -xyz22.Z)));
                line2.MakeUnbound();

                var intersectPoint = line1.GetIntersectWithPoint(line2);

                // 获取src管道上的交点并判断交点是否在原管道上
                Line perpendicularLine1 = Line.CreateBound(new XYZ(intersectPoint.X, intersectPoint.Y, xyz11.Z), new XYZ(intersectPoint.X, intersectPoint.Y, xyz12.Z));
                perpendicularLine1.MakeUnbound();
                Line srcLine = (lCurve1.Curve.Clone()) as Line;
                srcLine.MakeUnbound();
                var intersectPoint1 = srcLine.GetIntersectWithPoint(perpendicularLine1);
                bool intersectPoint1OnLine1 = (lCurve1.Curve as Line).IsOn(intersectPoint1);

                // 获取dst管道上的交点并判断交点是否在原管道上
                Line perpendicularLine2 = Line.CreateBound(new XYZ(intersectPoint.X, intersectPoint.Y, xyz21.Z), new XYZ(intersectPoint.X, intersectPoint.Y, xyz22.Z));
                perpendicularLine2.MakeUnbound();
                Line dstLine = (lCurve2.Curve.Clone()) as Line;
                dstLine.MakeUnbound();
                var intersectPoint2 = dstLine.GetIntersectWithPoint(perpendicularLine2);
                bool intersectPoint2OnLine2 = (lCurve2.Curve as Line).IsOn(intersectPoint2);

                // 移动dst管道
                XYZ transformXYZ = new XYZ(0, 0, intersectPoint1.Z - intersectPoint2.Z);
                dst.Location.Move(transformXYZ);

                if (src.GetDistanceFromIntersectWithPoint(dst) < RevitCommon.PipeFittingPlaceMinDis)
                {
                    intersectPoint1OnLine1 = false;
                }
                if (dst.GetDistanceFromIntersectWithPoint(src) < RevitCommon.PipeFittingPlaceMinDis)
                {
                    intersectPoint2OnLine2 = false;
                }

                if (intersectPoint1OnLine1 == true && intersectPoint2OnLine2 == true)
                {
                    this.NewCrossByTwoMEP(src, dst);
                }
                else if (intersectPoint1OnLine1 == true && intersectPoint2OnLine2 != true)
                {
                    this.NewTeeByTwoMEP(src, dst);
                }
                else if (intersectPoint1OnLine1 != true && intersectPoint2OnLine2 == true)
                {
                    this.NewTeeByTwoMEP(dst, src);
                }
                else
                {
                    this.NewElbowDefault(src, dst, null);
                }
            }
        }

        /// <summary>
        /// 根据两根管道创建四通管件并连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance NewCrossByTwoMEP(MEPCurve src, MEPCurve dst)
        {
            if (src.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == "无配件的电缆桥架")
            {
                this.NewCross_CableTray(src, dst);
                return null;
            }

            this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Cross);

            Dictionary<MEPCurve, bool> dic = new Dictionary<MEPCurve, bool>();

            FamilyInstance fi = null;

            try
            {
                Connector c1 = null, c2 = null, c3 = null, c4 = null;
                Connector c1LinkedC = null, c3LinkedC = null;

                Connector temp = null;

                XYZ intersectWithPoint = src.GetIntersectWithPoint(dst);

                var copyCurve1 = src.GetCopy(src.Document);
                var copyCurve2 = dst.GetCopy(src.Document);
                src.Document.Regenerate();

                c1 = src.ConnectorManager.Connectors.GetConnectorById(0);
                c2 = copyCurve1.GetConnectorByDirection(-c1.CoordinateSystem.BasisZ);
                temp = dst.ConnectorManager.Connectors.GetConnectorById(0);
                c3 = dst.GetConnectorByDirection(c1.CoordinateSystem.BasisZ.RotateBy(temp.CoordinateSystem.BasisZ.GetNormal(c1.CoordinateSystem.BasisZ), -90));
                if (c3 == null)
                    throw new Exception("四通连接需要选择两两垂直的风管");
                c4 = copyCurve2.GetConnectorByDirection(-c3.CoordinateSystem.BasisZ);

                if ((c1LinkedC = c1.GetConnectedConnector()) != null)
                {
                    c1.DisconnectFrom(c1LinkedC);
                    copyCurve1.GetConnectorByDirection(c1.CoordinateSystem.BasisZ).ConnectTo(c1LinkedC);
                }

                if ((c3LinkedC = c3.GetConnectedConnector()) != null)
                {
                    c3.DisconnectFrom(c3LinkedC);
                    copyCurve2.GetConnectorByDirection(c3.CoordinateSystem.BasisZ).ConnectTo(c3LinkedC);
                }

                c1.Origin = intersectWithPoint;
                c2.Origin = intersectWithPoint;
                c3.Origin = intersectWithPoint;
                c4.Origin = intersectWithPoint;

                src.Document.Regenerate();

                List<Connector> cons = new List<Connector> { c1, c2, c3, c4 };
                cons.Sort((p1, p2) =>
                {
                    return p2.GetWidth().CompareTo(p1.GetWidth());
                });

                c1 = cons.First();
                cons.Remove(c1);
                c2 = cons.FirstOrDefault(p => p.CoordinateSystem.BasisZ.IsAlmostEqualTo_PM(-c1.CoordinateSystem.BasisZ));
                cons.Remove(c2);
                c3 = cons.First();
                c4 = cons.Last();

                try
                {
                    return fi = this.RevitDoc.Create.NewCrossFitting(c1, c2, c3, c4);
                }
                catch
                {
                    return fi = this.RevitDoc.Create.NewCrossFitting(c1, c3, c2, c4);
                }
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                throw new Exception("未能创建四通管件：" + ex.Message);
            }
            finally
            {
                fi.WriteParm(src.GetParmType(), false);
            }
        }

        /// <summary>
        /// 三通连接，根据两根管道进行连接
        /// 其中一根管道通过复制创建
        /// </summary>
        /// <param name="src">被复制的管道</param>
        /// <param name="dst"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance NewTeeByTwoMEP(MEPCurve src, MEPCurve dst)
        {
            FamilySymbol fs = null;
            if (fs == null)
                fs = this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Tee);
            if (fs == null)
                throw new Exception("未能加载到三通管件族");

            return NewTeePM(dst, src, src, fs);
        }
        #endregion

        #region 最基础连接方法(protected)
        /// <summary>
        /// 过度件连接
        /// FamilySymbol仅用作判断截面类型是否符合，并不修改族类型
        /// Defaut的意义：用doc.Create中的NewFitting相关函数创建管件并自动连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="fs"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance NewTransitionDefault(MEPCurve src, MEPCurve dst, FamilySymbol fs)
        {
            this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Transition);
            Dictionary<MEPCurve, bool> dic = new Dictionary<MEPCurve, bool>();

            FamilyInstance fi = null;
            try
            {
                var connector1 = src.GetClosestConnector(dst.GetMiddleXYZ());
                var connector2 = dst.GetClosestConnector(src.GetMiddleXYZ());

                JudgeAndThrow(new Connector[] { connector1, connector2 }, fs, src);

                this.StartSpecialCableTray(dic, new MEPCurve[] { src, dst });

                //管件大小相同、形状相同、共线时，直接变一根
                if (connector1.Shape == connector2.Shape && src.IsParallelTo(dst) &&
                    ((connector1.Shape == ConnectorProfileType.Round && connector1.Radius == connector2.Radius)
                    || (connector1.Shape == ConnectorProfileType.Rectangular && connector1.Width.IsAlmostEqualTo(connector2.Width) && connector1.Height.IsAlmostEqualTo(connector2.Height))))
                {
                    #region 两根变一根
                    connector2 = connector2.Id == dst.ConnectorManager.Connectors.GetConnectorById(0).Id ? dst.ConnectorManager.Connectors.GetConnectorById(1) : dst.ConnectorManager.Connectors.GetConnectorById(0);
                    var linkedConnector = connector2.GetConnectedConnector();

                    connector1.Origin = connector2.Origin;

                    this.RevitDoc.Delete(dst.Id);

                    if (linkedConnector != null)
                        connector1.ConnectTo(linkedConnector);
                    #endregion

                    return null;
                }
                else
                    return fi = this.RevitDoc.Create.NewTransitionFitting(connector1, connector2);
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionDefault(ex, src, MEPCurveConnectTypeENUM.Transition);
                return fi;
            }
            finally
            {
                fi.WriteParm(src.GetParmType(), false);
                fi.WriteParm_ConnectedFitting(src.GetParmType());
                this.EndSpecialCableTray(dic);
            }
        }
        /// <summary>
        /// 过度件连接
        /// FamilySymbol用于创建对应族实例
        /// PM的意义：通过doc.Create中的NewFamilyInstance函数创建管件，手动编写代码连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        protected FamilyInstance NewTransitionPM(MEPCurve src, MEPCurve dst, FamilySymbol fs)
        {
            if (fs == null)
                fs = this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Transition);
            if (fs == null)
                throw new Exception("未能加载到过渡管件族");

            FamilyInstance fi = null;

            try
            {
                var connector1 = src.GetClosestConnector(dst.GetMiddleXYZ());
                var connector2 = dst.GetClosestConnector(src.GetMiddleXYZ());

                //管件大小相同、形状相同、共线时，直接变一根
                if (connector1.Shape == connector2.Shape && src.IsParallelTo(dst) &&
                    ((connector1.Shape == ConnectorProfileType.Round && connector1.Radius == connector2.Radius)
                    || (connector1.Shape == ConnectorProfileType.Rectangular && connector1.Width.IsAlmostEqualTo(connector2.Width) && connector1.Height.IsAlmostEqualTo(connector2.Height))))
                {
                    #region 两根变一根
                    connector2 = connector2.Id == dst.ConnectorManager.Connectors.GetConnectorById(0).Id ? dst.ConnectorManager.Connectors.GetConnectorById(1) : dst.ConnectorManager.Connectors.GetConnectorById(0);
                    var linkedConnector = connector2.GetConnectedConnector();

                    connector1.Origin = connector2.Origin;

                    this.RevitDoc.Delete(dst.Id);

                    if (linkedConnector != null)
                        connector1.ConnectTo(linkedConnector);
                    #endregion

                    return null;
                }
                else
                {
                    Connector fiConnector1 = null, fiConnector2 = null;
                    Connector startCon = null, endCon = null;
                    #region 确定连接点
                    if (connector1.Shape == ConnectorProfileType.Rectangular)
                    {
                        startCon = connector1;
                        endCon = connector2;
                    }
                    else
                    {
                        startCon = connector2;
                        endCon = connector1;
                    }
                    #endregion

                    #region 创建过渡件
                    var level = this.RevitDoc.GetElement((startCon.Owner as MEPCurve).GetParameters("参照标高").First().AsElementId()) as Level;
                    fi = this.RevitDoc.Create.NewFamilyInstance(startCon.Origin.Add(new XYZ(0, 0, -level.Elevation)), fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    fiConnector1 = fi.GetConnectorByDirection(new XYZ(-1, 0, 0));
                    fiConnector2 = fi.GetConnectorByDirection(new XYZ(1, 0, 0));
                    #endregion

                    #region 旋转
                    var rotateAngle = fiConnector1.CoordinateSystem.BasisZ.AngleTo_WithSign(-startCon.CoordinateSystem.BasisZ);
                    var normal = fiConnector1.CoordinateSystem.BasisZ.GetNormal(-startCon.CoordinateSystem.BasisZ);
                    ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(startCon.Origin, startCon.Origin + normal), Math.Abs(rotateAngle));
                    #endregion

                    #region 修改连接点大小
                    //第一个点
                    fiConnector1.SetWidth(startCon);
                    if ((fiConnector1.Shape == ConnectorProfileType.Rectangular || fiConnector1.Shape == ConnectorProfileType.Oval))//天方地圆
                    {
                        if (!startCon.CoordinateSystem.BasisX.IsAlmostEqualTo(fiConnector1.CoordinateSystem.BasisX) && !startCon.CoordinateSystem.BasisY.IsAlmostEqualTo(fiConnector1.CoordinateSystem.BasisY))
                            ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(fiConnector1.Origin, fiConnector2.Origin), Math.PI / 2);

                        var p = fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_WIDTH);
                        if (p != null)
                            p.Set(startCon.Width / 2);
                        p = fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_HEIGHT);
                        if (p != null)
                            p.Set(startCon.Height / 2);
                    }
                    else
                    {
                        var p = fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_WIDTH);
                        if (p != null)
                            p.Set(startCon.Radius);
                        p = fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_HEIGHT);
                        if (p != null)
                            p.Set(startCon.Radius);
                    }

                    //第二个点
                    if ((fiConnector2.Shape == ConnectorProfileType.Rectangular || fiConnector2.Shape == ConnectorProfileType.Oval))
                    {
                        if (!endCon.CoordinateSystem.BasisX.IsAlmostEqualTo(fiConnector2.CoordinateSystem.BasisX) && !endCon.CoordinateSystem.BasisY.IsAlmostEqualTo(fiConnector2.CoordinateSystem.BasisY))
                        {
                            fiConnector2.Width = endCon.Height;
                            fiConnector2.Height = endCon.Width;
                        }
                        else
                        {
                            fiConnector2.Width = endCon.Width;
                            fiConnector2.Height = endCon.Height;
                        }
                    }
                    else
                        fiConnector2.SetWidth(endCon);
                    #endregion

                    #region 连接
                    this.RevitDoc.Regenerate();
                    startCon.Origin = fiConnector1.Origin;
                    fiConnector1.ConnectTo(startCon);
                    endCon.Origin = fiConnector2.Origin;
                    fiConnector2.ConnectTo(endCon);
                    #endregion
                    return fi;
                }
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionDefault(ex, src, MEPCurveConnectTypeENUM.Transition);
                return fi;
            }
            finally
            {
                fi.WriteParm(src.GetParmType(), false);
                fi.WriteParm_ConnectedFitting(src.GetParmType());
            }
        }
        /// <summary>
        /// 主要用于NewTeePM函数
        /// </summary>
        /// <param name="fiCon"></param>
        /// <param name="mepCon"></param>
        /// <param name="fs"></param>
        /// <param name="isAline"></param>
        /// <returns></returns>
        protected FamilyInstance NewTransitionPM(Connector fiCon, Connector mepCon, FamilySymbol fs, bool isAline = false)
        {
            if (fs == null)
                fs = this.Judge_LoadDefaultFitting(mepCon.Owner as MEPCurve, MEPCurveConnectTypeENUM.Transition);
            if (fs == null)
                throw new Exception("未能加载到过渡管件族");

            FamilyInstance fi = null;

            try
            {
                #region 对齐
                if (isAline)
                {
                    XYZ closestXYZ = (mepCon.Owner as MEPCurve).GetClosestPoint(fiCon.Origin);
                    mepCon.Origin = closestXYZ;
                    ElementTransformUtils.MoveElement(mepCon.Owner.Document, mepCon.Owner.Id, fiCon.Origin - mepCon.Origin);
                }
                #endregion

                //管件大小相同、形状相同时，连接
                if (Line.CreateBound(fiCon.Origin, fiCon.Origin + fiCon.CoordinateSystem.BasisZ).IsColinearTo(Line.CreateBound(mepCon.Origin, mepCon.Origin + mepCon.CoordinateSystem.BasisZ)) &&
                    ((fiCon.Shape == ConnectorProfileType.Round && mepCon.Shape == ConnectorProfileType.Round && fiCon.Radius.IsAlmostEqualTo(mepCon.Radius)) ||
                    (fiCon.Shape == ConnectorProfileType.Rectangular && mepCon.Shape == ConnectorProfileType.Rectangular && fiCon.Width.IsAlmostEqualTo(mepCon.Width) && fiCon.Height.IsAlmostEqualTo(mepCon.Height)) ||
                    (fiCon.Shape == ConnectorProfileType.Rectangular && mepCon.Shape == ConnectorProfileType.Rectangular && fiCon.Width.IsAlmostEqualTo(mepCon.Height) && fiCon.Height.IsAlmostEqualTo(mepCon.Width))))
                {
                    mepCon.Origin = fiCon.Origin;
                    fiCon.ConnectTo(mepCon);
                    return null;
                }
                else
                {
                    Connector fiConnector1 = null, fiConnector2 = null;
                    Connector startCon = fiCon, endCon = mepCon;
                    double xDis = 0, yDis = 0;

                    #region 创建过渡件
                    var level = this.RevitDoc.GetElement((mepCon.Owner as MEPCurve).GetParameters("参照标高").First().AsElementId()) as Level;
                    fi = this.RevitDoc.Create.NewFamilyInstance(startCon.Origin.Add(new XYZ(0, 0, -level.Elevation)), fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    fiConnector1 = fi.GetConnectorByDirection(new XYZ(-1, 0, 0));
                    fiConnector2 = fi.GetConnectorByDirection(new XYZ(1, 0, 0));
                    if (fiConnector1.Shape == ConnectorProfileType.Round)
                    {
                        fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_WIDTH).Set(fiConnector1.Radius);
                        fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_HEIGHT).Set(fiConnector1.Radius);
                    }
                    else
                    {
                        fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_WIDTH).Set(fiConnector1.Width / 2);
                        fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_HEIGHT).Set(fiConnector1.Height / 2);
                    }
                    this.RevitDoc.Regenerate();
                    #endregion

                    #region 旋转
                    var rotateAngle = fiConnector1.CoordinateSystem.BasisZ.AngleTo_WithSign(-startCon.CoordinateSystem.BasisZ);
                    var normal = fiConnector1.CoordinateSystem.BasisZ.GetNormal(-startCon.CoordinateSystem.BasisZ);

                    //过渡件特有的计算水平垂直偏移的值
                    var mepOri = Transform.CreateRotationAtPoint(normal, Math.Abs(rotateAngle), startCon.Origin).Inverse.OfPoint(mepCon.Origin);
                    xDis = mepOri.Y - fiConnector2.Origin.Y;
                    yDis = mepOri.Z - fiConnector2.Origin.Z;

                    ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(startCon.Origin, startCon.Origin + normal), Math.Abs(rotateAngle));

                    if ((mepCon.Owner is Duct || mepCon.Owner is CableTray) && (mepCon.Owner as MEPCurve).IsStand() && fiCon.CoordinateSystem.BasisZ.IsAlmostEqualTo(XYZ.BasisZ))
                    {
                        double angle = fiCon.CoordinateSystem.BasisX.AngleTo_XYPlaneWithSign(XYZ.BasisY);
                        ElementTransformUtils.RotateElement(RevitDoc, fi.Id, Line.CreateUnbound((fi.Location as LocationPoint).Point, XYZ.BasisZ), -angle);
                    }
                    #endregion

                    #region 修改连接点大小
                    //第一个点
                    fiConnector1.SetWidth(startCon);
                    if ((fiConnector1.Shape == ConnectorProfileType.Rectangular || fiConnector1.Shape == ConnectorProfileType.Oval))//天方地圆
                    {
                        if (!startCon.CoordinateSystem.BasisX.IsAlmostEqualTo(fiConnector1.CoordinateSystem.BasisX) && !startCon.CoordinateSystem.BasisY.IsAlmostEqualTo(fiConnector1.CoordinateSystem.BasisY))
                            ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(fiConnector1.Origin, fiConnector2.Origin), Math.PI / 2);

                        fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_WIDTH).Set(startCon.Width / 2 + xDis);
                        fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_HEIGHT).Set(startCon.Height / 2 + yDis);
                    }
                    else
                    {
                        fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_WIDTH).Set(startCon.Radius + xDis);
                        fi.get_Parameter(BuiltInParameter.RBS_FAMILY_CONTENT_OFFSET_HEIGHT).Set(startCon.Radius + yDis);
                    }

                    //第二个点
                    if ((fiConnector2.Shape == ConnectorProfileType.Rectangular || fiConnector2.Shape == ConnectorProfileType.Oval))
                    {
                        if (!endCon.CoordinateSystem.BasisX.IsAlmostEqualTo(fiConnector2.CoordinateSystem.BasisX) && !endCon.CoordinateSystem.BasisY.IsAlmostEqualTo(fiConnector2.CoordinateSystem.BasisY))
                        {
                            fiConnector2.Width = endCon.Height;
                            fiConnector2.Height = endCon.Width;
                        }
                        else
                        {
                            fiConnector2.Width = endCon.Width;
                            fiConnector2.Height = endCon.Height;
                        }
                    }
                    else
                        fiConnector2.SetWidth(endCon);
                    #endregion

                    #region 连接
                    this.RevitDoc.Regenerate();
                    fiConnector1.ConnectTo(startCon);
                    endCon.Origin = fiConnector2.Origin;
                    fiConnector2.ConnectTo(endCon);
                    #endregion
                    return fi;
                }
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                throw ex;
            }
            finally
            {
                fi.WriteParm((mepCon.Owner as MEPCurve).GetParmType(), true);
                fi.WriteParm_ConnectedFitting((mepCon.Owner as MEPCurve).GetParmType());
            }
        }

        /// <summary>
        /// 弯曲管连接
        /// FamilySymbol仅用作判断截面类型是否符合，并不修改族类型
        /// Defaut的意义：用doc.Create中的new函数创建管件
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="fs"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance NewElbowDefault(MEPCurve src, MEPCurve dst, FamilySymbol fs)
        {
            this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Elbow);
            this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Transition);

            Dictionary<MEPCurve, bool> dic = new Dictionary<MEPCurve, bool>();

            FamilyInstance fi = null;
            FamilyInstance fiMore = null;
            try
            {
                Connector connector1 = null, connector2 = null;
                var intersectWithXYZ = src.GetIntersectWithPointLineExpandTwo(dst);
                if (intersectWithXYZ == null)
                {
                    connector1 = src.GetClosestConnector(dst.GetMiddleXYZ());
                    connector2 = dst.GetClosestConnector(src.GetMiddleXYZ());
                }
                else
                {
                    connector1 = src.GetClosestConnector(intersectWithXYZ);
                    connector2 = dst.GetClosestConnector(intersectWithXYZ);
                }

                JudgeAndThrow(new Connector[] { connector1, connector2 }, fs, src);

                var angel = connector1.CoordinateSystem.BasisZ.AngleTo(connector2.CoordinateSystem.BasisZ);
                if (angel.IsLessThanEqualTo(Math.PI * (89.0 / 180.0), 0.001))//针对两根管道角度小于90的处理，通过创建两个过度管件的方式解决，不过在一些角度下仍无法实现
                {
                    #region
                    //这里的scale是个管件，根据管道宽度和角度不同，需要有变化
                    double scale = 0.6 / UnitTransUtils.MMToFeet(70) * src.GetWidth();

                    //复制产生过度管道
                    var copy = src.GetCopy(this.RevitDoc);
                    (copy.Location as LocationCurve).Curve = Line.CreateBound(intersectWithXYZ, intersectWithXYZ + scale * connector1.CoordinateSystem.BasisZ.RotateBy(connector1.CoordinateSystem.BasisZ.GetNormal(connector2.CoordinateSystem.BasisZ), -angel * 90.0 / Math.PI));
                    Connector mainCTo = copy.GetClosestConnector(intersectWithXYZ);
                    Connector nextCTo = copy.ConnectorManager.Lookup(0).Id == mainCTo.Id ? copy.ConnectorManager.Lookup(1) : copy.ConnectorManager.Lookup(0);
                    ElementTransformUtils.MoveElement(this.RevitDoc, copy.Id, -connector1.CoordinateSystem.BasisZ * Math.Cos((Math.PI / 2) / 2) * scale);
                    this.RevitDoc.Regenerate();

                    //连接
                    fi = this.RevitDoc.Create.NewElbowFitting(connector1, mainCTo);
                    fiMore = this.RevitDoc.Create.NewElbowFitting(connector2, nextCTo);
                    return fi;
                    #endregion
                }
                else
                {
                    if (src is CableTray)//桥架由于有上空的问题，导致翻转布置后有问题，故特殊处理
                        return fi = this.RevitDoc.Create.NewElbowFitting(connector1, connector2);
                    else
                        return fi = this.NewElbowPM(src, dst, fs);
                }
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                if (ex.Message.Contains("too small or too large"))
                    this.ThrowExceptionDefault(new Exception("管道之间角度过小或过大"), src, MEPCurveConnectTypeENUM.Elbow);
                else
                    this.ThrowExceptionDefault(ex, src, MEPCurveConnectTypeENUM.Elbow);
                return fi;
            }
            finally
            {
                fi.WriteParm(src.GetParmType(), false);
                fiMore.WriteParm(src.GetParmType(), false);
                fi.WriteParm_ConnectedFitting(src.GetParmType());
                fiMore.WriteParm_ConnectedFitting(src.GetParmType());
            }
        }
        /// <summary>
        /// 弯曲管连接
        /// FamilySymbol用于创建对应族实例
        /// PM的意义：通过doc.Create中的NewFamilyInstance函数创建管件，手动编写代码连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="fs"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance NewElbowPM(MEPCurve src, MEPCurve dst, FamilySymbol fs)
        {
            if (fs == null)
                fs = this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Elbow);
            if (fs == null)
                throw new Exception("未能加载到弯头管件族");

            FamilyInstance fi = null;
            try
            {
                var xyz = src.GetIntersectWithPointLineExpandTwo(dst);
                if (xyz == null)
                {
                    if (src is Pipe)
                        throw new Exception("两根管道需在延长线上有交点");
                    else if (src is Duct)
                        throw new Exception("两根风管需在延长线上有交点");
                    else if (src is Conduit)
                        throw new Exception("两根管线需在延长线上有交点");
                    else if (src is CableTray)
                        throw new Exception("两根桥架需在延长线上有交点");
                }
                var startCon = src.GetClosestConnector(xyz);
                var endCon = dst.GetClosestConnector(xyz);
                double rotateAngle = 0;

                JudgeAndThrow(new Connector[] { startCon, endCon }, fs, src);

                //创建管件，确定类型、方向、角度
                XYZ intersectWithPoint = src.GetIntersectWithPointLineExpandTwo(dst);
                double angle = Line.CreateBound(intersectWithPoint, src.GetMiddleXYZ()).Direction.AngleTo_WithSign(-Line.CreateBound(intersectWithPoint, dst.GetMiddleXYZ()).Direction);
                angle = Math.Abs(angle);

                var level = this.RevitDoc.GetElement(src.GetParameters("参照标高").First().AsElementId()) as Level;
                fi = this.RevitDoc.Create.NewFamilyInstance(intersectWithPoint.Add(new XYZ(0, 0, -level.Elevation)), fs, level, 0);
                var fiConnector1 = fi.GetConnectorByDirection(new XYZ(-1, 0, 0));
                var fiConnector2 = fi.GetAnthorConnector(fiConnector1);

                //angle = angle > Math.PI / 2 ? (Math.PI - angle) : angle;
                if (fiConnector2.Angle != 0)
                    fiConnector2.Angle = angle;

                //先将管件的起点对正，起点方向是(-1,0,0)
                rotateAngle = fiConnector1.CoordinateSystem.BasisZ.AngleTo_WithSign(-startCon.CoordinateSystem.BasisZ);
                var normal = fiConnector1.CoordinateSystem.BasisZ.GetNormal(-startCon.CoordinateSystem.BasisZ);
                ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(intersectWithPoint, intersectWithPoint + normal), Math.Abs(rotateAngle));
                this.RevitDoc.Regenerate();

                //在起点对正后，以变换后的起点方向为法线向量，对正第三个点方向
                rotateAngle = fiConnector2.CoordinateSystem.BasisZ.AngleTo_WithSign(-endCon.CoordinateSystem.BasisZ, fiConnector1.CoordinateSystem.BasisZ);
                var normal1 = fiConnector1.CoordinateSystem.BasisZ;
                ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(intersectWithPoint, intersectWithPoint + normal1), Math.Abs(rotateAngle));
                this.RevitDoc.Regenerate();

                //设置管件宽度
                //if (startCon.Shape == ConnectorProfileType.Round && endCon.Shape == ConnectorProfileType.Round)
                {

                    bool turn = fiConnector1.CoordinateSystem.BasisZ.GetNormal(fiConnector2.CoordinateSystem.BasisZ).Z.IsAlmostEqualTo(0) ? true : false;
                    if (startCon.GetWidth() > endCon.GetWidth())
                    {
                        fiConnector1.SetWidth(startCon, turn); this.RevitDoc.Regenerate();
                        fiConnector2.SetWidth(startCon, turn); this.RevitDoc.Regenerate();
                    }
                    else
                    {
                        fiConnector1.SetWidth(endCon, turn); this.RevitDoc.Regenerate();
                        fiConnector2.SetWidth(endCon, turn); this.RevitDoc.Regenerate();
                    }
                }
                this.NewTransitionPM(fiConnector1, startCon, null);
                this.NewTransitionPM(fiConnector2, endCon, null);

                return fi;
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionPM(ex, src, MEPCurveConnectTypeENUM.Elbow);
                return fi;
            }
            finally
            {
                fi.WriteParm(src.GetParmType(), false);
                fi.WriteParm_ConnectedFitting(src.GetParmType());
            }
        }

        /// <summary>
        /// 三通连接
        /// dst1 dst2在同一直线上，dst1在dst2的左边，或dst1在dst2上方(dst1和dst2是同一条时例外)
        /// FamilySymbol仅用作判断截面类型是否符合，并不修改族类型
        /// Defaut的意义：用doc.Create中的NewFitting相关函数创建管件并自动连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst1"></param>
        /// <param name="dst2"></param>
        /// <param name="fs"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance NewTeeDefault(MEPCurve src, MEPCurve dst1, MEPCurve dst2, FamilySymbol fs)
        {
            this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Tee);
            this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Transition);

            Dictionary<MEPCurve, bool> dic = new Dictionary<MEPCurve, bool>();

            FamilyInstance fi = null;
            try
            {
                Connector connector1 = null, connector2 = null, connector3 = null;

                if (dst1 == dst2)
                {
                    Connector linkedC = null;
                    XYZ intersectWithPoint = src.GetIntersectWithPointLineExpandTwo(dst1);
                    MEPCurve copyCurve = dst1.GetCopy(this.RevitDoc);
                    connector1 = dst1.ConnectorManager.Connectors.GetConnectorById(0);
                    connector2 = copyCurve.ConnectorManager.Connectors.GetConnectorById(1);

                    if ((linkedC = connector1.GetConnectedConnector()) != null)
                    {
                        connector1.DisconnectFrom(linkedC);
                        copyCurve.ConnectorManager.Connectors.GetConnectorById(0).ConnectTo(linkedC);
                    }
                    connector3 = src.GetClosestConnector(intersectWithPoint);

                    connector1.Origin = intersectWithPoint;
                    connector2.Origin = intersectWithPoint;
                    src.Document.Regenerate();

                }
                else
                {
                    connector1 = dst1.GetClosestConnector(dst2);
                    connector2 = dst2.GetClosestConnector(connector1.Origin);
                    connector3 = src.GetClosestConnector(connector1.Origin);
                }

                JudgeAndThrow(new Connector[] { connector1, connector2, connector3 }, fs, src);

                this.StartSpecialCableTray(dic, new MEPCurve[] { src, dst1, dst2 });

                try
                {
                    return fi = this.RevitDoc.Create.NewTeeFitting(connector1, connector2, connector3);
                }
                catch
                {
                    return fi = this.RevitDoc.Create.NewTeeFitting(connector2, connector1, connector3);
                }
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionDefault(ex, src, MEPCurveConnectTypeENUM.Tee);
                return fi;
            }
            finally
            {
                fi.WriteParm(src.GetParmType(), true);
                fi.WriteParm_ConnectedFitting(src.GetParmType());
                this.EndSpecialCableTray(dic);
            }
        }
        /// <summary>
        /// 三通连接
        /// FamilySymbol用于创建对应族实例
        /// PM的意义：通过doc.Create中的NewFamilyInstance函数创建管件，手动编写代码连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst1"></param>
        /// <param name="dst2"></param>
        /// <param name="fs"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance NewTeePM(MEPCurve src, MEPCurve dst1, MEPCurve dst2, FamilySymbol fs)
        {
            if (dst1 == dst2 && src.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == "无配件的电缆桥架")
            {
                this.NewTee_CableTray(src, dst1);
                return null;
            }

            if (fs == null)
                fs = this.Judge_LoadDefaultFitting(src, MEPCurveConnectTypeENUM.Tee);
            if (fs == null)
                throw new Exception("未能加载到三通管件族");

            FamilyInstance fi = null;
            try
            {
                MEPCurveTeeFilterTypeEnum teeType = MEPCurveTeeFilterTypeEnum.Normal;
                Connector startCon = null, endCon1 = null, endCon2 = null;
                Connector fiConnector1 = null, fiConnector2 = null, fiConnector3 = null;
                double angle1 = 0, angle2 = 0, rotateAngle = 0;
                XYZ intersectWithPoint = dst1.GetIntersectWithPointLineExpandTwo(src) != null ? dst1.GetIntersectWithPointLineExpandTwo(src) : dst2.GetIntersectWithPointLineExpandTwo(src);
                if (intersectWithPoint == null)
                    throw new Exception("管道与管道之间未能相交");

                #region 创建管件,修改连接点宽度
                var level = this.RevitDoc.GetElement(src.GetParameters("参照标高").First().AsElementId()) as Level;
                fi = this.RevitDoc.Create.NewFamilyInstance(intersectWithPoint.Add(new XYZ(0, 0, -level.Elevation)), fs, level, 0);
                var type = fi.Symbol.Family.get_Parameter(BuiltInParameter.FAMILY_CONTENT_PART_TYPE).AsValueString();
                fiConnector1 = fi.GetConnectorByDirection(new XYZ(-1, 0, 0));
                if (type.Contains("T 形三通"))
                {
                    if (fi.GetConnectorByDirection(new XYZ(1, 0, 0)) != null)
                    {
                        teeType = MEPCurveTeeFilterTypeEnum.Normal;
                        fiConnector2 = fi.GetConnectorByDirection(new XYZ(1, 0, 0));
                        fiConnector3 = fi.GetTeeThirdConnector();
                    }
                    else if (fi.GetConnectorByDirection(new XYZ(0, 1, 0)) != null)
                    {
                        teeType = MEPCurveTeeFilterTypeEnum.T;
                        fiConnector2 = fi.GetConnectorByDirection(new XYZ(0, 1, 0));
                        fiConnector3 = fi.GetConnectorByDirection(new XYZ(0, -1, 0));
                    }
                    else
                    {
                        teeType = MEPCurveTeeFilterTypeEnum.Y;
                        fiConnector2 = fi.GetTeeSecondConnector();
                        fiConnector3 = fi.GetTeeThirdConnector();
                    }
                }
                else if (type.Contains("Y 形三通"))
                {
                    teeType = MEPCurveTeeFilterTypeEnum.Y;
                    fiConnector2 = fi.GetTeeSecondConnector();
                    fiConnector3 = fi.GetTeeThirdConnector();
                }
                else
                {
                    throw new Exception("管件的类型出错，应为T 形三通或者Y 形三通");
                }
                #endregion

                #region 确定连接点，以及相交点
                if (dst1 == dst2)//这种只会出现teeType=Normal的情况
                {
                    MEPCurve copyCurve = dst1.GetCopy(this.RevitDoc);
                    startCon = dst1.ConnectorManager.Connectors.GetConnectorById(0);
                    endCon1 = copyCurve.ConnectorManager.Connectors.GetConnectorById(1);
                    endCon2 = src.GetClosestConnector(src.GetIntersectWithPointLineExpandTwo(dst1));

                    startCon.ChangeLinkedConnectorTo(copyCurve.ConnectorManager.Connectors.GetConnectorById(startCon.Id));

                    startCon.Origin = intersectWithPoint;
                    endCon1.Origin = intersectWithPoint;
                    endCon2.Origin = intersectWithPoint;

                    this.RevitDoc.Regenerate();
                }
                else
                {
                    if (teeType == MEPCurveTeeFilterTypeEnum.Normal)
                    {
                        if (src.IsColinearTo(dst1))
                        {
                            startCon = src.GetClosestConnector(dst2);
                            endCon1 = dst1.GetClosestConnector(startCon.Origin);
                            endCon2 = dst2.GetClosestConnector(startCon.Origin);
                        }
                        else if (src.IsColinearTo(dst2))
                        {
                            startCon = src.GetClosestConnector(dst1);
                            endCon1 = dst2.GetClosestConnector(startCon.Origin);
                            endCon2 = dst1.GetClosestConnector(startCon.Origin);
                        }
                        else if (dst1.IsColinearTo(dst2))
                        {
                            startCon = dst1.GetClosestConnector(src);
                            endCon1 = dst2.GetClosestConnector(startCon.Origin);
                            endCon2 = src.GetClosestConnector(startCon.Origin);
                        }
                        else
                        {
                            this.RevitDoc.Delete(fi.Id);
                            throw new Exception("当前位置无法用此连接方式连接,需要有两个管道在同一直线上");
                        }
                    }
                    else if (teeType == MEPCurveTeeFilterTypeEnum.T)
                    {
                        if (src.IsColinearTo(dst1))
                        {
                            startCon = dst2.GetClosestConnector(src);
                            endCon1 = src.GetClosestConnector(startCon.Origin);
                            endCon2 = dst1.GetClosestConnector(startCon.Origin);
                        }
                        else if (src.IsColinearTo(dst2))
                        {
                            startCon = dst1.GetClosestConnector(src);
                            endCon1 = src.GetClosestConnector(startCon.Origin);
                            endCon2 = dst2.GetClosestConnector(startCon.Origin);
                        }
                        else if (dst1.IsColinearTo(dst2))
                        {
                            startCon = src.GetClosestConnector(dst2);
                            endCon1 = dst1.GetClosestConnector(startCon.Origin);
                            endCon2 = dst2.GetClosestConnector(startCon.Origin);
                        }
                        else
                        {
                            this.RevitDoc.Delete(fi.Id);
                            throw new Exception("当前位置无法用此连接方式连接,需要有两个管道在同一直线上");
                        }
                    }
                    else
                    {
                        startCon = src.GetClosestConnector(dst1);
                        endCon1 = dst1.GetClosestConnector(startCon.Origin);
                        endCon2 = dst2.GetClosestConnector(startCon.Origin);

                        double a1 = startCon.CoordinateSystem.BasisZ.AngleTo(endCon1.CoordinateSystem.BasisZ);
                        double a2 = startCon.CoordinateSystem.BasisZ.AngleTo(endCon2.CoordinateSystem.BasisZ);
                        if (a1 > Math.PI * 15 / 18 || a1 < Math.PI * 12 / 18)
                            throw new Exception("第一根风管和第二根风管的夹角请在120°到150°之间");

                        if (a2 > Math.PI * 15 / 18 || a2 < Math.PI * 12 / 18)
                            throw new Exception("第一根风管和第三根风管的夹角请在120°到150°之间");
                    }
                }
                JudgeAndThrow(new Connector[] { startCon, endCon1, endCon2 }, fs, src);
                #endregion

                #region 旋转
                //先将三通管件的起点对正，起点方向是(-1,0,0)
                rotateAngle = fiConnector1.CoordinateSystem.BasisZ.AngleTo_WithSign(-startCon.CoordinateSystem.BasisZ);
                var normal = fiConnector1.CoordinateSystem.BasisZ.IsAlmostEqualTo(startCon.CoordinateSystem.BasisZ) || fiConnector1.CoordinateSystem.BasisZ.IsAlmostEqualTo(-startCon.CoordinateSystem.BasisZ) ? XYZ.BasisZ : fiConnector1.CoordinateSystem.BasisZ.GetNormal(-startCon.CoordinateSystem.BasisZ);
                ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(intersectWithPoint, intersectWithPoint + normal), Math.Abs(rotateAngle));
                this.RevitDoc.Regenerate();

                ////在起点对正后，以变换后的起点方向为法线向量，对正第三个点方向
                if (fiConnector3.CoordinateSystem.BasisZ.IsAlmostEqualTo_PM(endCon2.CoordinateSystem.BasisZ) && src is CableTray)//这种情况下的桥架特殊处理
                {
                    var temp = startCon;
                    startCon = endCon1;
                    endCon1 = temp;
                    ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(intersectWithPoint, intersectWithPoint + normal), Math.PI);
                    this.RevitDoc.Regenerate();
                }
                else
                {
                    rotateAngle = fiConnector3.CoordinateSystem.BasisZ.AngleTo_WithSign(-endCon2.CoordinateSystem.BasisZ, fiConnector1.CoordinateSystem.BasisZ);
                    var normal1 = fiConnector1.CoordinateSystem.BasisZ;
                    ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(intersectWithPoint, intersectWithPoint + normal1), Math.Abs(rotateAngle));
                    this.RevitDoc.Regenerate();
                }
                #endregion

                #region 设置角度
                if (teeType == MEPCurveTeeFilterTypeEnum.Normal)
                {
                    angle2 = fiConnector1.CoordinateSystem.BasisZ.AngleTo(endCon2.CoordinateSystem.BasisZ);
                    if (!angle2.IsAlmostEqualTo(0))
                    {
                        var angles = fi.GetParameters("角度");
                        if (angles.Count != 0)
                            angles[0].Set(angle2);

                    }
                }
                else if (teeType == MEPCurveTeeFilterTypeEnum.Y)
                {
                    angle1 = fiConnector1.CoordinateSystem.BasisZ.AngleTo(endCon1.CoordinateSystem.BasisZ);
                    if (!angle1.IsAlmostEqualTo(0))
                    {
                        var angles = fi.GetParameters("出口角度");
                        if (angles.Count != 0)
                            angles[0].Set(angle1);
                    }

                    angle2 = fiConnector1.CoordinateSystem.BasisZ.AngleTo(endCon2.CoordinateSystem.BasisZ);
                    if (!angle2.IsAlmostEqualTo(0))
                    {
                        var angles = fi.GetParameters("支管角度");
                        if (angles.Count != 0)
                            angles[0].Set(angle2);
                    }
                }
                this.RevitDoc.Regenerate();
                #endregion

                #region 创建连接管件
                fiConnector1.AlineToBYMove(startCon, this.RevitDoc);
                fiConnector2.AlineToBYMove(endCon1, this.RevitDoc);
                fiConnector3.AlineToBYMove(endCon2, this.RevitDoc);

                var isTurn = endCon2.CoordinateSystem.BasisZ.Z.IsAlmostEqualTo(1, 0.001) || endCon2.CoordinateSystem.BasisZ.Z.IsAlmostEqualTo(-1, 0.001) ||
                    (src.IsCross() && dst1 == dst2 && dst1.IsStand());

                if ((startCon.Owner as MEPCurve).GetWidth() < (endCon1.Owner as MEPCurve).GetWidth())
                {
                    if ((endCon1.Owner as MEPCurve).GetWidth() < (endCon2.Owner as MEPCurve).GetWidth())
                    {
                        fiConnector1.SetWidth(startCon, isTurn); this.RevitDoc.Regenerate();
                        fiConnector2.SetWidth(endCon1, isTurn); this.RevitDoc.Regenerate();
                        fiConnector3.SetWidth(endCon2, isTurn);
                    }
                    else
                    {
                        fiConnector1.SetWidth(startCon, isTurn); this.RevitDoc.Regenerate();
                        fiConnector3.SetWidth(endCon2, isTurn); this.RevitDoc.Regenerate();
                        fiConnector2.SetWidth(endCon1, isTurn);
                    }
                }
                else
                {
                    if ((startCon.Owner as MEPCurve).GetWidth() < (endCon2.Owner as MEPCurve).GetWidth())
                    {
                        fiConnector1.SetWidth(startCon, isTurn); this.RevitDoc.Regenerate();
                        fiConnector2.SetWidth(endCon1, isTurn); this.RevitDoc.Regenerate();
                        fiConnector3.SetWidth(endCon2, isTurn);
                    }
                    else
                    {
                        fiConnector3.SetWidth(endCon2, isTurn); this.RevitDoc.Regenerate();
                        fiConnector2.SetWidth(endCon1, isTurn); this.RevitDoc.Regenerate();
                        fiConnector1.SetWidth(startCon, isTurn);
                    }
                }
                this.RevitDoc.Regenerate();
                this.NewTransitionPM(fiConnector1, startCon, null);
                this.NewTransitionPM(fiConnector2, endCon1, null);
                this.NewTransitionPM(fiConnector3, endCon2, null);
                this.RevitDoc.Regenerate();
                #endregion

                return fi;
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionPM(ex, src, MEPCurveConnectTypeENUM.Tee);
                return fi;
            }
            finally
            {
                fi.WriteParm(src.GetParmType(), true);
                fi.WriteParm_ConnectedFitting(src.GetParmType());
            }
        }

        /// <summary>
        /// 四通连接
        /// 连接点的Owner一定要是管道
        /// FamilySymbol仅用作判断截面类型是否符合，并不修改族类型
        /// Defaut的意义：用doc.Create中的NewFitting相关函数创建管件并自动连接
        /// </summary>
        /// <param name="connector1"></param>
        /// <param name="connector2"></param>
        /// <param name="connector3"></param>
        /// <param name="connector4"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        protected FamilyInstance NewCrossDefault(Connector connector1, Connector connector2, Connector connector3, Connector connector4, FamilySymbol fs)
        {
            this.Judge_LoadDefaultFitting(connector1.Owner as MEPCurve, MEPCurveConnectTypeENUM.Cross);
            this.Judge_LoadDefaultFitting(connector1.Owner as MEPCurve, MEPCurveConnectTypeENUM.Transition);

            FamilyInstance fi = null;
            try
            {
                List<Connector> connectors = new List<Connector>();
                connectors.Add(connector1);
                connectors.Add(connector2);
                connectors.Add(connector3);
                connectors.Add(connector4);

                JudgeAndThrow(new Connector[] { connectors[0], connectors[1], connectors[2], connectors[3] }, fs, connector1.Owner as MEPCurve);

                Connector c1 = null, c2 = null, c3 = null, c4 = null;

                //确定第一点
                c1 = connectors[0];
                connectors.Remove(c1);

                //确定第二点，与第一点反方向的点
                foreach (var con in connectors)
                {
                    if (c1.CoordinateSystem.BasisZ.IsAlmostEqualTo_PM(-con.CoordinateSystem.BasisZ, 0.001))
                    //if (c1.CoordinateSystem.BasisZ.AngleTo(con.CoordinateSystem.BasisZ).IsAlmostEqualTo(Math.PI))
                    {
                        c2 = con;
                        break;
                    }
                }
                if (c2 == null)
                {
                    if (c1.Owner is Pipe)
                        throw new Exception("请选择两两垂直的管道");
                    else if (c1.Owner is Duct)
                        throw new Exception("请选择两两垂直的风管");
                    else if (c1.Owner is Conduit)
                        throw new Exception("请选择两两垂直的线管");
                    else
                        throw new Exception("请选择两两垂直的桥架");
                }


                connectors.Remove(c2);

                //确定第三点，和第一点顺时针旋转90度
                foreach (var con in connectors)
                {
                    double aa = c1.CoordinateSystem.BasisZ.AngleTo_WithSign(con.CoordinateSystem.BasisZ);
                    if (c1.CoordinateSystem.BasisZ.AngleTo_WithSign(con.CoordinateSystem.BasisZ).IsAlmostEqualTo(-Math.PI / 2))
                    {
                        c3 = con;
                        break;
                    }
                }
                if (c3 == null)
                {
                    if (c1.Owner is Pipe)
                        throw new Exception("请选择两两垂直的管道");
                    else
                        throw new Exception("请选择两两垂直的风管");
                }
                connectors.Remove(c3);


                //确定第四点，和第一点逆时针旋转90度，或和第三点反方向
                foreach (var con in connectors)
                {
                    double aa = c1.CoordinateSystem.BasisZ.AngleTo_WithSign(con.CoordinateSystem.BasisZ, c1.CoordinateSystem.BasisZ.GetNormal(c3.CoordinateSystem.BasisZ));
                    if (c1.CoordinateSystem.BasisZ.AngleTo_WithSign(con.CoordinateSystem.BasisZ, c1.CoordinateSystem.BasisZ.GetNormal(c3.CoordinateSystem.BasisZ)).IsAlmostEqualTo(-Math.PI / 2 * 3))
                    {
                        c4 = con;
                        break;
                    }
                }
                if (c4 == null)
                {
                    if (c1.Owner is Pipe)
                        throw new Exception("请选择两两垂直的管道");
                    else
                        throw new Exception("请选择两两垂直的风管");
                }
                connectors.Remove(c4);

                return fi = this.RevitDoc.Create.NewCrossFitting(c1, c2, c3, c4);
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                if (ex.Message.Contains("Fitting cannot be created between the input connectors because they are not close enough for intersection."))
                    throw new Exception("未能创建四通管件：管道与管道之间未能相交");
                else
                    this.ThrowExceptionDefault(ex, connector1.Owner as MEPCurve, MEPCurveConnectTypeENUM.Cross);
                return fi;
            }
            finally
            {
                fi.WriteParm((connector1.Owner as MEPCurve).GetParmType(), false);
                fi.WriteParm_ConnectedFitting((connector1.Owner as MEPCurve).GetParmType());
            }
        }
        /// <summary>
        /// 四通连接
        /// 当前只适用于XY平面
        /// FamilySymbol仅用作判断截面类型是否符合，并不修改族类型
        /// Defaut的意义：用doc.Create中的NewFitting相关函数创建管件并自动连接
        /// </summary>
        /// <param name="dst1"></param>
        /// <param name="dst2"></param>
        /// <param name="dst3"></param>
        /// <param name="dst4"></param>
        /// <param name="fs"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance NewCrossDefault(MEPCurve dst1, MEPCurve dst2, MEPCurve dst3, MEPCurve dst4, FamilySymbol fs, bool isMakeSureMiddleMax = false)
        {
            this.Judge_LoadDefaultFitting(dst1, MEPCurveConnectTypeENUM.Cross);
            this.Judge_LoadDefaultFitting(dst1, MEPCurveConnectTypeENUM.Transition);

            Dictionary<MEPCurve, bool> dic = new Dictionary<MEPCurve, bool>();

            try
            {
                FamilyInstance fi = null;

                List<Connector> connectors = new List<Connector>();
                connectors.Add(dst1.GetClosestConnector(dst3));
                connectors.Add(dst2.GetClosestConnector(connectors[0].Origin));
                connectors.Add(dst3.GetClosestConnector(connectors[0].Origin));
                connectors.Add(dst4.GetClosestConnector(connectors[0].Origin));

                //进行排序，保证中心线是最大管
                if (isMakeSureMiddleMax)
                {
                    connectors.Sort((con1, con2) =>
                    {
                        if (con1.Shape == ConnectorProfileType.Round)
                        {
                            if (con1.Radius.IsAlmostEqualTo(con2.Radius))
                                return 0;
                            else if (con1.Radius > con2.Radius)
                                return 1;
                            else
                                return -1;
                        }
                        else
                        {
                            if ((con1.Width * con1.Height).IsAlmostEqualTo(con2.Width * con2.Height))
                                return 0;
                            else if ((con1.Width * con1.Height) > (con2.Width * con2.Height))
                                return 1;
                            else
                                return -1;
                        }
                    });
                    connectors.Reverse();
                }

                JudgeAndThrow(new Connector[] { connectors[0], connectors[1], connectors[2], connectors[3] }, fs, dst1);

                this.StartSpecialCableTray(dic, new MEPCurve[] { dst1, dst2, dst3, dst4 });

                return fi = this.NewCrossDefault(connectors[0], connectors[1], connectors[2], connectors[3], fs);
            }
            finally
            {
                this.EndSpecialCableTray(dic);
            }
        }
        /// <summary>
        /// 四通连接
        /// FamilySymbol用于创建对应族实例
        /// PM的意义：通过doc.Create中的NewFamilyInstance函数创建管件，手动编写代码连接
        /// </summary>
        /// <param name="dst1"></param>
        /// <param name="dst2"></param>
        /// <param name="dst3"></param>
        /// <param name="dst4"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        protected FamilyInstance NewCrossPM(MEPCurve dst1, MEPCurve dst2, MEPCurve dst3, MEPCurve dst4, FamilySymbol fs)
        {
            if (fs == null)
                fs = this.Judge_LoadDefaultFitting(dst1, MEPCurveConnectTypeENUM.Cross);
            if (fs == null)
                throw new Exception("未能加载到四通通管件族");

            FamilyInstance fi = null;
            try
            {
                MEPCurveCrossFilterTypeEnum crossType = MEPCurveCrossFilterTypeEnum.Normal;
                List<Connector> connectors = new List<Connector>();
                Connector startCon = null, endCon1 = null, endCon2 = null, endCon3 = null;
                Connector fiConnector1 = null, fiConnector2 = null, fiConnector3 = null, fiConnector4 = null;
                double rotateAngle = 0;
                XYZ intersectWithPoint = null;

                #region 确定连接点
                startCon = dst1.GetClosestConnector(dst2);
                connectors.Add(dst2.GetClosestConnector(startCon.Origin));
                connectors.Add(dst3.GetClosestConnector(startCon.Origin));
                connectors.Add(dst4.GetClosestConnector(startCon.Origin));

                //确定第二点，与第一点反方向的点
                foreach (var con in connectors)
                {
                    if (startCon.CoordinateSystem.BasisZ.AngleTo(con.CoordinateSystem.BasisZ).IsAlmostEqualTo(Math.PI))
                    {
                        endCon1 = con;
                        break;
                    }
                }

                //确定第三点，和第一点顺时针旋转90度
                foreach (var con in connectors)
                {
                    var angle = startCon.CoordinateSystem.BasisZ.AngleTo_WithSign(con.CoordinateSystem.BasisZ);
                    if (angle.IsAlmostEqualTo(-Math.PI / 2, 0.001) ||
                        (angle < (-Math.PI / 2) && angle > (-Math.PI)))
                    {
                        endCon2 = con;
                        break;
                    }
                }

                //确定第四点，和第一点逆时针旋转90度，或和第三点反方向
                foreach (var con in connectors)
                {
                    var angle = startCon.CoordinateSystem.BasisZ.AngleTo_WithSign(con.CoordinateSystem.BasisZ, startCon.CoordinateSystem.BasisZ.GetNormal(endCon2.CoordinateSystem.BasisZ));
                    if (angle.IsAlmostEqualTo(-Math.PI / 2 * 3, 0.001) ||
                    (angle < (-Math.PI) && angle > (-Math.PI / 2 * 3)))
                    {
                        endCon3 = con;
                        break;
                    }
                }
                if (endCon1 == null || endCon2 == null || endCon3 == null)
                {
                    if (crossType == MEPCurveCrossFilterTypeEnum.Normal)
                    {
                        if (startCon.Owner is Pipe)
                            throw new Exception("请选择两两垂直的管道");
                        else
                            throw new Exception("请选择两两垂直的风管");
                    }
                    else
                    {
                        if (startCon.Owner is Pipe)
                            throw new Exception("管道位置不符合斜四通连接要求");
                        else
                            throw new Exception("风管位置不符合斜四通连接要求");
                    }
                }

                intersectWithPoint = (startCon.Owner as MEPCurve).GetIntersectWithPointLineExpandTwo(endCon2.Owner as MEPCurve);
                #endregion

                #region 创建管件，确定是正常四通，还是斜四通
                var level = this.RevitDoc.GetElement(dst1.GetParameters("参照标高").First().AsElementId()) as Level;
                fi = this.RevitDoc.Create.NewFamilyInstance(intersectWithPoint.Add(new XYZ(0, 0, -level.Elevation)), fs, level, 0);
                if (fi.GetConnectorByDirection(new XYZ(0, 1, 0)) != null)
                {
                    fiConnector1 = fi.GetConnectorByDirection(new XYZ(-1, 0, 0));
                    fiConnector2 = fi.GetConnectorByDirection(new XYZ(1, 0, 0));
                    fiConnector3 = fi.GetConnectorByDirection(new XYZ(0, 1, 0));
                    fiConnector4 = fi.GetConnectorByDirection(new XYZ(0, -1, 0));
                    crossType = MEPCurveCrossFilterTypeEnum.Normal;
                }
                else
                {
                    fiConnector1 = fi.GetConnectorByDirection(new XYZ(-1, 0, 0));
                    fiConnector2 = fi.GetConnectorByDirection(new XYZ(1, 0, 0));
                    fiConnector3 = fi.GetCrossThirdConnector();
                    fiConnector4 = fi.GetCrossFourConnector();
                    crossType = MEPCurveCrossFilterTypeEnum.Bevel;
                }
                #endregion

                #region 调整管件连接点大小
                fiConnector1.SetWidth(startCon);
                fiConnector2.SetWidth(endCon1);
                fiConnector3.SetWidth(endCon2);
                fiConnector4.SetWidth(endCon3);
                #endregion

                #region 旋转
                //先将三通管件的起点对正，起点方向是(-1,0,0)
                rotateAngle = fiConnector1.CoordinateSystem.BasisZ.AngleTo_WithSign(-startCon.CoordinateSystem.BasisZ);
                var normal = fiConnector1.CoordinateSystem.BasisZ.GetNormal(-startCon.CoordinateSystem.BasisZ);
                ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(intersectWithPoint, intersectWithPoint + normal), Math.Abs(rotateAngle));
                this.RevitDoc.Regenerate();

                //在起点对正后，以变换后的起点方向为法线向量，对正第三个点方向
                rotateAngle = fiConnector3.CoordinateSystem.BasisZ.AngleTo_WithSign(-endCon2.CoordinateSystem.BasisZ, fiConnector1.CoordinateSystem.BasisZ);
                var normal1 = fiConnector1.CoordinateSystem.BasisZ;
                ElementTransformUtils.RotateElement(this.RevitDoc, fi.Id, Line.CreateBound(intersectWithPoint, intersectWithPoint + normal1), Math.Abs(rotateAngle));
                this.RevitDoc.Regenerate();
                #endregion

                #region 设置角度
                if (crossType == MEPCurveCrossFilterTypeEnum.Bevel)
                {
                    double angle1 = 0, angle2 = 0;
                    angle1 = fiConnector1.CoordinateSystem.BasisZ.AngleTo(endCon2.CoordinateSystem.BasisZ);
                    if (!angle1.IsAlmostEqualTo(0))
                    {
                        var angles = fi.GetParameters("支管1角度");
                        if (angles.Count != 0)
                            angles[0].Set(angle1);
                    }

                    angle2 = fiConnector1.CoordinateSystem.BasisZ.AngleTo(endCon3.CoordinateSystem.BasisZ);
                    if (!angle2.IsAlmostEqualTo(0))
                    {
                        var angles = fi.GetParameters("支管2角度");
                        if (angles.Count != 0)
                            angles[0].Set(angle2);
                    }
                }
                this.RevitDoc.Regenerate();
                #endregion

                #region 连接
                fiConnector1.AlineToBYMove(startCon, this.RevitDoc);
                this.RevitDoc.Create.NewTransitionFitting(startCon, fiConnector1);
                fiConnector2.AlineToBYMove(endCon1, this.RevitDoc);
                this.RevitDoc.Create.NewTransitionFitting(endCon1, fiConnector2);
                fiConnector3.AlineToBYMove(endCon2, this.RevitDoc);
                this.RevitDoc.Create.NewTransitionFitting(endCon2, fiConnector3);
                fiConnector4.AlineToBYMove(endCon3, this.RevitDoc);
                this.RevitDoc.Create.NewTransitionFitting(endCon3, fiConnector4);
                #endregion
                return fi;
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionPM(ex, dst1, MEPCurveConnectTypeENUM.Cross);
                return fi;
            }
            finally
            {
                fi.WriteParm(dst1.GetParmType(), false);
                fi.WriteParm_ConnectedFitting(dst1.GetParmType());
            }
        }

        /// <summary>
        /// 来回弯连接
        /// FamilySymbol仅用作判断截面类型是否符合，并不修改族类型
        /// Defaut的意义：用doc.Create中的NewFitting相关函数创建管件并自动连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="fs"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        protected FamilyInstance[] NewElbowGoBackDefault(MEPCurve src, MEPCurve dst, FamilySymbol fs, Document doc, int angle)
        {
            FamilyInstance[] fis = new FamilyInstance[2];
            try
            {
                if (src is CableTray)
                {
                    var fi = NewTransitionDefault(src, dst, fs);
                    if (fi == null)
                        return fis = new FamilyInstance[0];
                    else
                        return fis = new FamilyInstance[1] { fi };
                }

                //-------    ---------
                var dis = src.GetClosestPoint(dst.ConnectorManager.Lookup(0).Origin).DistanceTo(dst.ConnectorManager.Lookup(0).Origin);
                if (dis.IsLessThanEqualTo((src.GetWidth() + dst.GetWidth()) / 4))
                {
                    var con1 = src.GetClosestConnector(dst);
                    var con2 = dst.GetClosestConnector(src);
                    var normal = con1.CoordinateSystem.BasisZ.GetNormal((con2.Origin - con1.Origin).Normalize());
                    ElementTransformUtils.MoveElement(doc, src.Id, con1.CoordinateSystem.BasisZ.RotateBy(normal, 90) * dis);

                    var fi = NewTransitionDefault(src, dst, fs);
                    if (fi == null)
                        return fis = new FamilyInstance[0];
                    else
                        return fis = new FamilyInstance[1] { fi };
                }
                else if (dis.IsLessThan(src.GetWidth() + dst.GetWidth()))
                {
                    throw new Exception("连接失败：管道不在同一直线上，但又不够距离生成来回弯");
                }

                MEPCurve copyCurve = null;

                var connector1 = src.GetClosestConnector(dst.GetMiddleXYZ());
                var connector2 = dst.GetClosestConnector(src.GetMiddleXYZ());

                JudgeAndThrow(new Connector[] { connector1, connector2 }, fs, src);

                //创建管件
                //copyCurve = src.GetCopy(doc);
                //foreach (Connector con in copyCurve.ConnectorManager.Connectors)
                //{
                //    con.SetWidth(src.GetWidth() < dst.GetWidth() ? connector1 : connector2);
                //}
                copyCurve = src.GetWidth() < dst.GetWidth() ? src.GetCopy(doc) : dst.GetCopy(doc);

                this.RevitDoc.Regenerate();

                XYZ dir = Line.CreateBound(connector1.Origin, connector2.Origin).Direction;
                XYZ rotate = connector1.CoordinateSystem.BasisZ.RotateBy(connector1.CoordinateSystem.BasisZ.GetNormal(dir), angle);
                (copyCurve.Location as LocationCurve).Curve = Line.CreateBound(connector1.Origin, connector1.Origin + rotate * dst.GetClosestPoint(connector1.Origin).DistanceTo(connector1.Origin));

                //进行连接
                fis[0] = NewElbowDefault(dst, copyCurve, null);
                fis[1] = NewElbowDefault(src, copyCurve, null);

                return fis;
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionDefault(ex, src, MEPCurveConnectTypeENUM.Elbow);
                return fis;
            }
            finally
            {
                foreach (var fi in fis)
                {
                    if (fi != null)
                    {
                        fi.WriteParm(src.GetParmType(), false);
                        fi.WriteParm_ConnectedFitting(src.GetParmType());
                    }
                }

            }
        }
        /// <summary>
        /// 来回弯连接
        /// FamilySymbol用于创建对应族实例
        /// PM的意义：通过doc.Create中的NewFamilyInstance函数创建管件，手动编写代码连接
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <param name="fs"></param>
        /// <param name="doc"></param>
        /// <param name="angle"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        protected FamilyInstance[] NewElbowGoBackPM(MEPCurve src, MEPCurve dst, FamilySymbol fs, Document doc, int angle)
        {
            FamilyInstance[] fis = new FamilyInstance[2];
            try
            {
                //-------    ---------
                if (src.IsColinearTo(dst))
                {
                    var fi = NewTransitionPM(src, dst, fs);
                    if (fi == null)
                        return fis = new FamilyInstance[0];
                    else
                        return fis = new FamilyInstance[1] { fi };
                }

                MEPCurve copyCurve = null;

                var connector1 = src.GetClosestConnector(dst.GetMiddleXYZ());
                var connector2 = dst.GetClosestConnector(src.GetMiddleXYZ());

                JudgeAndThrow(new Connector[] { connector1, connector2 }, fs, src);

                //创建管件
                copyCurve = src.GetCopy(doc);
                foreach (Connector con in copyCurve.ConnectorManager.Connectors)
                {
                    con.SetWidth(src.GetWidth() < dst.GetWidth() ? connector1 : connector2);
                }

                XYZ dir = Line.CreateBound(connector1.Origin, connector2.Origin).Direction;
                XYZ rotate = connector1.CoordinateSystem.BasisZ.RotateBy(connector1.CoordinateSystem.BasisZ.GetNormal(dir), angle);
                (copyCurve.Location as LocationCurve).Curve = Line.CreateBound(connector1.Origin, connector1.Origin + rotate);
                this.RevitDoc.Regenerate();

                //进行连接
                fis[0] = this.NewElbowPM(dst, copyCurve, fs);
                fis[1] = this.NewElbowPM(src, copyCurve, fs);

                return fis;
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionDefault(ex, src, MEPCurveConnectTypeENUM.Elbow);
                return fis;
            }
            finally
            {
                foreach (var fi in fis)
                {
                    fi.WriteParm(src.GetParmType(), false);
                    fi.WriteParm_ConnectedFitting(src.GetParmType());
                }
            }
        }

        protected void NewTee_CableTray(MEPCurve src, MEPCurve dst)
        {
            try
            {
                XYZ intersectWithPoint = src.GetIntersectWithPointLineExpandTwo(dst);
                if (intersectWithPoint == null)
                    throw new Exception("管线与管线之间未能相交");

                Connector connector1 = null, connector2 = null;
                if (src.IsOn(intersectWithPoint))
                {
                    connector1 = src.GetClosestConnector(intersectWithPoint);
                    connector2 = dst.GetClosestConnector(intersectWithPoint);
                }
                else
                {
                    connector2 = src.GetClosestConnector(intersectWithPoint);
                    connector1 = dst.GetClosestConnector(intersectWithPoint);
                }

                this.RevitDoc.Create.NewTeeFitting(connector1, connector1, connector2);
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionPM(ex, src, MEPCurveConnectTypeENUM.Tee);
            }
        }

        protected void NewCross_CableTray(MEPCurve src, MEPCurve dst)
        {
            try
            {
                XYZ intersectWithPoint = src.GetIntersectWithPointLineExpandTwo(dst);
                if (intersectWithPoint == null)
                    throw new Exception("管线与管线之间未能相交");

                Connector connector1 = null, connector2 = null, connector3 = null;
                MEPCurve copyCurve = dst.GetCopy(this.RevitDoc);
                connector1 = src.ConnectorManager.Lookup(0);
                connector2 = dst.ConnectorManager.Lookup(0);
                connector3 = copyCurve.ConnectorManager.Lookup(1);

                connector2.ChangeLinkedConnectorTo(connector3);
                //修改无连接件连接的连接口
                var line = Line.CreateBound(connector2.Origin, intersectWithPoint);
                foreach (Connector con in dst.ConnectorManager.Connectors)
                {
                    if (con.Id != 0 && con.Id != 1)
                    {
                        if (line.IsOn(line.GetClosestPoint(con.Origin)))
                        {
                            var linkedC = con.GetConnectedConnector();
                            linkedC.DisconnectFrom(con);
                            this.RevitDoc.Create.NewTeeFitting(connector3, connector3, linkedC);
                        }
                    }
                }

                this.RevitDoc.Create.NewTeeFitting(connector1, connector1, connector2);
                this.RevitDoc.Create.NewTeeFitting(connector1, connector1, connector3);
            }
            catch (Exception ex)
            {
                base.log.AddLog(ex);
                this.ThrowExceptionPM(ex, src, MEPCurveConnectTypeENUM.Cross);
            }
        }
        #endregion

        #region 辅助方法(private)
        private void StartSpecialCableTray(Dictionary<MEPCurve, bool> dic, MEPCurve[] meps)
        {
            dic.Clear();
            foreach (var mep in meps)
                dic.Add(mep, mep.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).AsValueString() == "无配件的电缆桥架");

            if (dic.Where(p => p.Value == true).Count() > 0)
            {
                var t = new FilteredElementCollector(this.RevitDoc).OfClass(typeof(CableTrayType)).FirstOrDefault(p => p.GetParameters("族名称").First().AsString() == "带配件的电缆桥架");
                foreach (var pair in dic)
                {
                    if (pair.Value)
                        pair.Key.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).Set(t.Id);
                }
            }
        }
        private void EndSpecialCableTray(Dictionary<MEPCurve, bool> dic)
        {
            if (dic.Where(p => p.Value == true).Count() > 0)
            {
                var t = new FilteredElementCollector(this.RevitDoc).OfClass(typeof(CableTrayType)).FirstOrDefault(p => p.GetParameters("族名称").First().AsString() == "无配件的电缆桥架");
                foreach (var pair in dic)
                {
                    if (pair.Value)
                        pair.Key.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM).Set(t.Id);
                }
            }
        }

        /// <summary>
        /// 判断连接点是否已连接，以及管件截面和管道截面是否一致
        /// </summary>
        /// <param name="connectors"></param>
        /// <param name="fs"></param>
        /// <param name="src"></param>
        /// <param name="doc"></param>
        private void JudgeAndThrow(Connector[] connectors, FamilySymbol fs, MEPCurve src)
        {
            FamilyInstance fiTemp = null;
            try
            {
                foreach (var con in connectors)
                {
                    if (con.IsConnected)
                        throw new Exception("连接点已连接!");

                    if (fs != null)
                    {
                        fiTemp = fiTemp == null ? this.RevitDoc.Create.NewFamilyInstance(new XYZ(0, 0, 0), fs, 0) : fiTemp;
                        if (fiTemp.MEPModel.ConnectorManager.Connectors.GetConnectorByIndex(0).Shape != con.Shape)
                        {
                            if (src is Duct)
                                throw new Exception("管件和风管的截面类型不匹配");
                            else if (src is Pipe)
                                throw new Exception("管件和管道的截面类型不匹配");
                        }
                    }
                }
            }
            finally
            {
                if (fiTemp != null)
                    this.RevitDoc.Delete(fiTemp.Id);
            }
        }
        /// <summary>
        /// 检测系统中是否有默认连接项，无则进行添加
        /// </summary>
        /// <param name="src"></param>
        /// <param name="type"></param>
        protected FamilySymbol Judge_LoadDefaultFitting(MEPCurve src, MEPCurveConnectTypeENUM type)
        {
            FamilySymbol fs = null;
            switch (type)
            {
                case MEPCurveConnectTypeENUM.Elbow://弯头
                    fs = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Elbows); break;
                case MEPCurveConnectTypeENUM.Tee://三通
                    fs = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Junctions); break;
                case MEPCurveConnectTypeENUM.Cross://四通
                    fs = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Crosses); break;
                case MEPCurveConnectTypeENUM.Transition://过渡件
                    fs = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Transitions); break;
                case MEPCurveConnectTypeENUM.TakeOff://侧接
                    fs = src.GetDefaultTakeoffFittingSymbol(); break;
                default:
                    fs = null;
                    break;
            }

            if (fs != null)
                return fs;

            if (this.IsOnlyUseRevitDefault)
                return null;

            var familyName = this.GetDefaultFittingName(src, type);
            if (familyName == null)
                return null;

            fs = FamilyLoadUtils.FindFamilySymbol_SubTransaction(this.RevitDoc, familyName, null);
            if (fs == null)
                return null;
            if (src is Pipe || src is Duct)
            {
                RoutingPreferenceManager rpm = src.GetMEPCurveType().RoutingPreferenceManager;
                var rule = new RoutingPreferenceRule(fs.Id, "");
                rule.AddCriterion(PrimarySizeCriterion.All());

                switch (type)
                {
                    case MEPCurveConnectTypeENUM.Elbow://弯头
                        rpm.AddRule(RoutingPreferenceRuleGroupType.Elbows, rule); break;
                    case MEPCurveConnectTypeENUM.Tee://三通
                        rpm.AddRule(RoutingPreferenceRuleGroupType.Junctions, rule); break;
                    case MEPCurveConnectTypeENUM.Cross://四通
                        rpm.AddRule(RoutingPreferenceRuleGroupType.Crosses, rule); break;
                    case MEPCurveConnectTypeENUM.Transition://过渡件
                        rpm.AddRule(RoutingPreferenceRuleGroupType.Transitions, rule); break;
                    case MEPCurveConnectTypeENUM.TakeOff://侧接
                        rpm.AddRule(RoutingPreferenceRuleGroupType.Junctions, rule); break;
                    default:
                        break;
                }

            }
            else if (src is Conduit)
            {
                Parameter param = null;

                switch (type)
                {
                    case MEPCurveConnectTypeENUM.Elbow://弯头
                        param = (src.Document.GetElement(src.GetTypeId())).GetParameters("弯头").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Tee://三通
                        param = (src.Document.GetElement(src.GetTypeId())).GetParameters("T 形三通").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Cross://四通
                        param = (src.Document.GetElement(src.GetTypeId())).GetParameters("交叉线").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Transition://过渡件
                        param = (src.Document.GetElement(src.GetTypeId())).GetParameters("过渡件").FirstOrDefault(); break;
                    default:
                        break;
                }

                if (param != null)
                {
                    param.Set(fs.Id);
                }
            }
            else if (src is CableTray)
            {
                Parameter param = null;
                var t = new FilteredElementCollector(src.Document).OfClass(typeof(CableTrayType)).FirstOrDefault(p => p.GetParameters("族名称").First().AsString() == "带配件的电缆桥架");
                switch (type)
                {
                    case MEPCurveConnectTypeENUM.Elbow://弯头
                        param = t.GetParameters("水平弯头").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Tee://三通
                        param = t.GetParameters("T 形三通").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Cross://四通
                        param = t.GetParameters("交叉线").FirstOrDefault(); break;
                    case MEPCurveConnectTypeENUM.Transition://过渡件
                        param = t.GetParameters("过渡件").FirstOrDefault(); break;
                    default:
                        break;
                }

                if (param != null)
                {
                    param.Set(fs.Id);
                }
            }
            return fs;
        }
        /// <summary>
        /// 获取设置文件中管件名称 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetDefaultFittingName(MEPCurve src, MEPCurveConnectTypeENUM type)
        {
            string searchText = "/Root/";
            if (src is Pipe)
            {
                searchText += "Pipe/";
            }
            else if (src is Duct)
            {
                var con = src.ConnectorManager.Lookup(0);
                if (con.Shape == ConnectorProfileType.Round)
                    searchText += "Duct/圆形/";
                else if (con.Shape == ConnectorProfileType.Rectangular)
                    searchText += "Duct/矩形/";
                else if (con.Shape == ConnectorProfileType.Oval)
                    searchText += "Duct/椭圆/";
            }
            else if (src is Conduit)
                searchText += "Conduit/";
            else if (src is CableTray)
                searchText += "CableTray/";

            switch (type)
            {
                case MEPCurveConnectTypeENUM.Elbow://弯头
                    searchText += "弯头"; break;
                case MEPCurveConnectTypeENUM.Tee://三通
                    searchText += "三通"; break;
                case MEPCurveConnectTypeENUM.Cross://四通
                    searchText += "四通"; break;
                case MEPCurveConnectTypeENUM.Transition://过渡件
                    searchText += "过渡件"; break;
                case MEPCurveConnectTypeENUM.TakeOff://侧接
                    searchText += "侧接"; break;
                default:
                    break;
            }

            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
            xml.Load(FamilyLoadUtils.FamilyLibPath + "\\..\\..\\SysData\\MEPCurveConnect.xml");
            var node = xml.SelectSingleNode(searchText);
            if (node == null)
                return null;
            else
                return node.InnerText;
        }

        private void ThrowExceptionDefault(Exception ex, MEPCurve src, MEPCurveConnectTypeENUM type)
        {
            if (ex.Message == "连接点已连接")
                throw new Exception("未能创建管件：" + ex.Message);

            if (ex.Message.Contains("连接失败"))
                throw ex;


            bool isHas = false;
            switch (type)
            {
                case MEPCurveConnectTypeENUM.Elbow:
                    isHas = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Elbows) == null ? false : true; break;
                case MEPCurveConnectTypeENUM.Tee:
                    isHas = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Junctions) == null ? false : true; break;
                case MEPCurveConnectTypeENUM.Cross:
                    isHas = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Crosses) == null ? false : true; break;
                case MEPCurveConnectTypeENUM.Transition:
                    isHas = src.GetDefaultFittingSymbol(RoutingPreferenceRuleGroupType.Transitions) == null ? false : true; break;
                case MEPCurveConnectTypeENUM.TakeOff:
                    isHas = src.GetDefaultTakeoffFittingSymbol() == null ? false : true; break;
                default:
                    isHas = false;
                    break;
            }

            if (ex.Message.Contains("failed to insert"))
                ex = new Exception("创建相应管件失败");

            switch (type)
            {
                case MEPCurveConnectTypeENUM.Elbow:
                    throw new Exception((ex.Message.Contains("未能创建弯头管件：") ? "" : "未能创建弯头管件：") + (!isHas ? "未能加载布管系统配置的默认项" : ex.Message));
                case MEPCurveConnectTypeENUM.Tee:
                    throw new Exception((ex.Message.Contains("未能创建三通管件：") ? "" : "未能创建三通管件：") + (!isHas ? "未能加载布管系统配置的默认项" : ex.Message));
                case MEPCurveConnectTypeENUM.Cross:
                    throw new Exception((ex.Message.Contains("未能创建四通管件：") ? "" : "未能创建四通管件：") + (!isHas ? "未能加载布管系统配置的默认项" : ex.Message));
                case MEPCurveConnectTypeENUM.Transition:
                    throw new Exception((ex.Message.Contains("未能创建连接管件：") ? "" : "未能创建连接管件：") + (!isHas ? "未能加载布管系统配置的默认项" : ex.Message));
                case MEPCurveConnectTypeENUM.TakeOff:
                    throw new Exception((ex.Message.Contains("未能创建侧连接管件：") ? "" : "未能创建侧连接管件：") + (!isHas ? "未能加载布管系统配置的默认项" : ex.Message));
                default:
                    throw new Exception((ex.Message.Contains("未能创建管件：") ? "" : "未能创建管件：") + ex.Message);
            }
        }
        private void ThrowExceptionPM(Exception ex, MEPCurve src, MEPCurveConnectTypeENUM type)
        {
            if (ex.Message == "连接点已连接")
                throw new Exception("未能创建管件：" + ex.Message);

            if (ex.Message.Contains("failed to insert"))
                ex = new Exception("创建相应管件失败");

            switch (type)
            {
                case MEPCurveConnectTypeENUM.Elbow:
                    throw new Exception("未能创建弯头管件：" + (ex.Message));
                case MEPCurveConnectTypeENUM.Tee:
                    throw new Exception("未能创建三通管件：" + (ex.Message));
                case MEPCurveConnectTypeENUM.Cross:
                    throw new Exception("未能创建四通管件：" + (ex.Message));
                case MEPCurveConnectTypeENUM.Transition:
                    throw new Exception("未能创建连接管件：" + (ex.Message));
                default:
                    throw new Exception("未能创建管件：" + ex.Message);
            }
        }
        #endregion
    }
}
