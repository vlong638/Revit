using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MyRevit
{
    public class HeaderNode
    {
        public HeaderNode(int columnIndex, int height, int width, string headerText, string propertyName, List<HeaderNode> subNodes = null)
        {
            ColumnIndex = columnIndex;
            Height = height;
            Width = width;
            HeaderText = headerText;
            PropertyName = propertyName;
            SubNodes = subNodes;
        }

        /// <summary>
        /// 对应重绘时的Index,因为有子节点现象,子节点是不予重绘的
        /// </summary>
        public int ColumnIndex { set; get; }
        public string ColumnName { set; get; }
        public int Height { set; get; }
        public int Width { set; get; }
        public string HeaderText { set; get; }
        public string PropertyName { set; get; }
        public List<HeaderNode> SubNodes { set; get; }
        public int GetMaxHeight()
        {
            if (SubNodes == null)
            {
                return Height;
            }
            else
            {
                return Height + SubNodes.Max(c => c.GetMaxHeight());
            }
        }
        public int GetMaxLevel()
        {
            if (SubNodes == null)
            {
                return 1;
            }
            else
            {
                return 1 + SubNodes.Max(c => c.GetMaxLevel());
            }
        }
        public bool IsLeaf()
        {
            return SubNodes == null || SubNodes.Count() == 0;
        }
        public void AppendLeavesTo(ref List<HeaderNode> result)
        {
            if (IsLeaf())
            {
                result.Add(this);
            }
            else
            {
                foreach (var SubNode in SubNodes)
                {
                    SubNode.AppendLeavesTo(ref result);
                }
            }
        }
        public int GetLeavesCount()
        {
            if (IsLeaf())
            {
                return 1;
            }
            else
            {
                return SubNodes.Sum(c => c.GetLeavesCount());
            }
        }
        public void Paint(MyDGV0427 dgv, PaintEventArgs e, Point location)
        {
            //区域填充
            Rectangle r1 = new Rectangle()
            {
                X = location.X + 1,
                Y = location.Y + 1,
                Width = Width - 2,
                Height = Height - 2,
            };
            e.Graphics.FillRectangle(new SolidBrush(dgv.ColumnHeadersDefaultCellStyle.BackColor), r1);
            //文字
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString(ColumnName,
                dgv.ColumnHeadersDefaultCellStyle.Font,
                new SolidBrush(dgv.ColumnHeadersDefaultCellStyle.ForeColor),
                r1,
                format);
            //递归处理子节点
            if (!IsLeaf())
            {
                int width = 0;
                foreach (var SubNode in SubNodes)
                {
                    SubNode.Paint(dgv, e, new Point(location.X + width, location.Y + Height));
                    width += SubNode.Width;
                }
            }
        }
        public void Paint(MyDGV0427 dgv, DataGridViewCellPaintingEventArgs e, Point location)
        {
            //区域填充
            Rectangle r1 = new Rectangle()
            {
                X = location.X + 1,
                Y = location.Y + 1,
                Width = Width - 2,
                Height = Height - 2,
            };
            e.Graphics.FillRectangle(new SolidBrush(dgv.ColumnHeadersDefaultCellStyle.BackColor), r1);
            SolidBrush gridBrush = new SolidBrush(dgv.GridColor);
            Pen gridLinePen = new Pen(gridBrush);
            //描边-底线
            e.Graphics.DrawLine(gridLinePen
                , r1.Left
                , r1.Bottom
                , r1.Right
                , r1.Bottom);
            //描边-右端线
            e.Graphics.DrawLine(gridLinePen
                , r1.Right
                , r1.Top
                , r1.Right
                , r1.Bottom);
            //文字
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            e.Graphics.DrawString(HeaderText,
                dgv.ColumnHeadersDefaultCellStyle.Font,
                new SolidBrush(dgv.ColumnHeadersDefaultCellStyle.ForeColor),
                r1,
                format);
            //递归处理子节点
            if (!IsLeaf())
            {
                int width = 0;
                foreach (var SubNode in SubNodes)
                {
                    SubNode.Paint(dgv, e, new Point(location.X + width, location.Y + Height));
                    width += SubNode.Width;
                }
            }
        }
    }
    public class MyDGV0427 : DataGridView
    {
        List<HeaderNode> _HeaderNodes;
        public List<HeaderNode> HeaderNodes
        {
            set
            {
                _HeaderNodes = value;

                Columns.Clear();
                if (value == null)
                    return;
                List<HeaderNode> leaves = new List<HeaderNode>();
                foreach (var subNode in value)
                {
                    subNode.AppendLeavesTo(ref leaves);
                }
                for (int i = 0; i < leaves.Count(); i++)
                {
                    var node = leaves[i];
                    Columns.Add(node.ColumnName, node.HeaderText);
                    var column = Columns[i];
                    if (column.DataPropertyName != null)
                        column.DataPropertyName = node.PropertyName;
                    column.Width = node.Width;
                }
                ColumnHeadersHeight = value.Max(c => c.GetMaxHeight());
            }
            get
            {
                return _HeaderNodes;
            }
        }

        public MyDGV0427() : base()
        {
            AutoGenerateColumns = false;//列名根据设置固化
            AllowUserToResizeColumns = false;//列宽编辑
            AllowUserToResizeRows = false;//行高编辑
            AllowUserToOrderColumns = false;//不允许用户编辑列
            AllowUserToAddRows = false;//动态增加行
            AllowUserToDeleteRows = false;//动态删除行
            AllowUserToResizeRows = false;//编辑行高
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
            ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;

            //Paint += MyDGV0427_Paint;
            CellPainting += MyDGV0427_CellPainting;
        }

        private void MyDGV0427_Paint(object sender, PaintEventArgs e)
        {
            if (HeaderNodes == null)
                return;

            int index = 0;
            foreach (var HeaderNode in HeaderNodes)
            {
                HeaderNode.Paint(this, e, this.GetCellDisplayRectangle(index, -1, true).Location);
                index += HeaderNode.GetLeavesCount();
            }
        }
        private void MyDGV0427_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex > -1)
            {
                var headerNode = HeaderNodes.FirstOrDefault(c => c.ColumnIndex == e.ColumnIndex);
                if (headerNode != null)
                    headerNode.Paint(this, e, e.CellBounds.Location);
                e.Handled = true;

                //e.PaintBackground(e.CellBounds, false);
                //System.Drawing.Rectangle r2 = e.CellBounds;
                //r2.Y += e.CellBounds.Height / 2;
                //r2.Height += e.CellBounds.Height / 2;
                //e.PaintContent(r2);
                //e.Handled = true;
            }
        }
    }
}
