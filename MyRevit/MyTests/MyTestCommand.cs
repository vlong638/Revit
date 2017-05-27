using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PmSoft.Common.CommonClass;
using System.IO;
using System.Linq;

namespace MyRevit.Entities
{
    [Transaction(TransactionMode.Manual)]
    class MyTestCommand : IExternalCommand
    {
        static string SharedParameterFile = Path.Combine(ApplicationPath.GetParentPathOfCurrent, "ConstructionManagement.txt");
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            doc.Application.SharedParametersFilename = SharedParameterFile;
            if (!File.Exists(SharedParameterFile))
            {
                var fs = File.Create(SharedParameterFile);
                fs.Close();
            }
            DefinitionFile definitionFile = doc.Application.OpenSharedParameterFile();
            return Result.Succeeded;
        }
    }
}
