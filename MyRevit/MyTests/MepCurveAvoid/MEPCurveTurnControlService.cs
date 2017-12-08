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
    enum TurnDirectionModeENUM { Up, Down, Null }
    public interface IMEPCurveTurnControlService
    {
        void AutomaticTurn(List<MEPCurve> meps);
    }

    class MEPCurveTurnControlService : MEPCurveConnectControlService, IMEPCurveTurnControlService
    {

        private List<MultiMEPCurveTurnBase> multiTurnRules = new List<MultiMEPCurveTurnBase>();
        private List<SingleMEPCurveTurnBase> singleTurnRules = new List<SingleMEPCurveTurnBase>();
        private Dictionary<Type, double> angleDic = new Dictionary<Type, double>();

        public MEPCurveTurnControlService(UIApplication uiApp)
            : base(uiApp)
        {
            this.multiTurnRules.Add(new MultiMEPCurveTurn_YaLi());
            this.multiTurnRules.Add(new MultiMEPCurveTurn_WuYaLi());
            this.multiTurnRules.Add(new MultiMEPCurveTurn_NTDuct());
            this.multiTurnRules.Add(new MultiMEPCurveTurn_NTPipe());
            this.multiTurnRules.Add(new MultiMEPCurveTurn_DQ());

            this.singleTurnRules.Add(new MEPCurveTurn_Same_Same());
            this.singleTurnRules.Add(new MEPCurveTurn_YaLi_WuYaLi());
            this.singleTurnRules.Add(new MEPCurveTurn_Cold_Hot());
            this.singleTurnRules.Add(new MEPCurveTurn_YaLiPipe_Duct());
            this.singleTurnRules.Add(new MEPCurveTurn_YaLiPipe_NTPipe());
            this.singleTurnRules.Add(new MEPCurveTurn_YaLiPipe_DQ());
            this.singleTurnRules.Add(new MEPCurveTurn_Duct_WuYaLiPipe());
            this.singleTurnRules.Add(new MEPCurveTurn_NTPipe_WuYaLiPipe());
            this.singleTurnRules.Add(new MEPCurveTurn_DQ_WuYaLiPipe());
            this.singleTurnRules.Add(new MEPCurveTurn_NTPipe_NTDuct());
            this.singleTurnRules.Add(new MEPCurveTurn_DQ_NTDuct());
            this.singleTurnRules.Add(new MEPCurveTurn_DQ_NTPipe());

            this.angleDic.Add(typeof(Pipe), 90);
            this.angleDic.Add(typeof(Duct), 90);
            this.angleDic.Add(typeof(Conduit), 45);
            this.angleDic.Add(typeof(CableTray), 45);
        }

        public void AutomaticTurn(List<MEPCurve> meps)
        {
            List<ToTurnEnt> ents = this.GetTurnEnts(meps);

            foreach (var ent in ents)
            {
                var nowTurnMeps = ent.NowTurnMeps;
                if (nowTurnMeps.Count > 1)
                {
                    this.DealWithMulti(ent.BaseMep, nowTurnMeps);
                }
                else if (nowTurnMeps.Count == 1)
                {
                    //判断所属的绕弯规则，并进行绕弯
                    this.DealWithSingle(ent.BaseMep, nowTurnMeps.First());
                }
            }
        }

        /// <summary>
        /// 获得所有碰撞的数据
        /// </summary>
        /// <param name="meps"></param>
        /// <returns></returns>
        private List<ToTurnEnt> GetTurnEnts(List<MEPCurve> meps)
        {
            List<ToTurnEnt> ents = new List<ToTurnEnt>();
            for (int i = 0; i < meps.Count; i++)
            {
                ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(meps[i]);
                for (int j = i + 1; j < meps.Count; j++)
                {
                    if (filter.PassesFilter(meps[j]) && !meps[i].IsParallelTo(meps[j]))
                    {
                        var ent = ents.FirstOrDefault(p => p.BaseMep == meps[i]);
                        if (ent == null)
                            ents.Add(ent = new ToTurnEnt() { BaseMep = meps[i], TurnMeps = new List<MEPCurve>() });
                        ent.TurnMeps.Add(meps[j]);

                        ent = null;
                        ent = ents.FirstOrDefault(p => p.BaseMep == meps[j]);
                        if (ent == null)
                            ents.Add(ent = new ToTurnEnt() { BaseMep = meps[j], TurnMeps = new List<MEPCurve>() });
                        ent.TurnMeps.Add(meps[i]);
                    }
                }
            }
            ents.Sort((e1, e2) =>
            {
                var temp = e2.TurnMeps.Count.CompareTo(e1.TurnMeps.Count);
                if (temp == 0)
                    return e2.MaxDistance.CompareTo(e1.MaxDistance);
                else
                    return temp;
            });
            return ents;
        }

        /// <summary>
        /// 处理多管碰撞
        /// </summary>
        /// <param name="mep"></param>
        /// <param name="meps"></param>
        /// <returns></returns>
        private bool DealWithMulti(MEPCurve mep, List<MEPCurve> meps)
        {
            foreach (var rule in this.multiTurnRules)
            {
                var result = rule.IsKeepingWith(mep, meps);
                if (result == MultiMEPCurveTurnBase.MultiTurnResultENUM.SingleTurn)
                {
                    var paramEnts = this.CalcAndGetParamEnts(mep, meps);
                    foreach (var ent in paramEnts)
                    {
                        this.DoTurn(ent.MoveMep, ent.StaticMeps.First(), ent.VDis, ent.Angle, ent.TurnDirectionMode, ent.XYZ1, ent.XYZ2);
                    }
                    return true;
                }
                else if (result == MultiMEPCurveTurnBase.MultiTurnResultENUM.MultiTurn)
                {
                    foreach (var item in meps)
                    {
                        var paramEnt = this.CalcAndGetParamEnt(item, mep);
                        if (paramEnt.ResultType == SingleTurnParamEnt.ResultTypeENUM.Success)
                            this.DoTurn(paramEnt.MoveMep, paramEnt.StaticMep, paramEnt.VDis, paramEnt.Angle, paramEnt.TurnDirectionMode, paramEnt.XYZ1, paramEnt.XYZ2);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 处理单管碰撞
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private bool DealWithSingle(MEPCurve mep1, MEPCurve mep2)
        {
            foreach (var rule in this.singleTurnRules)//循环规则
            {
                if (rule.IsKeepingWith(mep1, mep2))
                {
                    var paramEnt = this.CalcAndGetParamEnt(rule.MoveMEP, rule.StaticMEP);
                    if (paramEnt.ResultType == SingleTurnParamEnt.ResultTypeENUM.Success)
                        this.DoTurn(paramEnt.MoveMep, paramEnt.StaticMep, paramEnt.VDis, paramEnt.Angle, paramEnt.TurnDirectionMode, paramEnt.XYZ1, paramEnt.XYZ2);
                    else if (paramEnt.ResultType == SingleTurnParamEnt.ResultTypeENUM.EndTurn)
                    {
                        this.DoTurnCross(paramEnt.VDis, paramEnt.Angle, paramEnt.TurnDirectionMode, paramEnt.EndTurnEnts);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获得多管绕弯的位置数据
        /// </summary>
        /// <param name="mep"></param>
        /// <param name="meps"></param>
        /// <returns></returns>
        private List<MultiTurnParamEnt> CalcAndGetParamEnts(MEPCurve mep, List<MEPCurve> meps)
        {
            var con = mep.ConnectorManager.Connectors.GetConnectorById(0);
            var singleTurnParamEnts = meps.Select(p => this.CalcAndGetParamEnt(mep, p)).Where(p => p.ResultType == SingleTurnParamEnt.ResultTypeENUM.Success).ToList();
            singleTurnParamEnts.Sort((p1, p2) => { return con.Origin.DistanceTo(p1.XYZ1).CompareTo(con.Origin.DistanceTo(p2.XYZ1)); });

            List<MultiTurnParamEnt> ents = new List<MultiTurnParamEnt>();
            foreach (var item in singleTurnParamEnts)
            {
                if (ents.Count == 0)
                {
                    MultiTurnParamEnt ent = new MultiTurnParamEnt();
                    ent.MoveMep = mep;
                    ent.StaticMeps = new List<MEPCurve> { item.StaticMep };
                    ent.TurnDirectionMode = item.TurnDirectionMode;
                    ent.Angle = item.Angle;
                    ent.VDis = item.VDis;
                    ent.XYZ1 = item.XYZ1;
                    ent.XYZ2 = item.XYZ2;
                    ents.Add(ent);
                }
                else
                {
                    var ent = ents.Last();
                    var line = Line.CreateBound(ent.XYZ1, ent.XYZ2);
                    line = Line.CreateBound(ent.XYZ1 - line.Direction * item.Length, ent.XYZ2 + line.Direction * item.Length);
                    if (!line.IsOn(item.XYZ1 - line.Direction * item.Length))
                    {
                        MultiTurnParamEnt ent1 = new MultiTurnParamEnt();
                        ent1.MoveMep = mep;
                        ent1.StaticMeps = new List<MEPCurve> { item.StaticMep };
                        ent1.TurnDirectionMode = item.TurnDirectionMode;
                        ent1.Angle = item.Angle;
                        ent1.VDis = item.VDis;
                        ent1.XYZ1 = item.XYZ1;
                        ent1.XYZ2 = item.XYZ2;
                        ents.Add(ent1);
                    }
                    else
                    {
                        ent.StaticMeps.Add(item.StaticMep);
                        ent.VDis = item.VDis.IsGreaterThan(ent.VDis) ? item.VDis : ent.VDis;
                        ent.XYZ2 = line.IsOn(item.XYZ2) ? ent.XYZ2 : item.XYZ2;
                    }
                }
            }

            return ents;
        }

        /// <summary>
        /// 获得单管绕弯的位置数据
        /// </summary>
        /// <param name="moveMep"></param>
        /// <param name="staticMep"></param>
        /// <returns></returns>
        private SingleTurnParamEnt CalcAndGetParamEnt(MEPCurve moveMep, MEPCurve staticMep)
        {
            SingleTurnParamEnt ent = new SingleTurnParamEnt() { ResultType = SingleTurnParamEnt.ResultTypeENUM.Error, EndTurnEnts = new List<EndTurnEnt>() };
            try
            {
                double angle;
                double widthX, widthY, length, heightDisUp, heightDisDown;
                double vDisUp, hDisUp, vDisDown, hDisDown;
                XYZ xyz1Up, xyz2Up, xyz1Down, xyz2Down;
                TurnDirectionModeENUM turnDirectionMode = TurnDirectionModeENUM.Null;

                angle = this.angleDic[moveMep.GetType()];
                if (!this.GetFittingData(moveMep, angle, out widthX, out widthY, out length))
                    return ent;

                #region 计算heightDisUp,heightDisDown
                var lineMove = moveMep.GetLine();
                var lineMoveH = Line.CreateUnbound(lineMove.Origin.Add(new XYZ(0, 0, -lineMove.Origin.Z)), lineMove.Direction.Add(new XYZ(0, 0, -lineMove.Direction.Z)).Normalize());

                var lineStatic = staticMep.GetLine();
                var lineStaticH = Line.CreateUnbound(lineStatic.Origin.Add(new XYZ(0, 0, -lineStatic.Origin.Z)), lineStatic.Direction.Add(new XYZ(0, 0, -lineStatic.Direction.Z)).Normalize());

                var xyz = lineMoveH.GetIntersectWithPoint(lineStaticH);
                var lineV = Line.CreateUnbound(xyz, XYZ.BasisZ);

                var xyzMove = lineV.GetIntersectWithPoint(lineMove);
                var xyzStatic = lineV.GetIntersectWithPoint(lineStatic);

                heightDisUp = xyzMove.Z.IsLessThanEqualTo(xyzStatic.Z) ? xyzMove.DistanceTo(xyzStatic) : 0;
                heightDisDown = xyzMove.Z.IsGreaterThanEqualTo(xyzStatic.Z) ? xyzMove.DistanceTo(xyzStatic) : 0;
                #endregion

                #region 计算vDis，hDis，xyz1，xyz2
                //计算上翻距离
                vDisUp = (staticMep.GetExternalHeight() / 2 + heightDisUp + widthY);
                vDisUp = (2 * widthY) > vDisUp ? (2 * widthY) : vDisUp;
                vDisUp = vDisUp + UnitTransUtils.MMToFeet(5);

                if (angle.IsAlmostEqualTo(90, 0.001))
                {
                    hDisUp = (moveMep.GetExternalWidth() / 2 + staticMep.GetExternalWidth() / 2);
                    hDisUp = widthX > hDisUp ? widthX : hDisUp;
                    hDisUp = hDisUp + UnitTransUtils.MMToFeet(5);
                }
                else
                {
                    var tempWidth = staticMep.GetExternalWidth() / 2 + UnitTransUtils.MMToFeet(5);
                    hDisUp = length + vDisUp / Math.Tan(Math.PI * angle / 180) + staticMep.GetExternalWidth() / 2 + UnitTransUtils.MMToFeet(5);
                    hDisUp = hDisUp.IsGreaterThan(tempWidth) ? hDisUp : tempWidth;
                }

                xyz1Up = moveMep.GetLine().GetClosestPoint(staticMep.GetLine()) + moveMep.ConnectorManager.Connectors.GetConnectorById(0).CoordinateSystem.BasisZ.Multiply(hDisUp);
                xyz2Up = moveMep.GetLine().GetClosestPoint(staticMep.GetLine()) + moveMep.ConnectorManager.Connectors.GetConnectorById(1).CoordinateSystem.BasisZ.Multiply(hDisUp);

                //计算下翻距离
                vDisDown = (staticMep.GetExternalHeight() / 2 + heightDisDown + widthY);
                vDisDown = (2 * widthY) > vDisDown ? (2 * widthY) : vDisDown;
                vDisDown = vDisDown + UnitTransUtils.MMToFeet(5);

                if (angle.IsAlmostEqualTo(90, 0.001))
                {
                    hDisDown = (moveMep.GetExternalWidth() / 2 + staticMep.GetExternalWidth() / 2);
                    hDisDown = widthX > hDisDown ? widthX : hDisDown;
                    hDisDown = hDisDown + UnitTransUtils.MMToFeet(5);
                }
                else
                {
                    var tempWidth = staticMep.GetExternalWidth() / 2 + UnitTransUtils.MMToFeet(5);
                    hDisDown = length + vDisDown / Math.Tan(Math.PI * angle / 180) + UnitTransUtils.MMToFeet(5);
                    hDisDown = hDisDown.IsGreaterThan(tempWidth) ? hDisDown : tempWidth;
                }

                xyz1Down = moveMep.GetLine().GetClosestPoint(staticMep.GetLine()) + moveMep.ConnectorManager.Connectors.GetConnectorById(0).CoordinateSystem.BasisZ.Multiply(hDisDown);
                xyz2Down = moveMep.GetLine().GetClosestPoint(staticMep.GetLine()) + moveMep.ConnectorManager.Connectors.GetConnectorById(1).CoordinateSystem.BasisZ.Multiply(hDisDown);
                #endregion

                #region 计算方向,判断上下翻哪个可行
                var connector1 = moveMep.ConnectorManager.Connectors.GetConnectorById(0);
                var id = connector1.CoordinateSystem.BasisZ.GetNormal(staticMep.ConnectorManager.Lookup(0).CoordinateSystem.BasisZ);

                //获得方向矩阵
                var transUp = Transform.CreateTranslation(xyz1Up + (id.AngleTo(XYZ.BasisZ).IsLessThanEqualTo(Math.PI / 2, 0.001) ? id : -id) * vDisUp - connector1.Origin);
                var transDown = Transform.CreateTranslation(xyz1Up + (id.AngleTo(XYZ.BasisZ).IsGreaterThan(Math.PI / 2) ? id : -id) * vDisUp - connector1.Origin);

                //获得转换后的线
                var face = this.GetInSideFace(moveMep, moveMep.ConnectorManager.Lookup(0).Origin);
                CurveLoop curveLoopUp = new CurveLoop();
                CurveLoop curveLoopDown = new CurveLoop();
                foreach (Curve curve in face.GetCurveArr())
                {
                    curveLoopUp.Append(curve.CreateTransformed(transUp));
                    curveLoopDown.Append(curve.CreateTransformed(transDown));
                }

                //获得实体
                var solidUp = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoopUp }, -connector1.CoordinateSystem.BasisZ, xyz1Up.DistanceTo(xyz2Up));
                var solidDown = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveLoopDown }, -connector1.CoordinateSystem.BasisZ, xyz1Down.DistanceTo(xyz2Down));

                var elements = new FilteredElementCollector(this.RevitDoc).WhereElementIsViewIndependent().ToList();
                elements = new List<Element> { };
                if (this.IsSolidCutElements(solidUp, elements))
                {
                    if (this.IsSolidCutElements(solidDown, elements))
                    {
                        turnDirectionMode = TurnDirectionModeENUM.Null;
                        goto End;
                    }
                    else
                    {
                        turnDirectionMode = TurnDirectionModeENUM.Down;
                        goto End;
                    }
                }
                else
                {
                    turnDirectionMode = TurnDirectionModeENUM.Up;
                    goto End;
                }
                #endregion

                End:
                if (turnDirectionMode != TurnDirectionModeENUM.Null)
                {
                    ent.MoveMep = moveMep;
                    ent.StaticMep = staticMep;
                    ent.Angle = angle;
                    ent.TurnDirectionMode = turnDirectionMode;
                    ent.WidthX = widthX;
                    ent.WidthY = widthY;
                    ent.Length = length;
                    ent.HeightDis = turnDirectionMode == TurnDirectionModeENUM.Up ? heightDisUp : heightDisDown;
                    ent.HDis = turnDirectionMode == TurnDirectionModeENUM.Up ? hDisUp : hDisDown;
                    ent.VDis = turnDirectionMode == TurnDirectionModeENUM.Up ? vDisUp : vDisDown;
                    ent.XYZ1 = turnDirectionMode == TurnDirectionModeENUM.Up ? xyz1Up : xyz1Down;
                    ent.XYZ2 = turnDirectionMode == TurnDirectionModeENUM.Up ? xyz2Up : xyz2Down;

                    if (moveMep.IsOn(ent.XYZ1) && moveMep.IsOn(ent.XYZ1))
                    {
                        ent.ResultType = SingleTurnParamEnt.ResultTypeENUM.Success;
                    }
                    else if (moveMep.IsOn(ent.XYZ1) || moveMep.IsOn(ent.XYZ2))
                    {

                        var outXYZ = moveMep.IsOn(ent.XYZ1) ? ent.XYZ2 : ent.XYZ1;//离开了管道的点
                        var con = moveMep.GetClosestConnector(ent.XYZ2);//离开点一端的连接点
                        var fiCon = con.GetConnectedConnector();

                        ent.EndTurnEnts.Add(new EndTurnEnt() { NextMoveMep = moveMep, Con = con, XYZ = moveMep.IsOn(ent.XYZ1) ? ent.XYZ1 : ent.XYZ2 });
                        if (fiCon != null && fiCon.Owner is FamilyInstance)
                        {
                            var fi = fiCon.Owner as FamilyInstance;
                            foreach (Connector item in fi.MEPModel.ConnectorManager.Connectors)
                            {
                                if (item.Id != fiCon.Id)
                                {
                                    var mepCon = item.GetConnectedConnector();
                                    if (mepCon != null && mepCon.Owner is MEPCurve)
                                        ent.EndTurnEnts.Add(new EndTurnEnt() { NextMoveMep = mepCon.Owner as MEPCurve, Con = item, XYZ = mepCon.Origin - mepCon.CoordinateSystem.BasisZ * ent.HDis });
                                }
                            }
                        }

                        if (ent.EndTurnEnts.Count == 1)
                            ent.ResultType = SingleTurnParamEnt.ResultTypeENUM.Error;
                        else
                            ent.ResultType = SingleTurnParamEnt.ResultTypeENUM.EndTurn;
                    }
                }
            }
            catch (Exception ex)
            {
                ent.ErrorEX = ex;
                return ent;
            }
            return ent;
        }

        private bool IsSolidCutElements(Solid solid, IList<Element> elements)
        {
            ElementIntersectsSolidFilter filter = new ElementIntersectsSolidFilter(solid);
            bool isIntersectUp = false;
            foreach (var element in elements)
            {
                if (filter.PassesFilter(element))
                {
                    isIntersectUp = true;
                    break;
                }
            }

            return isIntersectUp;
        }

        /// <summary>
        /// 计算管道所用管件的WidthX，widthY，Length
        /// </summary>
        /// <param name="mep"></param>
        /// <param name="angle"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        private bool GetFittingData(MEPCurve mep, double angle, out double widthX, out double widthY, out double length)
        {
            widthX = 0;
            widthY = 0;
            length = 0;

            Connector con1 = null, con2 = null;
            Line line1 = null, line2 = null;
            FamilySymbol fs = null;
            if (mep is CableTray)
            {
                var param = (mep as CableTray).GetMEPCurveType().GetParameters("垂直内弯头");
                if (param.Count != 0)
                    fs = this.RevitDoc.GetElement(param.First().AsElementId()) as FamilySymbol;
            }
            else
                fs = this.Judge_LoadDefaultFitting(mep, MEPCurveConnectTypeENUM.Elbow);
            if (fs == null)
                return false;

            Transaction trans = new Transaction(mep.Document);
            try
            {
                trans.Start("临时");
                var fi = this.RevitDoc.Create.NewFamilyInstance(XYZ.Zero, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                var angleP = fi.GetParameters("角度").FirstOrDefault();
                if (angleP != null)
                    angleP.Set(Math.PI * angle / 180);
                foreach (Connector con in fi.MEPModel.ConnectorManager.Connectors)
                {
                    if (mep is CableTray || (mep is Duct && con.Shape == ConnectorProfileType.Rectangular))
                        con.SetWidth(mep.ConnectorManager.Lookup(0), true);
                    else
                        con.SetWidth(mep.ConnectorManager.Lookup(0));
                    if (con1 == null)
                        con1 = con;
                    else
                        con2 = con;
                }
                this.RevitDoc.Regenerate();

                //生成出口直线
                line1 = Line.CreateUnbound(con1.Origin, con1.CoordinateSystem.BasisZ);
                line2 = Line.CreateUnbound(con2.Origin, con2.CoordinateSystem.BasisZ);
                length = line1.GetIntersectWithPoint(line2).DistanceTo(con1.Origin);

                //计算X向和Y向的距离
                widthX = Math.Abs(con1.Origin.X - con2.Origin.X);
                widthY = Math.Abs(con1.Origin.Y - con2.Origin.Y);

                trans.RollBack();
            }
            catch
            {
                if (trans.HasStarted())
                    trans.RollBack();

                return false;
            }
            return true;
        }

        /// <summary>
        /// 实现绕弯
        /// </summary>
        /// <param name="moveMep"></param>
        /// <param name="staticMep"></param>
        /// <param name="verticalDis"></param>
        /// <param name="angle"></param>
        /// <param name="turnDirMode"></param>
        /// <param name="xyz1"></param>
        /// <param name="xyz2"></param>
        /// <returns></returns>
        private int DoTurn(MEPCurve moveMep, MEPCurve staticMep, double verticalDis, double angle, TurnDirectionModeENUM turnDirMode, XYZ xyz1, XYZ xyz2)
        {
            Connector connector1 = null, connector2 = null;
            MEPCurve copyMep = null, copyMepCross = null, copyMepStand1 = null, copyMepStand2 = null;

            FailuresPreprocessor failure = new FailuresPreprocessor();
            Transaction trans = new Transaction(this.RevitDoc);
            try
            {
                trans.Start("全自动避让");

                //创建管道
                copyMep = moveMep.GetCopy(moveMep.Document);
                copyMepCross = moveMep.GetCopy(moveMep.Document);
                copyMepStand1 = moveMep.GetCopy(moveMep.Document);
                copyMepStand2 = moveMep.GetCopy(moveMep.Document);

                //确定连接点,并重新连接
                connector1 = moveMep.ConnectorManager.Connectors.GetConnectorById(0);
                connector2 = copyMep.ConnectorManager.Connectors.GetConnectorById(1);
                connector1.ChangeLinkedConnectorTo(copyMep.ConnectorManager.Connectors.GetConnectorById(0));

                //调整被选中管道位置
                connector1.Origin = connector1.Origin.DistanceTo(xyz1) > connector1.Origin.DistanceTo(xyz2) ? xyz1 : xyz2;
                connector2.Origin = connector2.Origin.DistanceTo(xyz1) > connector2.Origin.DistanceTo(xyz2) ? xyz1 : xyz2;
                this.RevitDoc.Regenerate();

                //调整两根斜管的位置
                var normal1 = this.GetNormalForMove(moveMep, staticMep, connector1, turnDirMode);
                (copyMepStand1.Location as LocationCurve).Curve = Line.CreateBound(connector1.Origin, connector1.Origin + connector1.CoordinateSystem.BasisZ * verticalDis);
                ElementTransformUtils.RotateElement(
                    this.RevitDoc,
                    copyMepStand1.Id,
                    Line.CreateBound(connector1.Origin, connector1.Origin + normal1),
                     -angle * Math.PI / 180);

                var normal2 = this.GetNormalForMove(moveMep, staticMep, connector2, turnDirMode);
                (copyMepStand2.Location as LocationCurve).Curve = Line.CreateBound(connector2.Origin, connector2.Origin + connector2.CoordinateSystem.BasisZ * verticalDis);
                ElementTransformUtils.RotateElement(
                    this.RevitDoc,
                    copyMepStand2.Id,
                    Line.CreateBound(connector2.Origin, connector2.Origin + normal2),
                      -angle * Math.PI / 180);

                //调整上移管的位置
                (copyMepCross.Location as LocationCurve).Curve = Line.CreateBound(connector1.Origin, connector2.Origin);
                XYZ moveDir = connector1.CoordinateSystem.BasisZ.RotateBy(normal1, -90);
                ElementTransformUtils.MoveElement(
                    this.RevitDoc,
                    copyMepCross.Id,
                     moveDir * verticalDis);

                this.RevitDoc.Regenerate();

                this.NewTwoFitting(moveMep, copyMepStand1, null);
                this.NewTwoFitting(copyMepCross, copyMepStand1, null);
                this.NewTwoFitting(copyMepCross, copyMepStand2, null);
                this.NewTwoFitting(copyMep, copyMepStand2, null);

                if (trans.Commit(trans.GetFailureHandlingOptions().SetClearAfterRollback(true).SetFailuresPreprocessor(failure)) == TransactionStatus.RolledBack)
                    return 0;
                else
                    return 1;
            }
            catch
            {
                return 0;
            }
            finally
            {
                if (trans.GetStatus() == TransactionStatus.Started)
                    trans.RollBack(trans.GetFailureHandlingOptions().SetClearAfterRollback(true).SetFailuresPreprocessor(failure));
            }
        }

        public int DoTurnCross(double verticalDis, double angle, TurnDirectionModeENUM turnDirMode, List<EndTurnEnt> turnEnts)
        {
            FailuresPreprocessor failure = new FailuresPreprocessor();
            Transaction trans = new Transaction(this.RevitDoc);
            try
            {
                trans.Start("全自动避让");

                foreach (var ent in turnEnts)
                {
                    //创建管道
                    ent.CopyMepCross = ent.NextMoveMep.GetCopy(this.RevitDoc);
                    ent.CopyMepStand = ent.NextMoveMep.GetCopy(this.RevitDoc);

                    //确定连接点,并重新连接
                    ent.KeyCon = ent.NextMoveMep.GetClosestConnector(ent.Con.Origin);
                    var connector2 = ent.CopyMepCross.ConnectorManager.Connectors.GetConnectorById(ent.KeyCon.Id == 0 ? 1 : 0);
                    ent.KeyCon.ChangeLinkedConnectorTo(ent.CopyMepCross.ConnectorManager.Connectors.GetConnectorById(ent.KeyCon.Id == 0 ? 0 : 1));

                    //调整被选中管道位置
                    ent.KeyCon.Origin = connector2.Origin = ent.XYZ;
                }

                this.RevitDoc.Regenerate();

                foreach (var ent in turnEnts)
                {
                    //调整两根斜管的位置
                    var normal1 = ent.KeyCon.CoordinateSystem.BasisZ.GetNormal(turnDirMode == TurnDirectionModeENUM.Up ? -XYZ.BasisZ : XYZ.BasisZ);
                    (ent.CopyMepStand.Location as LocationCurve).Curve = Line.CreateBound(ent.KeyCon.Origin, ent.KeyCon.Origin + ent.KeyCon.CoordinateSystem.BasisZ * verticalDis);
                    ElementTransformUtils.RotateElement(
                        this.RevitDoc,
                        ent.CopyMepStand.Id,
                        Line.CreateBound(ent.KeyCon.Origin, ent.KeyCon.Origin + normal1),
                         -angle * Math.PI / 180);

                    //调整上移管的位置
                    XYZ moveDir = ent.KeyCon.CoordinateSystem.BasisZ.RotateBy(normal1, -90);
                    if (turnEnts.IndexOf(ent) == 0)
                        ElementTransformUtils.MoveElement(
                            this.RevitDoc,
                            ent.CopyMepCross.Id,
                            moveDir * verticalDis);

                    this.RevitDoc.Regenerate();

                    this.NewTwoFitting(ent.NextMoveMep, ent.CopyMepStand, null);
                    this.NewTwoFitting(ent.CopyMepCross, ent.CopyMepStand, null);
                }

                if (trans.Commit(trans.GetFailureHandlingOptions().SetClearAfterRollback(true).SetFailuresPreprocessor(failure)) == TransactionStatus.RolledBack)
                    return 0;
                else
                    return 1;
            }
            catch
            {
                return 0;
            }
            finally
            {
                if (trans.GetStatus() == TransactionStatus.Started)
                    trans.RollBack(trans.GetFailureHandlingOptions().SetClearAfterRollback(true).SetFailuresPreprocessor(failure));
            }
        }

        private int DoTurn(MEPCurve moveMep, double verticalDis, double angle, TurnDirectionModeENUM turnDirMode, XYZ xyz, Connector con)
        {
            Connector connector1 = null, connector2 = null;
            MEPCurve copyMep = null, copyMepStand1 = null;

            FailuresPreprocessor failure = new FailuresPreprocessor();
            Transaction trans = new Transaction(this.RevitDoc);
            try
            {
                trans.Start("全自动避让");

                //创建管道
                copyMep = moveMep.GetCopy(moveMep.Document);
                copyMepStand1 = moveMep.GetCopy(moveMep.Document);

                //确定连接点,并重新连接
                connector1 = moveMep.GetClosestConnector(con.Origin);
                connector2 = copyMep.ConnectorManager.Connectors.GetConnectorById(connector1.Id == 0 ? 1 : 0);
                connector1.ChangeLinkedConnectorTo(copyMep.ConnectorManager.Connectors.GetConnectorById(connector1.Id == 0 ? 0 : 1));

                //调整被选中管道位置
                connector1.Origin = connector2.Origin = xyz;
                this.RevitDoc.Regenerate();

                //调整两根斜管的位置
                var normal1 = connector1.CoordinateSystem.BasisZ.GetNormal(turnDirMode == TurnDirectionModeENUM.Up ? -XYZ.BasisZ : XYZ.BasisZ);
                (copyMepStand1.Location as LocationCurve).Curve = Line.CreateBound(connector1.Origin, connector1.Origin + connector1.CoordinateSystem.BasisZ * verticalDis);
                ElementTransformUtils.RotateElement(
                    this.RevitDoc,
                    copyMepStand1.Id,
                    Line.CreateBound(connector1.Origin, connector1.Origin + normal1),
                     -angle * Math.PI / 180);

                //调整上移管的位置
                XYZ moveDir = connector1.CoordinateSystem.BasisZ.RotateBy(normal1, -90);
                ElementTransformUtils.MoveElement(
                    this.RevitDoc,
                    copyMep.Id,
                     moveDir * verticalDis);


                this.RevitDoc.Regenerate();

                this.NewTwoFitting(moveMep, copyMepStand1, null);
                this.NewTwoFitting(copyMep, copyMepStand1, null);

                if (trans.Commit(trans.GetFailureHandlingOptions().SetClearAfterRollback(true).SetFailuresPreprocessor(failure)) == TransactionStatus.RolledBack)
                    return 0;
                else
                    return 1;
            }
            catch
            {
                return 0;
            }
            finally
            {
                if (trans.GetStatus() == TransactionStatus.Started)
                    trans.RollBack(trans.GetFailureHandlingOptions().SetClearAfterRollback(true).SetFailuresPreprocessor(failure));
            }
        }

        /// <summary>
        /// 获得绕弯方向的向量
        /// </summary>
        /// <param name="mep"></param>
        /// <param name="dst"></param>
        /// <param name="connector1"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private XYZ GetNormalForMove(MEPCurve mep, MEPCurve dst, Connector connector1, TurnDirectionModeENUM dir)
        {
            XYZ normal = null;
            if (dst.IsCross() || (dst.GetSlope().IsLessThanEqualTo(1)))//被绕过的弯是横管或者角度小于45度的管
            {
                XYZ dstV = dst.GetLine().Direction;
                if (mep.IsStand())//立管做特殊处理
                {
                    dstV = dstV - new XYZ(0, 0, dstV.Z);
                    var angle = dstV.AngleTo(XYZ.BasisX);
                    angle = angle > Math.PI / 2 ? (Math.PI - angle) : angle;
                    dstV = angle < (Math.PI / 4) ? new XYZ(0, -1, 0) : new XYZ(1, 0, 0);
                }
                else
                {
                    dstV = new XYZ(0, 0, -1);
                }

                normal = connector1.CoordinateSystem.BasisZ.GetNormal(dir == TurnDirectionModeENUM.Up ? dstV : -dstV);

            }
            else//被绕过的弯是立管或者角度大于45度的管
            {
                //var normal = connector1.CoordinateSystem.BasisZ.GetNormal(data.TurnDirection == MEPCurveTurnData.TurnDirectionENUM.Up ? new XYZ(0, -1, 0) : new XYZ(0, 1, 0));
                XYZ dstV = null;
                if (mep.IsStand())//立管做特殊处理
                {
                    dstV = dst.GetLine().Direction;
                    dstV = dstV - new XYZ(0, 0, dstV.Z);
                    var angle = dstV.AngleTo(XYZ.BasisX);
                    angle = angle > Math.PI / 2 ? (Math.PI - angle) : angle;
                    dstV = angle < (Math.PI / 4) ? new XYZ(0, -1, 0) : new XYZ(1, 0, 0);
                }
                else
                {
                    dstV = mep.GetLine().Direction;
                    dstV = dstV - new XYZ(0, 0, dstV.Z);
                    var angle = dstV.AngleTo(XYZ.BasisX);
                    angle = angle > Math.PI / 2 ? (Math.PI - angle) : angle;
                    dstV = angle < (Math.PI / 4) ? new XYZ(0, -1, 0) : new XYZ(1, 0, 0);
                }

                normal = connector1.CoordinateSystem.BasisZ.GetNormal(dir == TurnDirectionModeENUM.Up ? dstV : -dstV);
            }

            return normal;
        }

        /// <summary>
        /// 获取某点所在管线上的面
        /// </summary>
        /// <param name="mep"></param>
        /// <param name="xyz"></param>
        /// <returns></returns>
        private Face GetInSideFace(MEPCurve mep, XYZ xyz)
        {
            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            var geometry = mep.get_Geometry(options);
            foreach (var geoElement in geometry)
            {
                Solid solid = geoElement as Solid;
                if (solid != null)
                {
                    foreach (Face face in solid.Faces)
                    {
                        var result = face.Project(xyz);
                        if (result.XYZPoint.IsAlmostEqualTo(xyz))
                        {
                            return face;
                        }
                    }
                }
            }

            return null;
        }

        #region 绕弯中间数据
        /// <summary>
        /// 需要绕弯的管线的数据类
        /// </summary>
        class ToTurnEnt
        {
            public MEPCurve BaseMep { get; set; }
            public List<MEPCurve> TurnMeps { get; set; }

            public List<MEPCurve> NowTurnMeps
            {
                get
                {
                    ElementIntersectsElementFilter filter = new ElementIntersectsElementFilter(this.BaseMep);
                    return this.TurnMeps.Where(p => filter.PassesFilter(p)).ToList();
                }
            }

            private double maxDistance = 0;
            /// <summary>
            /// 莫名的距离
            /// </summary>
            public double MaxDistance
            {
                get
                {
                    if (this.maxDistance == 0)
                    {
                        List<XYZ> xyzs = new List<XYZ>();
                        foreach (var mep in TurnMeps)
                            xyzs.Add(this.BaseMep.GetLine().GetClosestPoint(mep.GetLine()));

                        var xyz = this.BaseMep.GetLine().GetEndPoint(0);
                        xyzs.Sort((p1, p2) => { return xyz.DistanceTo(p1).CompareTo(xyz.DistanceTo(p2)); });
                        if (xyzs.Count >= 2)
                            this.maxDistance = xyzs.First().DistanceTo(xyzs.Last());
                        else
                            this.maxDistance = 0;
                    }
                    return this.maxDistance;
                }
            }
        }

        /// <summary>
        /// 计算绕弯位置所用的中间数据类
        /// </summary>
        class SingleTurnParamEnt
        {
            public enum ResultTypeENUM { Error, Success, EndTurn }

            /// <summary>
            /// 静止管道
            /// </summary>
            public MEPCurve StaticMep { get; set; }

            /// <summary>
            /// 绕弯管道
            /// </summary>
            public MEPCurve MoveMep { get; set; }

            /// <summary>
            /// 管件的X向距离
            /// </summary>
            public double WidthX { get; set; }

            /// <summary>
            /// 管件的Y向距离
            /// </summary>
            public double WidthY { get; set; }

            /// <summary>
            /// 高低差
            /// </summary>
            public double HeightDis { get; set; }

            /// <summary>
            /// 水平距离
            /// </summary>
            public double HDis { get; set; }

            /// <summary>
            /// 竖直距离
            /// </summary>
            public double VDis { get; set; }

            /// <summary>
            /// 臂长，从中心点到connter的长度
            /// </summary>
            public double Length { get; set; }

            /// <summary>
            /// 绕弯角度
            /// </summary>
            public double Angle { get; set; }

            /// <summary>
            /// 绕弯方向
            /// </summary>
            public TurnDirectionModeENUM TurnDirectionMode { get; set; }

            /// <summary>
            /// 绕弯点1
            /// </summary>
            public XYZ XYZ1 { get; set; }

            /// <summary>
            /// 绕晚点2
            /// </summary>
            public XYZ XYZ2 { get; set; }

            /// <summary>
            /// 端点绕弯数据
            /// </summary>
            public List<EndTurnEnt> EndTurnEnts { get; set; }

            /// <summary>
            /// 初始化成功与否的标志
            /// </summary>
            public ResultTypeENUM ResultType { get; set; }

            public Exception ErrorEX { get; set; }
        }

        /// <summary>
        /// 端点绕弯所用的数据类
        /// </summary>
        public class EndTurnEnt
        {
            public MEPCurve NextMoveMep { get; set; }
            public Connector Con { get; set; }
            public XYZ XYZ { get; set; }

            public MEPCurve CopyMepCross { get; set; }
            public MEPCurve CopyMepStand { get; set; }

            public Connector KeyCon { get; set; }
        }

        /// <summary>
        /// 计算绕弯位置所用的中间数据类
        /// </summary>
        class MultiTurnParamEnt
        {
            /// <summary>
            /// 静止管道
            /// </summary>
            public List<MEPCurve> StaticMeps { get; set; }

            /// <summary>
            /// 绕弯管道
            /// </summary>
            public MEPCurve MoveMep { get; set; }

            /// <summary>
            /// 水平距离
            /// </summary>
            public double HDis { get; set; }

            /// <summary>
            /// 竖直距离
            /// </summary>
            public double VDis { get; set; }

            /// <summary>
            /// 臂长，从中心点到connter的长度
            /// </summary>
            public double Length { get; set; }

            /// <summary>
            /// 绕弯角度
            /// </summary>
            public double Angle { get; set; }

            /// <summary>
            /// 绕弯方向
            /// </summary>
            public TurnDirectionModeENUM TurnDirectionMode { get; set; }

            /// <summary>
            /// 绕弯点1
            /// </summary>
            public XYZ XYZ1 { get; set; }

            /// <summary>
            /// 绕晚点2
            /// </summary>
            public XYZ XYZ2 { get; set; }

            /// <summary>
            /// 初始化成功与否的标志
            /// </summary>
            public bool IsInited { get; set; }

            public Exception ErrorEX { get; set; }
        }

        class FailuresPreprocessor : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                var failures = failuresAccessor.GetFailureMessages();
                if (failures.Count > 0)
                    return FailureProcessingResult.ProceedWithRollBack;
                else
                    return FailureProcessingResult.Continue;
            }
        }
        #endregion
    }

    #region 多管避让的类
    /// <summary>
    /// 压力水管的多管避让
    /// </summary>
    class MultiMEPCurveTurn_YaLi : MultiMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncSingle
        {
            get
            {
                return this.IsMEPCurve_YaLiPipe;
            }
        }

        protected override Func<List<MEPCurve>, bool> CompareFuncMulti
        {
            get
            {
                return (List<MEPCurve> list) => { return true; };
            }
        }
    }

    /// <summary>
    /// 无压力水管的多管避让
    /// </summary>
    class MultiMEPCurveTurn_WuYaLi : MultiMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncSingle
        {
            get
            {
                return this.IsMEPCurve_WuYaLiPipe;
            }
        }

        protected override Func<List<MEPCurve>, bool> CompareFuncMulti
        {
            get
            {
                return (List<MEPCurve> list) => { return true; };
            }
        }

        public override MultiTurnResultENUM IsKeepingWith(MEPCurve mep, List<MEPCurve> meps)
        {
            if (this.CompareFuncSingle(mep) && this.CompareFuncMulti(meps))
            {
                var name = mep.GetMEPCurveType().Name;

                if (name.Contains("废水") || name.Contains("污水"))
                    return MultiTurnResultENUM.MultiTurn;
                else if (name.Contains("通气") || name.Contains("雨水"))
                    return MultiTurnResultENUM.SingleTurn;
                else
                    return MultiTurnResultENUM.SingleTurn;
            }
            else
                return MultiTurnResultENUM.Null;
        }
    }

    /// <summary>
    /// 暖通风管的多管避让
    /// </summary>
    class MultiMEPCurveTurn_NTDuct : MultiMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncSingle
        {
            get
            {
                return this.IsMEPCurve_NTDuct;
            }
        }

        protected override Func<List<MEPCurve>, bool> CompareFuncMulti
        {
            get
            {
                return (List<MEPCurve> list) => { return true; };
            }
        }

        public override MultiTurnResultENUM IsKeepingWith(MEPCurve mep, List<MEPCurve> meps)
        {
            if (this.CompareFuncSingle(mep) && this.CompareFuncMulti(meps))
            {
                return MultiTurnResultENUM.MultiTurn;
            }
            else
                return MultiTurnResultENUM.Null;
        }
    }

    /// <summary>
    /// 暖通水管的多管避让
    /// </summary>
    class MultiMEPCurveTurn_NTPipe : MultiMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncSingle
        {
            get
            {
                return this.IsMEPCurve_NTPipe;
            }
        }

        protected override Func<List<MEPCurve>, bool> CompareFuncMulti
        {
            get
            {
                return (List<MEPCurve> list) => { return true; };
            }
        }

        public override MultiTurnResultENUM IsKeepingWith(MEPCurve mep, List<MEPCurve> meps)
        {
            if (this.CompareFuncSingle(mep) && this.CompareFuncMulti(meps))
            {
                return MultiTurnResultENUM.SingleTurn;
            }
            else
                return MultiTurnResultENUM.Null;
        }
    }

    /// <summary>
    /// 电气桥架的多管避让
    /// </summary>
    class MultiMEPCurveTurn_DQ : MultiMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncSingle
        {
            get
            {
                return this.IsMEPCurve_DQ;
            }
        }

        protected override Func<List<MEPCurve>, bool> CompareFuncMulti
        {
            get
            {
                return (List<MEPCurve> list) => { return true; };
            }
        }

        public override MultiTurnResultENUM IsKeepingWith(MEPCurve mep, List<MEPCurve> meps)
        {
            if (this.CompareFuncSingle(mep) && this.CompareFuncMulti(meps))
            {
                return MultiTurnResultENUM.SingleTurn;
            }
            else
                return MultiTurnResultENUM.Null;
        }
    }

    /// <summary>
    /// 多管避让的基础类
    /// </summary>
    abstract class MultiMEPCurveTurnBase : MEPCurveTurnBase
    {
        public enum MultiTurnResultENUM { Null, SingleTurn, MultiTurn }

        protected abstract Func<MEPCurve, bool> CompareFuncSingle { get; }
        protected abstract Func<List<MEPCurve>, bool> CompareFuncMulti { get; }

        public virtual MultiTurnResultENUM IsKeepingWith(MEPCurve mep, List<MEPCurve> meps)
        {
            if (this.CompareFuncSingle(mep) && this.CompareFuncMulti(meps))
                return MultiTurnResultENUM.SingleTurn;
            else
                return MultiTurnResultENUM.Null;
        }
    }
    #endregion

    #region 单管避让的类
    /// <summary>
    /// 类型相同，小管避让大管
    /// </summary>
    class MEPCurveTurn_Same_Same : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsKeepingWith(MEPCurve mep1, MEPCurve mep2)
        {
            if ((this.IsMEPCurve_ColdJPS(mep1) && this.IsMEPCurve_ColdJPS(mep2)) ||
                (this.IsMEPCurve_DQ(mep1) && this.IsMEPCurve_DQ(mep2)) ||
                (this.IsMEPCurve_HotJPS(mep1) && this.IsMEPCurve_HotJPS(mep2)) ||
                (this.IsMEPCurve_NTDuct(mep1) && this.IsMEPCurve_NTDuct(mep2)) ||
                (this.IsMEPCurve_NTPipe(mep1) && this.IsMEPCurve_NTPipe(mep2)) ||
                (this.IsMEPCurve_WuYaLiPipe(mep1) && this.IsMEPCurve_WuYaLiPipe(mep2)) ||
                (this.IsMEPCurve_YaLiPipe(mep1) && this.IsMEPCurve_YaLiPipe(mep2)))
            {
                var compareValue = this.CompareSize(mep1, mep2);
                if (compareValue <= 0)
                {
                    this.MoveMEP = mep1;
                    this.StaticMEP = mep2;
                }
                else if (compareValue > 0)
                {
                    this.MoveMEP = mep2;
                    this.StaticMEP = mep1;
                }
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 压力管道避让无压力管道
    /// </summary>
    class MEPCurveTurn_YaLi_WuYaLi : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_YaLiPipe;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_WuYaLiPipe;
            }
        }
    }

    /// <summary>
    /// 冷水管道避让热水管道
    /// </summary>
    class MEPCurveTurn_Cold_Hot : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_ColdJPS;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_HotJPS;
            }
        }
    }

    /// <summary>
    /// 压力管道避让风管
    /// </summary>
    class MEPCurveTurn_YaLiPipe_Duct : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_YaLiPipe;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_NTDuct;
            }
        }
    }

    /// <summary>
    /// 压力管道避让暖通水管
    /// </summary>
    class MEPCurveTurn_YaLiPipe_NTPipe : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_YaLiPipe;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_NTPipe;
            }
        }

        public override bool IsKeepingWith(MEPCurve mep1, MEPCurve mep2)
        {
            if ((this.IsMEPCurve_YaLiPipe(mep1) && !this.IsMEPCurve_NTPipe(mep1) && this.IsMEPCurve_NTPipe(mep2)) ||
                (this.IsMEPCurve_YaLiPipe(mep2) && !this.IsMEPCurve_NTPipe(mep2) && this.IsMEPCurve_NTPipe(mep1)))
            {
                var compareValue = this.CompareSize(mep1, mep2);
                if (compareValue < 0)
                {
                    this.MoveMEP = mep1;
                    this.StaticMEP = mep2;
                }
                else if (compareValue > 0)
                {
                    this.MoveMEP = mep2;
                    this.StaticMEP = mep1;
                }
                else if (compareValue == 0)
                {
                    if (this.IsMEPCurve_NTPipe(mep1))
                    {
                        this.MoveMEP = mep2;
                        this.StaticMEP = mep1;
                    }
                    else
                    {
                        this.MoveMEP = mep1;
                        this.StaticMEP = mep2;
                    }
                }
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 压力管道避让电气
    /// </summary>
    class MEPCurveTurn_YaLiPipe_DQ : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_YaLiPipe;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_DQ;
            }
        }

        protected override void SetMEPCurves(MEPCurve mep1, MEPCurve mep2)
        {
            var width1 = mep1.GetWidth();
            var width2 = mep1.GetWidth();

            if (width1.IsLessThan(UnitTransUtils.MMToFeet(80)) && width2.IsGreaterThan(UnitTransUtils.MMToFeet(200)))
            {
                this.MoveMEP = mep1;
                this.StaticMEP = mep2;
            }
            else
            {
                this.MoveMEP = mep2;
                this.StaticMEP = mep1;
            }
        }
    }

    /// <summary>
    /// 暖通风管避让无压力管道
    /// </summary>
    class MEPCurveTurn_Duct_WuYaLiPipe : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_NTDuct;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_WuYaLiPipe;
            }
        }
    }

    /// <summary>
    /// 暖通水管避让无压力管道
    /// </summary>
    class MEPCurveTurn_NTPipe_WuYaLiPipe : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_NTPipe;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_WuYaLiPipe;
            }
        }
    }

    /// <summary>
    /// 电气避让无压力管道
    /// </summary>
    class MEPCurveTurn_DQ_WuYaLiPipe : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_DQ;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_WuYaLiPipe;
            }
        }
    }

    /// <summary>
    /// 暖通水管避让暖通风管
    /// </summary>
    class MEPCurveTurn_NTPipe_NTDuct : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_NTPipe;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_NTDuct;
            }
        }
    }

    /// <summary>
    /// 电气避让暖通风管
    /// </summary>
    class MEPCurveTurn_DQ_NTDuct : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_DQ;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_NTDuct;
            }
        }
    }

    /// <summary>
    /// 电气避让暖通水管
    /// 
    /// </summary>
    class MEPCurveTurn_DQ_NTPipe : SingleMEPCurveTurnBase
    {
        protected override Func<MEPCurve, bool> CompareFuncLeft
        {
            get
            {
                return this.IsMEPCurve_DQ;
            }
        }

        protected override Func<MEPCurve, bool> CompareFuncRight
        {
            get
            {
                return this.IsMEPCurve_NTPipe;
            }
        }
    }

    abstract class SingleMEPCurveTurnBase : MEPCurveTurnBase
    {
        public MEPCurve StaticMEP { get; protected set; }
        public MEPCurve MoveMEP { get; protected set; }

        protected abstract Func<MEPCurve, bool> CompareFuncLeft { get; }
        protected abstract Func<MEPCurve, bool> CompareFuncRight { get; }

        public virtual bool IsKeepingWith(MEPCurve mep1, MEPCurve mep2)
        {
            if (this.Compare(this.CompareFuncLeft, this.CompareFuncRight, ref mep1, ref mep2))
            {
                this.SetMEPCurves(mep1, mep2);
                return true;
            }
            else
                return false;
        }
        protected virtual void SetMEPCurves(MEPCurve mep1, MEPCurve mep2)
        {
            this.MoveMEP = mep1;
            this.StaticMEP = mep2;
        }

        protected int CompareSize(MEPCurve mep1, MEPCurve mep2)
        {
            return mep1.GetWidth().CompareTo(mep2.GetWidth());

            if (mep1 is Pipe || mep2 is Pipe)
            {
                return mep1.GetWidth().CompareTo(mep2.GetWidth());
            }

            throw new Exception("需补充CompareSize方法");
        }

        protected bool Compare(Func<MEPCurve, bool> func1, Func<MEPCurve, bool> func2, ref MEPCurve mep1, ref MEPCurve mep2)
        {
            if (func1(mep1) && func2(mep2))
                return true;
            else if (func1(mep2) && func2(mep1))
            {
                var temp = mep1;
                mep1 = mep2;
                mep2 = temp;
                return true;
            }
            else
                return false;
        }
    }
    #endregion

    #region 避让的基础类
    abstract class MEPCurveTurnBase
    {
        public static readonly string[] PipingSystem_YaLi = new string[] { "循环供水", "循环回水", "干式消防系统", "湿式消防系统", "预作用消防系统", "其他消防系统", "家用冷水", "家用热水" };
        public static readonly string[] PipingSystem_WuYaLi = new string[] { "卫生设备", "通气管", "其他" };
        public static readonly string[] NTSystem = new string[] { "循环供水", "循环回水" };

        /// <summary>
        /// 是否为压力管
        /// </summary>
        /// <param name="mep"></param>
        /// <returns></returns>
        protected bool IsMEPCurve_YaLiPipe(MEPCurve mep)
        {
            var pipe = mep as Pipe;
            if (pipe == null)
                return false;

            var p = pipe.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
            if (p == null)
                return false;

            var value = p.AsString();
            if (PipingSystem_YaLi.Contains(value))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否为无压管
        /// </summary>
        /// <param name="mep"></param>
        /// <returns></returns>
        protected bool IsMEPCurve_WuYaLiPipe(MEPCurve mep)
        {
            var pipe = mep as Pipe;
            if (pipe == null)
                return false;

            var p = pipe.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
            if (p == null)
                return false;

            var value = p.AsString();
            if (PipingSystem_WuYaLi.Contains(value))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否为暖通风管
        /// </summary>
        /// <param name="mep"></param>
        /// <returns></returns>
        protected bool IsMEPCurve_NTDuct(MEPCurve mep)
        {
            if (mep is Duct)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否为暖通水管
        /// </summary>
        /// <param name="mep"></param>
        /// <returns></returns>
        protected bool IsMEPCurve_NTPipe(MEPCurve mep)
        {
            var pipe = mep as Pipe;
            if (pipe == null)
                return false;

            var p = pipe.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
            if (p == null)
                return false;

            var value = p.AsString();
            if (NTSystem.Contains(value))
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否为电气管线
        /// </summary>
        /// <param name="mep"></param>
        /// <returns></returns>
        protected bool IsMEPCurve_DQ(MEPCurve mep)
        {
            if (mep is CableTray || mep is Conduit)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否为冷水管
        /// </summary>
        /// <param name="mep"></param>
        /// <returns></returns>
        protected bool IsMEPCurve_ColdJPS(MEPCurve mep)
        {
            var pipe = mep as Pipe;
            if (pipe == null)
                return false;

            var p = pipe.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
            if (p == null)
                return false;

            var value = p.AsString();
            if (value == "家用冷水")
                return true;
            else
                return false;
        }

        /// <summary>
        /// 是否为热水管
        /// </summary>
        /// <param name="mep"></param>
        /// <returns></returns>
        protected bool IsMEPCurve_HotJPS(MEPCurve mep)
        {
            var pipe = mep as Pipe;
            if (pipe == null)
                return false;

            var p = pipe.get_Parameter(BuiltInParameter.RBS_SYSTEM_CLASSIFICATION_PARAM);
            if (p == null)
                return false;

            var value = p.AsString();
            if (value == "家用热水")
                return true;
            else
                return false;
        }
    }
    #endregion
}
