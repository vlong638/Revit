using System.Collections.Generic;
using System.Windows.Forms;

namespace MyRevit.EarthWork.Controls
{
    public class HeaderNode
    {
        public List<HeaderNode> SubNodes = new List<HeaderNode>();

        public int Height { set; get; }
        public int Width { set; get; }
    }


    public class ComplexHeaderDGV: DataGridView
    {
        public List<HeaderNode> HeaderNodes = new List<HeaderNode>();

        public ComplexHeaderDGV():base()
        {
            AllowUserToOrderColumns = false;//不允许用户编辑列
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            //首列不处理
            if (e.ColumnIndex<0)
            {
                MessageBox.Show($"e.ColumnIndex(-1).Width:{e.CellBounds.Width}");
                base.OnCellPainting(e);
                return;
            }
            //绘制处理
            if (e.RowIndex == -1)
            {

            }
            base.OnCellPainting(e);
        }
    }
}
