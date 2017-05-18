using Autodesk.Revit.DB;
using PmSoft.Common.CommonClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyRevit.Utilities
{
    public class PMSoftHelper
    {
        public static FaceRecorderForRevit GetRecorder(string segName, Document doc)
        {
            return new FaceRecorderForRevit(segName, doc.PathName);
        }
    }
}
