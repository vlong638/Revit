using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using MyRevit.MyTests.CompoundStructureAnnotation;
using MyRevit.MyTests.PipeAnnotationTest;
using MyRevit.MyTests.Utilities;
using MyRevit.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Interop;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    /// <summary>
    /// CompoundStructureAnnotation ViewType
    /// </summary>
    public enum CSAViewType
    {
        Idle,
        Select,
        Generate,
        Close,
    }

    /// <summary>
    /// CompoundStructureAnnotation 文字的定位方案
    /// </summary>
    public enum CSALocationType
    {
        /// <summary>
        /// 在线端
        /// </summary>
        OnEdge,
        /// <summary>
        /// 在线上
        /// </summary>
        OnLine,
    }

    public class TextLocation
    {
        public TextLocation(XYZ start, XYZ end)
        {
            Start = start;
            End = end;
        }

        public XYZ Start { set; get; }
        public XYZ End { set; get; }
    }

    /// <summary>
    /// CompoundStructureAnnotation数据的载体
    /// </summary>
    public class CSAModel: ModelBase<CSAModel>
    {
        public CSAModel(string data = "") : base(data)
        {
            if (Texts == null)
                Texts = new List<string>();
            if (TextLocations == null)
                TextLocations = new List<XYZ>();
            if (Texts == null)
                Texts = new List<string>();
        }

        #region 需要留存的数据
        /// <summary>
        /// 标注元素 对象,可以是墙,屋顶,伸展屋顶等等
        /// </summary>
        public ElementId TargetId { set; get; }
        /// <summary>
        /// 线 对象
        /// </summary>
        public List<ElementId> LineIds { get; set; }
        /// <summary>
        /// 文字 对象
        /// </summary>
        public List<ElementId> TextNoteIds { set; get; }
        /// <summary>
        /// 文字 定位方案
        /// </summary>
        public CSALocationType CSALocationType { set; get; }
        /// <summary>
        /// 文本 位置
        /// </summary>
        public List<XYZ> TextLocations { set; get; }

        public override void FromData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return;
            var values = data.Split(PropertySplitter_Char);
            if (values.Count() != 6)
                throw new NotImplementedException("数据格式错误");

            TargetId = new ElementId(Convert.ToInt32(values[0]));
            LineIds = new List<ElementId>();
            foreach (var item in values[1].Split(PropertyInnerSplitter_Char))
                LineIds.Add(new ElementId(Convert.ToInt32(item)));
            //TextNoteIds
            TextNoteIds = new List<ElementId>();
            foreach (var item in values[2].Split(PropertyInnerSplitter_Char))
                TextNoteIds.Add(new ElementId(Convert.ToInt32(item)));
            CSALocationType = (CSALocationType)Enum.Parse(typeof(CSALocationType), values[3]);
            //TextLocations
            TextLocations = new List<XYZ>();
            foreach (var item in values[4].Split(PropertyInnerSplitter_Char))
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                var innerItem = item.Split(PropertyInnerSplitter2_Char);
                TextLocations.Add(new XYZ(Convert.ToDouble(innerItem[0]), Convert.ToDouble(innerItem[1]), Convert.ToDouble(innerItem[2])));
            }
            //Texts
            Texts = new List<string>();
            foreach (var item in values[5].Split(PropertyInnerSplitter_Char))
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                Texts.Add(item);
            }
        }

        public override string ToData()
        {
            return TargetId.IntegerValue
            + PropertySplitter + string.Join(PropertyInnerSplitter, LineIds.Select(c => c.IntegerValue))
            + PropertySplitter + string.Join(PropertyInnerSplitter, TextNoteIds.Select(c => c.IntegerValue))
            + PropertySplitter + (int)CSALocationType
            + PropertySplitter + string.Join(PropertyInnerSplitter, TextLocations.Select(c => c.X + PropertyInnerSplitter2 + c.Y + PropertyInnerSplitter2 + c.Z))
            + PropertySplitter + string.Join(PropertyInnerSplitter, Texts);
        }
        #endregion

        #region 无需留存的数据
        /// <summary>
        /// 文字样式
        /// </summary>
        public ElementId TextNoteTypeElementId
        {
            get
            {
                return textNoteTypeElementId;
            }
            set
            {
                textNoteTypeElementId = value;
                var textNoteType = VLConstraints.Doc.GetElement(textNoteTypeElementId) as TextNoteType;
                VLConstraints.CurrentFontSizeScale = UnitHelper.ConvertFromFootTo(textNoteType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble(), VLUnitType.millimeter);//文本大小
                VLConstraints.CurrentFontWidthScale = textNoteType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE).AsDouble();//文本宽度系数
            }
        }
        private ElementId textNoteTypeElementId = null;
        /// <summary>
        /// 需要显示的结构标注信息
        /// </summary>
        public List<string> Texts { set; get; }
        /// <summary>
        /// 坐标定位,平行于标注对象
        /// </summary>
        public XYZ ParallelVector = null;
        /// <summary>
        /// 坐标定位,垂直于标注对象
        /// </summary>
        public XYZ VerticalVector = null;
        /// <summary>
        /// 线起点
        /// </summary>
        public XYZ LineStartLocation { set; get; }
        /// <summary>
        /// 线终点
        /// </summary>
        public XYZ LineEndLocation { set; get; }
        /// <summary>
        /// 横线
        /// </summary>
        public List<TextLocation> ParallelLinesLocations { set; get; }
        #endregion

        #region 方法
        public void UpdateVectors( LocationCurve locationCurve)
        {
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = (locationCurve.Curve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            double xyzTolarance = 0.01;
            if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
            {
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            }
            VerticalVector = verticalVector;
            ParallelVector = parallelVector;
        }

        public void CalculateLocations(Element element, XYZ offset)
        {
            var scale = 1 / VLConstraints.OrientFontSizeScale * VLConstraints.CurrentFontSizeScale;
            var width = CSALocationType.GetLineWidth() * scale;
            var height = 400 * scale;
            var widthFoot = UnitHelper.ConvertToFoot(width, VLUnitType.millimeter);
            var heightFoot = UnitHelper.ConvertToFoot(height, VLUnitType.millimeter);
            var verticalFix = UnitHelper.ConvertToFoot(100, VLUnitType.millimeter) * scale;
            var locationCurve = element.Location as LocationCurve;
            UpdateVectors(locationCurve);
            //线起始点 (中点)
            LineStartLocation = (locationCurve.Curve.GetEndPoint(0) + locationCurve.Curve.GetEndPoint(1)) / 2;
            //文本位置 start:(附着元素中点+线基本高度+文本高度*(文本个数-1))  end: start+宽
            //高度,宽度 取决于文本 
            ParallelLinesLocations = new List<TextLocation>();
            TextLocations = new List<XYZ>();
            for (int i = 0; i < Texts.Count(); i++)
            {
                var start = LineStartLocation + (heightFoot + i * VLConstraints.CurrentFontHeight) * VerticalVector;
                var end = start + widthFoot * ParallelVector;
                ParallelLinesLocations.Add(new TextLocation(start, end));
                TextLocations.Add(CSALocationType.GetTextLocation(verticalFix, VerticalVector, start, end));
            }
            //线终点 (最高的文本位置)
            LineEndLocation = ParallelLinesLocations[ParallelLinesLocations.Count - 1].Start;
        }

        public void CalculateLocations(Element element)
        {
            CalculateLocations(element, new XYZ(0, 0, 0));
        }
        #endregion
    }

    static class CSALocationTypeHelper
    {
        public static double GetLineWidth(this CSALocationType CSALocationType)
        {
            return CSALocationType == CSALocationType.OnEdge ? 400 : 3000;
        }

        public static XYZ GetTextLocation(this CSALocationType CSALocationType, double verticalFix, XYZ verticalVector, XYZ start, XYZ end)
        {
            return CSALocationType == CSALocationType.OnEdge ? end + (VLConstraints.CurrentFontHeight / 2 + verticalFix) * verticalVector : start + (VLConstraints.CurrentFontHeight + verticalFix) * verticalVector;
        }
    }

    /// <summary>
    /// 为CSAModel数据服务的处理类
    /// </summary>
    public static class CSAModelEx
    {
        /// <summary>
        /// 获取文字的样式方案
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<TextNoteType> GetTextNoteTypes(this CSAModel model)
        {
            var textNoteTypes = new FilteredElementCollector(VLConstraints.Doc).OfClass(typeof(TextNoteType)).ToElements().Select(p => p as TextNoteType).ToList();
            return textNoteTypes;
        }

        /// <summary>
        /// 线 参数设置
        /// </summary>
        public static void UpdateLineParameters(this CSAModel model, FamilyInstance line, double lineHeight, double lineWidth, double space, int textCount)
        {
            line.GetParameters(TagProperty.线高度1.ToString()).First().Set(UnitHelper.ConvertToFoot(lineHeight, VLConstraints.CurrentUnitType));
            line.GetParameters(TagProperty.线宽度.ToString()).First().Set(UnitHelper.ConvertToFoot(lineWidth, VLConstraints.CurrentUnitType));
            line.GetParameters(TagProperty.线下探长度.ToString()).First().Set(0);
            line.GetParameters(TagProperty.间距.ToString()).First().Set(UnitHelper.ConvertToFoot(space, VLConstraints.CurrentUnitType));
            line.GetParameters(TagProperty.文字行数.ToString()).First().Set(textCount);
        }

        /// <summary>
        /// 获取 CompoundStructure 对象
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static CompoundStructure GetCompoundStructure(this CSAModel model, Element element)
        {
            CompoundStructure compoundStructure = null;
            if (element is Wall)
            {
                compoundStructure = (element as Wall).WallType.GetCompoundStructure();
            }
            else if (element is Floor)
            {
                compoundStructure = (element as Floor).FloorType.GetCompoundStructure();
            }
            else if (element is ExtrusionRoof)//TODO 屋顶有多种类型
            {
                compoundStructure = (element as ExtrusionRoof).RoofType.GetCompoundStructure();
            }
            else
            {
                throw new NotImplementedException("暂不支持该类型的对象,未能成功获取CompoundStructure");
            }
            return compoundStructure;
        }

        /// <summary>
        /// 从 CompoundStructure 中获取标注信息
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="compoundStructure"></param>
        /// <returns></returns>
        public static List<string> FetchTextsFromCompoundStructure(this CSAModel model, Document doc, CompoundStructure compoundStructure)
        {
            var layers = compoundStructure.GetLayers();
            var texts = new List<string>();
            foreach (var layer in layers)
            {
                if (layer.MaterialId.IntegerValue < 0)
                    continue;
                var material = doc.GetElement(layer.MaterialId);
                if (material == null)
                    continue;
                texts.Add(layer.Width + doc.GetElement(layer.MaterialId).Name);
            }
            model.Texts = texts;
            return texts;
        }

        /// <summary>
        /// 从需要标注的对象中获取标注的源位置
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static XYZ GetElementLocation(this CSAModel model, Element element)
        {
            var locationCurve = element.Location as LocationCurve;
            var location = (locationCurve.Curve.GetEndPoint(0) + locationCurve.Curve.GetEndPoint(1)) / 2;
            return location;
        }

        private static void SetCurveFromLine(CSAModel model, LocationCurve locationCurve)
        {
            XYZ parallelVector = null;
            XYZ verticalVector = null;
            parallelVector = (locationCurve.Curve as Line).Direction;
            verticalVector = new XYZ(parallelVector.Y, -parallelVector.X, 0);
            parallelVector = LocationHelper.GetVectorByQuadrant(parallelVector, QuadrantType.OneAndFour);
            verticalVector = LocationHelper.GetVectorByQuadrant(verticalVector, QuadrantType.OneAndTwo);
            double xyzTolarance = 0.01;
            if (Math.Abs(verticalVector.X) > 1 - xyzTolarance)
            {
                verticalVector = new XYZ(-verticalVector.X, -verticalVector.Y, verticalVector.Z);
            }
            model.VerticalVector = verticalVector;
            model.ParallelVector = parallelVector;
        }

        /// <summary>
        /// 按XY的点的Z轴旋转
        /// </summary>
        /// <param name="point"></param>
        /// <param name="xyz"></param>
        /// <param name="verticalVector"></param>
        public static void RotateByXY(this LocationPoint point, XYZ xyz, XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            if (verticalVector.X > VLConstraints.MiniValueForXYZ)
                point.Rotate(axis, 2 * Math.PI - verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
            else
                point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }

        /// <summary>
        /// 按XY的点的Z轴旋转
        /// </summary>
        /// <param name="point"></param>
        /// <param name="xyz"></param>
        /// <param name="verticalVector"></param>
        public static void RotateByXY(this Location point, XYZ xyz, XYZ verticalVector)
        {
            Line axis = Line.CreateBound(xyz, xyz.Add(new XYZ(0, 0, 10)));
            if (verticalVector.X > VLConstraints.MiniValueForXYZ)
                point.Rotate(axis, 2 * Math.PI - verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
            else
                point.Rotate(axis, verticalVector.AngleTo(new XYZ(0, 1, verticalVector.Z)));
        }


        ///// <summary>
        ///// 获取OnLineEdge方案的标注点位
        ///// </summary>
        ///// <param name="model">模型数据</param>
        ///// <param name="height">线高度</param>
        ///// <param name="horizontalFix">对应策略的水平修正</param>
        ///// <param name="verizontalFix">对应策略的垂直修正</param>
        ///// <param name="i">第几个标注文字</param>
        ///// <param name="actualLength">文本长度</param>
        ///// <returns></returns>
        //private static XYZ GetTagHeadPositionWithParam(CSAModel model, double height, double horizontalFix, double verizontalFix, int i, double actualLength)
        //{
        //    XYZ parallelVector = model.ParallelVector;
        //    XYZ verticalVector = model.VerticalVector;
        //    XYZ startPoint = model.LineLocation;
        //    var result = startPoint + (UnitHelper.ConvertToFoot(height - i * VLConstraints.OrientFontHeight, VLConstraints.CurrentUnitType)) * verticalVector
        //    + horizontalFix * parallelVector + verizontalFix * verticalVector
        //    + actualLength / 25.4 * parallelVector;
        //    return result;
        //}
    }


    public class ViewModelBase : DependencyObject, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        /// <summary>
        /// 实现INPC接口 监控属性改变
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    /// <summary>
    /// CompoundStructureAnnotation ViewModel
    /// </summary>
    public class CSAViewModel : ViewModelBase
    {
        public CSAViewModel()
        {
            ViewType = CSAViewType.Idle;
            Model = new CSAModel();
            LocationType = CSALocationType.OnEdge;
        }

        public CSAViewType ViewType { set; get; }
        public CSAModel Model { set; get; }

        #region 绑定用属性 需在ViewModel中初始化
        CSALocationType LocationType
        {
            get
            {
                return Model.CSALocationType;
            }
            set
            {
                Model.CSALocationType = value;
                RaisePropertyChanged("IsDocument");
                RaisePropertyChanged("IsLinkDocument");
            }
        }
        public bool IsOnLine
        {
            get { return LocationType == CSALocationType.OnLine; }
            set { if (value) LocationType = CSALocationType.OnLine; }
        }
        public bool IsOnEdge
        {
            get { return LocationType == CSALocationType.OnEdge; }
            set { if (value) LocationType = CSALocationType.OnEdge; }
        }

        public List<TextNoteType> TextNoteTypes { get { return Model.GetTextNoteTypes(); } }
        public ElementId TextNoteTypeElementId
        {
            get
            {
                if (Model.TextNoteTypeElementId == null)
                {
                    Model.TextNoteTypeElementId = TextNoteTypes.FirstOrDefault().Id;
                }
                return Model.TextNoteTypeElementId;
            }
            set
            {
                Model.TextNoteTypeElementId = value;
                this.RaisePropertyChanged("Type");
            }
        }
        #endregion

        public void Execute(CompoundStructureAnnotationWindow window, PmSoft.Optimization.DrawingProduction.CompoundStructureAnnotationSet set, UIDocument uiDoc)
        {
            switch (ViewType)
            {
                case CSAViewType.Idle:
                    window = new CompoundStructureAnnotationWindow(set);
                    IntPtr rvtPtr = Autodesk.Windows.ComponentManager.ApplicationWindow;
                    WindowInteropHelper helper = new WindowInteropHelper(window);
                    helper.Owner = rvtPtr;
                    window.ShowDialog();
                    break;
                case CSAViewType.Select:
                    if (window.IsActive)
                        window.Close();
                    using (PmSoft.Common.RevitClass.PickObjectsMouseHook MouseHook = new PmSoft.Common.RevitClass.PickObjectsMouseHook())
                    {
                        MouseHook.InstallHook(PmSoft.Common.RevitClass.PickObjectsMouseHook.OKModeENUM.Objects);
                        try
                        {
                            Model.TargetId = uiDoc.Selection.PickObject(ObjectType.Element, new ClassFilter(typeof(Wall))).ElementId;
                            MouseHook.UninstallHook();
                            ViewType = CSAViewType.Generate;
                        }
                        catch (Exception ex)
                        {
                            MouseHook.UninstallHook();
                            ViewType = CSAViewType.Idle;
                        }
                    }
                    break;
                case CSAViewType.Generate:
                    var doc = uiDoc.Document;
                    if (TransactionHelper.DelegateTransaction(doc, "生成结构标注", (Func<bool>)(() =>
                    {
                        var element = doc.GetElement(Model.TargetId);
                        CompoundStructure compoundStructure = Model.GetCompoundStructure(element);//获取文本载体
                        if (compoundStructure == null)
                            return false;
                        var texts = Model.FetchTextsFromCompoundStructure(doc, compoundStructure);//获取文本数据
                        if (texts.Count == 0)
                            return false;

                        var Collection = CompoundStructureAnnotationContext.GetCollection(doc);
                        if (Collection.Data.FirstOrDefault(c => c.TargetId.IntegerValue == Model.TargetId.IntegerValue) != null)
                            return false;
                        CompoundStructureAnnotationContext.Creater.Generate(doc, Model, element);
                        Collection.Data.Add(Model);
                        Collection.Save(doc);
                        return true;
                    })))
                        ViewType = CSAViewType.Select;
                    else
                        ViewType = CSAViewType.Idle;
                    break;
                case CSAViewType.Close:
                    window.Close();
                    break;
                default:
                    break;
            }
        }
    }

    public class CSACreater
    {
        public void Generate(Document doc, CSAModel model, Element element)
        {
            Generate(doc, model, element, new XYZ(0, 0, 0));
        }

        private void Generate(Document doc, CSAModel model, Element element, XYZ offset)
        {
            //主体
            model.TargetId = element.Id;
            //线生成
            model.CalculateLocations(element, offset);//计算内容定位
            List<Line> lines = new List<Line>();
            lines.Add(Line.CreateBound(model.LineStartLocation, model.LineEndLocation));
            foreach (var parallelLinesLocation in model.ParallelLinesLocations)
                lines.Add(Line.CreateBound(parallelLinesLocation.Start, parallelLinesLocation.End));
            model.LineIds = new List<ElementId>();
            foreach (var line in lines)
            {
                var lineElement = doc.Create.NewModelCurve(line, VLConstraints.UIDoc.ActiveView.SketchPlane);
                model.LineIds.Add(lineElement.Id);
            }
            //文本生成
            List<TextNote> textNotes = new List<TextNote>();
            foreach (var text in model.Texts)//生成 文本
            {
                var textLocation = model.TextLocations[model.Texts.IndexOf(text)];
                var textNote = TextNote.Create(doc, doc.ActiveView.Id, textLocation, text, model.TextNoteTypeElementId);
                textNotes.Add(textNote);
                textNote.Location.RotateByXY(textLocation, model.VerticalVector);
            }
            model.TextNoteIds = textNotes.Select(c => c.Id).ToList();
            //测试用
            //GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\1023结构做法标注.png", lines, Model.TextLocations);
        }

        internal void Regenerate(Document doc, CSAModel model, Element target)
        {
            CompoundStructureAnnotationContext.Creater.Regenerate(doc, model, target, new XYZ(0,0,0));
        }

        internal void Regenerate(Document doc, CSAModel model, Element target, XYZ offset)
        {
            //不是选取的文本类型 以Text的文本类型为准
            if (model.TextNoteTypeElementId == null)
                model.TextNoteTypeElementId = (doc.GetElement(model.TextNoteIds[0]) as TextNote).TextNoteType.Id;
            //删除线
            foreach (var item in model.LineIds)
                if (doc.GetElement(item) != null)
                    doc.Delete(item);
            //删除标注
            foreach (var item in model.TextNoteIds)
                if (doc.GetElement(item) != null)
                    doc.Delete(item);
            Generate(doc, model, target, offset);
        }
    }
}
