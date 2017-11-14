using Autodesk.Revit.DB;
namespace MyRevit.MyTests.VLBase
{
    interface VLStorable
    {
        bool Save(Document doc);
    }
}
