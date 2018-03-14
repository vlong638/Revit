using Autodesk.Revit.UI;
using PmSoft.MepProject.MepWork.General.MEPCurveTurn;

namespace PmSoft.MepProject.MepWork.CommandExe
{
    /// <summary>
    /// 
    /// <remarks>作者:夏锦慧，创建日期:2018/03/12 09:20，修改日期:</remarks>
    /// </summary>
    public class MEPCurveAutomaticTurnExe
    {
        public static Result Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            if (!PmSoft.MainModel.MainUI.CommandExe.PmAuthorization.Instance().HasJDJMModule())
            {
                return Result.Cancelled;
            }

            MEPCurveAutomaticTurnSet turn = new MEPCurveAutomaticTurnSet(commandData);
            if (turn.DoCmd())
                return Result.Succeeded;
            else
                return Result.Failed;
        }
    }
}
