﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MyRevit.MyTests.CompoundStructureAnnotation;
using PmSoft.Optimization.DrawingProduction;

namespace MyRevit.MyTests.BeamAlignToFloor
{
    [Transaction(TransactionMode.Manual)]
    public class CommandOfCompoundStructureAnnotation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var app = commandData.Application.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = commandData.Application.ActiveUIDocument.Document;
            return new CompoundStructureAnnotationSet(uiApp).DoCmd() ? Result.Succeeded : Result.Failed;
        }
    }
}
