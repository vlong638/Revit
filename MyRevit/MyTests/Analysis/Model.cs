using System;
using Autodesk.Revit.DB;
using MyRevit.MyTests.VLBase;
using PmSoft.Optimization.DrawingProduction.Utils;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace MyRevit.MyTests.Analysis
{
    public class AnalysisModel : VLModel
    {
        public ElementId TargetId { set; get; }
        public string RootPath { set; get; }
        public Document Document { get; internal set; }
        public string DisplayName { get; internal set; }

        public AnalysisModel(string data) : base(data)
        {
        }


        public override bool LoadData(string data)
        {
            if (string.IsNullOrEmpty(data))
                return false;
            try
            {
                StringReader sr = new StringReader(data);
                RootPath = sr.ReadFormatString();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public override string ToData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendItem(RootPath);
            return sb.ToString();
        }

        public void DisplayGeometry()
        {
            var element = Document.GetElement(TargetId);
            var geometryElement = element.get_Geometry(new Options() { View = Document.ActiveView }) as GeometryElement;
            var directory = GetDirectoryPath();
            foreach (var geometryObject in geometryElement)
            {
                if (geometryObject is Solid)
                {
                    var solid = geometryObject as Solid;
                    int index = 1;
                    foreach (Face face in solid.Faces)
                    {
                        List<List<XYZ>> ptArrayLoops = new List<List<XYZ>>();
                        foreach (EdgeArray edgeArray in face.EdgeLoops)
                                foreach (Edge edge in edgeArray)
                                    ptArrayLoops.Add(edge.Tessellate().ToList());
                        double xMin = ptArrayLoops.AsParallel().Min(c1 => c1.Min(c2 => c2.X));
                        double xMax = ptArrayLoops.AsParallel().Max(c1 => c1.Max(c2 => c2.X));
                        double yMin = ptArrayLoops.AsParallel().Min(c1 => c1.Min(c2 => c2.Y));
                        double yMax = ptArrayLoops.AsParallel().Max(c1 => c1.Max(c2 => c2.Y));
                        string normalStr = "";
                        if (face is PlanarFace)
                        {
                            var normal = (face as PlanarFace).Normal;
                            normalStr = normal.X.ToString("f2") + "_" + normal.Y.ToString("f2") + "_" + normal.Z.ToString("f2");
                        }
                        string fileName = string.Format("{0}Face{1}.png", index , normalStr);
                        GraphicsDisplayerV2.Display(xMin, xMax, yMin, yMax,Path.Combine(directory, fileName), () =>
                        {
                            GraphicsDisplayerV2.DisplayLines(ptArrayLoops, GraphicsDisplayerV2.DefaultPen, true, true);
                        });
                        index++;
                    }
                }
                else
                {
                    throw new NotImplementedException("出现了非预期内的GeometryObject");
                }
            }
        }

        private string GetDirectoryPath()
        {
            var directory = Path.Combine(RootPath, DateTime.Now.ToString("MM_dd_hh_mm_ss_") + DisplayName);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return directory;
        }
    }
}