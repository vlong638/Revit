using MyRevit.SubsidenceMonitor.Interfaces;
using System;
using System.Drawing;
using System.Text;

namespace MyRevit.SubsidenceMonitor.Entities
{
    /// <summary>
    /// 颜色/透明度配置
    /// </summary>
    public class CPSettings : ITNodeData
    {
        #region Properties
        /// <summary>
        /// 图元可见
        /// </summary>
        public bool IsVisible { set; get; } = true;
        /// <summary>
        /// 半色调
        /// </summary>
        public bool IsHalftone { set; get; } = false;
        /// <summary>
        /// 表面可见
        /// </summary>
        public bool IsSurfaceVisible { set; get; } = true;
        /// <summary>
        /// 填充物 Surface即Projection
        /// </summary>
        public int FillerId { set; get; } = -1;
        /// <summary>
        /// 曲面透明度
        /// </summary>
        public int SurfaceTransparency { set; get; }
        /// <summary>
        /// 颜色 Surface即Projection
        /// </summar>y
        public Color Color { set; get; } = Color.White;
        #endregion

        #region Constructors
        public CPSettings(bool isVisible, bool isHalftone, bool isSurfaceVisible, Color color, int fillerId, int surfaceTransparency)
        {
            Init(isVisible, isHalftone, isSurfaceVisible, color, fillerId, surfaceTransparency);
        }
        void Init(bool isVisible, bool isHalftone, bool isSurfaceVisible, Color color, int fillerId, int surfaceTransparency)
        {
            IsVisible = isVisible;
            IsHalftone = isHalftone;
            IsSurfaceVisible = isSurfaceVisible;
            Color = color;
            FillerId = fillerId;
            SurfaceTransparency = surfaceTransparency;
        }

        public CPSettings(string str)
        {
            DeserializeFromString(str);
        }
        static char SeperatorChar = ',';
        static string SeperatorString = ",";
        public string SerializeToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(IsVisible + SeperatorString);
            sb.Append(IsHalftone + SeperatorString);
            sb.Append(IsSurfaceVisible + SeperatorString);
            sb.Append(ColorTranslator.ToHtml(Color) + SeperatorString);
            sb.Append(FillerId + SeperatorString);
            sb.Append(SurfaceTransparency + SeperatorString);
            return sb.ToString();
        }
        public void DeserializeFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;
            var args = str.Split(SeperatorChar);
            IsVisible = Convert.ToBoolean(args[0]);
            IsHalftone = Convert.ToBoolean(args[1]);
            IsSurfaceVisible = Convert.ToBoolean(args[2]);
            Color = ColorTranslator.FromHtml(args[3]);
            FillerId = Convert.ToInt32(args[4]);
            SurfaceTransparency = Convert.ToInt32(args[5]);
        } 
        #endregion
    }
}
