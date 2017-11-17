using Autodesk.Revit.DB;
using MyRevit.SubsidenceMonitor.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MyRevit.SubsidenceMonitor.UI
{
    public partial class CPSettingsForm : System.Windows.Forms.Form
    {
        static List<Element> _fillPatterns;
        static List<Element> GetFillPatterns(Document doc)
        {
            if (_fillPatterns == null)
            {
                _fillPatterns = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).Where(p => (p as FillPatternElement).GetFillPattern().Target == FillPatternTarget.Drafting).ToList();
            }
            return _fillPatterns;
        }
        public object doc { set; get; }
        public CPSettings CPSettings { set; get; }

        public CPSettingsForm(Document Document, string cpSettingStr)
        {
            InitializeComponent();

            //控件参数初始化
            traceBar_Transparency.Minimum = 0;
            traceBar_Transparency.Maximum = 100;
            //对象初始化
            CPSettings = new CPSettings(cpSettingStr);
            cb_FillPattern.DisplayMember = "Name";
            cb_FillPattern.ValueMember = "Id";
            cb_FillPattern.DataSource = GetFillPatterns(Document);
            cb_IsVisible.Checked = CPSettings.IsVisible;
            cb_IsSurfaceVisible.Checked = CPSettings.IsSurfaceVisible;
            cb_IsHalftone.Checked = CPSettings.IsHalftone;
            traceBar_Transparency.Value = CPSettings.SurfaceTransparency;
            tb_Transparency.Text = CPSettings.SurfaceTransparency.ToString();
            btn_Color.ImageAlign = ContentAlignment.MiddleLeft;
            btn_Color.TextAlign = ContentAlignment.MiddleLeft;
            RenderColorButton(CPSettings.Color);

            //值变更事件
            cb_IsVisible.CheckedChanged += ValueChanged;
            cb_IsHalftone.CheckedChanged += ValueChanged;
            cb_IsSurfaceVisible.CheckedChanged += ValueChanged;
            cb_FillPattern.SelectedIndexChanged += ValueChanged;
            tb_Transparency.TextChanged += tb_Transparency_TextChanged;
        }

        /// <summary>
        /// 将选中颜色的内容渲染到按钮上
        /// </summary>
        /// <param name="color"></param>
        private void RenderColorButton(System.Drawing.Color color)
        {
            int width = 18;
            var height = width;
            var image = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(image);
            graphics.FillRectangle(new SolidBrush(color), new Rectangle(0, 0, width, height));
            btn_Color.Image = image;
            btn_Color.Text = $"   RGB {color.R}-{color.G}-{color.B}";
        }
        private void ValueChanged(object sender, EventArgs e)
        {
            CPSettings.IsVisible = cb_IsVisible.Checked;
            CPSettings.IsSurfaceVisible = cb_IsSurfaceVisible.Checked;
            CPSettings.IsHalftone = cb_IsHalftone.Checked;
            CPSettings.SurfaceTransparency = traceBar_Transparency.Value;
            CPSettings.FillerId = ((ElementId)cb_FillPattern.SelectedValue).IntegerValue;
        }
        /// <summary>
        /// 重置透明值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ResetTransparency_Click(object sender, EventArgs e)
        {
            tb_Transparency.Text = "0";
        }
        /// <summary>
        /// 颜色选择(界面)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Color_Click(object sender, EventArgs e)
        {
            //禁止使用自定义颜色  
            colorDialog1.AllowFullOpen = false;
            //提供自己给定的颜色  
            colorDialog1.CustomColors = new int[] { 6916092, 15195440, 16107657, 1836924, 3758726, 12566463, 7526079, 7405793, 6945974, 241502, 2296476, 5130294, 3102017, 7324121, 14993507, 11730944 };
            colorDialog1.ShowHelp = true;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                CPSettings.Color = colorDialog1.Color;
                RenderColorButton(CPSettings.Color);
                ValueChanged(sender, e);
            }
        }
        /// <summary>
        /// 滑动透明值(滑动条)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void traceBar_Transparency_Scroll(object sender, EventArgs e)
        {
            tb_Transparency.TextChanged -= tb_Transparency_TextChanged;
            tb_Transparency.Text = traceBar_Transparency.Value.ToString();
            tb_Transparency.TextChanged += tb_Transparency_TextChanged;
            ValueChanged(sender, e);
        }
        /// <summary>
        /// /设置透明值(文本)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_Transparency_TextChanged(object sender, EventArgs e)
        {
            traceBar_Transparency.Scroll -= traceBar_Transparency_Scroll;
            int t = 0;
            if (int.TryParse(tb_Transparency.Text, out t))
            {
                if (t > 100)
                    t = 100;
                if (t < 0)
                    t = 0;
            }
            else
            {
                t = 0;
            }
            tb_Transparency.Text = t.ToString();
            traceBar_Transparency.Value = t;
            traceBar_Transparency.Scroll += traceBar_Transparency_Scroll;
            ValueChanged(sender, e);
        }
        public string Data { set; get; }
        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Submit_Click(object sender, EventArgs e)
        {
            Data = CPSettings.SerializeToString();
            DialogResult = DialogResult.OK;
            this.Close();
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
