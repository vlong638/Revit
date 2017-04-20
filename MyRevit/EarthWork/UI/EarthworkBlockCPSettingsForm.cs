using MyRevit.EarthWork.Entity;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyRevit.EarthWork.UI
{
    public partial class EarthworkBlockCPSettingsForm : Form
    {
        EarthworkBlockCPSettings CPSettings;
        EarthworkBlockCPSettings CPSettingsMemo;
        EarthworkBlockingForm Mainform;

        public EarthworkBlockCPSettingsForm(EarthworkBlockingForm form,EarthworkBlockCPSettings cpSettings)
        {
            InitializeComponent();

            //传值存储
            Mainform = form;
            CPSettings = cpSettings;
            CPSettingsMemo = CPSettings.Clone();
            //控件参数初始化
            traceBar_Transparency.Minimum = 0;
            traceBar_Transparency.Maximum = 100;
            cb_IsVisible.Checked = CPSettings.IsVisible;
            cb_IsHalftone.Checked = CPSettings.IsHalftone;
            traceBar_Transparency.Value = CPSettings.SurfaceTransparency;
            tb_Transparency.Text = CPSettings.SurfaceTransparency.ToString();
            btn_Color.ImageAlign = ContentAlignment.MiddleLeft;
            btn_Color.TextAlign = ContentAlignment.MiddleLeft;
            UpdateCPSettings(CPSettings.Color);
        }

        private void UpdateCPSettings(Color color)
        {
            int width = 18;
            var height = width;
            var image = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(image);
            graphics.FillRectangle(new SolidBrush(color), new Rectangle(0, 0, width, height));
            btn_Color.Image = image;
            btn_Color.Text = $"   RGB {color.R}-{color.G}-{color.B}";
        }

        /// <summary>
        /// 确定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Submit_Click(object sender, EventArgs e)
        {
            btn_Apply_Click(sender, e);
            this.Close();
        }
        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Cancel_Click(object sender, EventArgs e)
        {
            CPSettings.Copy(CPSettingsMemo);
            this.Close();
        }
        /// <summary>
        /// 应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Apply_Click(object sender, EventArgs e)
        {
            CPSettings.IsVisible = cb_IsVisible.Checked;
            CPSettings.IsHalftone = cb_IsHalftone.Checked;
            CPSettings.SurfaceTransparency = traceBar_Transparency.Value;
            CPSettingsMemo = CPSettings.Clone();
            //保存数据
            PmSoft.Common.CommonClass.FaceRecorderForRevit recorder = new PmSoft.Common.CommonClass.FaceRecorderForRevit(nameof(EarthworkBlockingForm)
                , PmSoft.Common.CommonClass.ApplicationPath.GetCurrentPath(Mainform.m_Doc));
            recorder.WriteValue(nameof(EarthworkBlocking), Newtonsoft.Json.JsonConvert.SerializeObject(Mainform.Blocking));
            //更新视图内容
            CPSettings.ApplySetting(Mainform.Blocking, Mainform.Block.ElementIds);
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
        /// 滑动透明值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void traceBar_Transparency_Scroll(object sender, EventArgs e)
        {
            tb_Transparency.TextChanged -= tb_Transparency_TextChanged;
            tb_Transparency.Text = traceBar_Transparency.Value.ToString();
            tb_Transparency.TextChanged += tb_Transparency_TextChanged;
        }
        /// <summary>
        /// 设置透明值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_Transparency_TextChanged(object sender, EventArgs e)
        {
            traceBar_Transparency.Scroll -= traceBar_Transparency_Scroll;
            int t = 0;
            if (int.TryParse(tb_Transparency.Text, out t))
            {
                if (t>100)
                {
                    t = 100;
                }
                if (t<0)
                {
                    t = 0;
                }
            }
            else
            {
                t = 0;
            }
            tb_Transparency.Text = t.ToString();
            traceBar_Transparency.Value = t;
            traceBar_Transparency.Scroll += traceBar_Transparency_Scroll;
        }
        private void btn_Color_Click(object sender, EventArgs e)
        {
            //禁止使用自定义颜色  
            colorDialog1.AllowFullOpen = false;
            //提供自己给定的颜色  
            colorDialog1.CustomColors = new int[] { 6916092, 15195440, 16107657, 1836924, 3758726, 12566463, 7526079, 7405793, 6945974, 241502, 2296476, 5130294, 3102017, 7324121, 14993507, 11730944 };
            colorDialog1.ShowHelp = true;
            if (colorDialog1.ShowDialog()==DialogResult.OK)
            {
                CPSettings.Color = colorDialog1.Color;
                UpdateCPSettings(CPSettings.Color);
            }
        }
    }
}
