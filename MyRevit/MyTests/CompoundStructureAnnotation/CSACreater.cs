using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    public class CSACreater
    {
        #region 详图线方案
        //public void Generate(Document doc, CSAModel model, Element element)
        //{
        //    Generate(doc, model, element, new XYZ(0, 0, 0));
        //}

        //private void Generate(Document doc, CSAModel model, Element element, XYZ offset)
        //{
        //    CompoundStructure compoundStructure = model.GetCompoundStructure(element);//获取文本载体
        //    if (compoundStructure == null)
        //        return;
        //    var texts = model.FetchTextsFromCompoundStructure(doc, compoundStructure);//获取文本数据
        //    if (texts.Count == 0)
        //        return;
        //    //主体
        //    model.TargetId = element.Id;

        //    #region old
        //    ////线生成
        //    //model.CalculateLocations(element, offset);//计算内容定位
        //    //List<Line> lines = new List<Line>();
        //    //lines.Add(Line.CreateBound(model.LineStartLocation, model.LineEndLocation));
        //    //foreach (var parallelLinesLocation in model.ParallelLinesLocations)
        //    //    lines.Add(Line.CreateBound(parallelLinesLocation.Start, parallelLinesLocation.End));
        //    //model.LineIds = new List<ElementId>();
        //    //foreach (var line in lines)
        //    //{
        //    //    var lineElement = doc.Create.NewModelCurve(line, VLConstraints.Doc.ActiveView.SketchPlane);
        //    //    model.LineIds.Add(lineElement.Id);
        //    //} 
        //    #endregion

        //    //族线
        //    var lineFamilySymbol = VLConstraints.GetMultipleTagSymbol(doc);//获取线标注类型
        //    var line = doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), lineFamilySymbol, doc.ActiveView);//生成 线
        //    model.CalculateLocations(element, line, offset);//计算内容定位
        //    var lineLocation = model.LineLocation;
        //    var textLocations = model.TextLocations;
        //    ElementTransformUtils.MoveElement(doc, line.Id, lineLocation);//线定位
        //    LocationPoint locationPoint = line.Location as LocationPoint;//线 旋转处理
        //    locationPoint.RotateByXY(lineLocation, model.VerticalVector);
        //    model.LineId = line.Id;
        //    model.UpdateLineParameters(line, 500, 200, AnnotationConstaints.TextHeight, model.Texts.Count());//线参数设置 
        //    //文本生成
        //    List<TextNote> textNotes = new List<TextNote>();
        //    foreach (var text in model.Texts)//生成 文本
        //    {
        //        var textLocation = model.TextLocations[model.Texts.IndexOf(text)];
        //        var textNote = TextNote.Create(doc, doc.ActiveView.Id, textLocation, text, model.TextNoteTypeElementId);
        //        textNotes.Add(textNote);
        //        textNote.Location.RotateByXY(textLocation, model.VerticalVector);
        //    }
        //    model.TextNoteIds = textNotes.Select(c => c.Id).ToList();
        //    //测试用
        //    //GraphicsDisplayerManager.Display(@"E:\WorkingSpace\Outputs\Images\1023结构做法标注.png", lines, Model.TextLocations);
        //}

        //internal void Regenerate(Document doc, CSAModel model, Element target)
        //{
        //    CSAContext.Creater.Regenerate(doc, model, target, new XYZ(0, 0, 0));
        //}

        //internal void Regenerate(Document doc, CSAModel model, Element target, XYZ offset)
        //{
        //    //不是选取的文本类型 以Text的文本类型为准
        //    if (model.TextNoteTypeElementId == null)
        //        model.TextNoteTypeElementId = (doc.GetElement(model.TextNoteIds[0]) as TextNote).TextNoteType.Id;
        //    //删除线
        //    foreach (var item in model.LineIds)
        //        if (doc.GetElement(item) != null)
        //            doc.Delete(item);
        //    //删除标注
        //    foreach (var item in model.TextNoteIds)
        //        if (doc.GetElement(item) != null)
        //            doc.Delete(item);
        //    Generate(doc, model, target, offset);
        //}

        //internal void Clear(Document doc, CSAModel model)
        //{
        //    //删除线
        //    foreach (var item in model.LineIds)
        //        if (doc.GetElement(item) != null)
        //            doc.Delete(item);
        //    //删除标注
        //    foreach (var item in model.TextNoteIds)
        //        if (doc.GetElement(item) != null)
        //            doc.Delete(item);
        //}
        #endregion


        #region 线族方案
        public void Generate(Document doc, CSAModel model, Element element)
        {
            Generate(doc, model, element, null);
        }
        public void Generate(Document doc, CSAModel model, Element element, XYZ offset)
        {
            CompoundStructure compoundStructure = model.GetCompoundStructure(element);//获取文本载体
            if (compoundStructure == null)
                return;
            var texts = model.FetchTextsFromCompoundStructure(doc, compoundStructure);//获取文本数据
            if (texts.Count == 0)
                return;
            if (texts.Count == 1)
            {
                TaskDialog.Show("警告", "暂不支持单层的结构做法标注");
            }
            else
            {
                model.TargetId = element.Id;//主体
                var lineFamilySymbol = VLConstraintsForCSA.GetMultipleTagSymbol(doc);//获取线标注类型 
                bool isRegenerate = offset != null;
                FamilyInstance line;
                if (isRegenerate)
                {
                    line = doc.GetElement(model.LineId) as FamilyInstance;
                    model.CalculateLocations(element, line, offset);//计算内容定位
                    Clear(doc, model);
                    line = doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), lineFamilySymbol, doc.ActiveView);//生成 线
                }
                else
                {
                    line = doc.Create.NewFamilyInstance(new XYZ(0, 0, 0), lineFamilySymbol, doc.ActiveView);//生成 线
                    model.CalculateLocations(element, line, offset);//计算内容定位
                }
                var lineLocation = model.LineLocation;
                var textLocations = model.TextLocations;
                ElementTransformUtils.MoveElement(doc, line.Id, lineLocation);//线定位
                LocationPoint locationPoint = line.Location as LocationPoint;//线 旋转处理
                locationPoint.RotateByXY(lineLocation, model.VerticalVector);
                model.LineId = line.Id;
                model.UpdateLineParameters(line, model.LineHeight, model.LineWidth, model.LineSpace, model.Texts.Count());//线参数设置
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
        }
        public void Regenerate(Document doc, CSAModel model, Element target)
        {
            FamilyInstance line = doc.GetElement(model.LineId) as FamilyInstance;
            CSAContext.Creator.Regenerate(doc, model, target, (line.Location as LocationPoint).Point - model.LineLocation);
        }
        public void Regenerate(Document doc, CSAModel model, Element target, XYZ offset)
        {
            //不是选取的文本类型 以Text的文本类型为准
            if (model.TextNoteTypeElementId == null)
                model.TextNoteTypeElementId = (doc.GetElement(model.TextNoteIds[0]) as TextNote).TextNoteType.Id;
            Generate(doc, model, target, offset);
        }
        public void Clear(Document doc, CSAModel model)
        {
            //删除线
            if (model.LineId != null && doc.GetElement(model.LineId) != null)
                doc.Delete(model.LineId);
            //删除标注
            foreach (var item in model.TextNoteIds)
                if (doc.GetElement(item) != null)
                    doc.Delete(item);
        }
        #endregion
    }
}
