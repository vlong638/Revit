using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyRevit.MyTests.VLBase
{
    /// <summary>
    /// 格式定制
    /// </summary>
    interface VLSerializable
    {
        bool LoadData(string data);
        string ToData();
    }
    /// <summary>
    /// Data构建用的扩展
    /// </summary>
    public static class StringBuilderEx
    {
        public static string Splitter = " ";
        public static string InnerSplitterForXYZ = ",";
        public static char InnerSplitterForXYZ_Char = ',';

        #region SetData
        public static void AppendItem(this StringBuilder sb, Enum e)
        {
            sb.AppendItem(e.ToString());
        }
        public static void AppendItem(this StringBuilder sb, ElementId elementId)
        {
            sb.AppendItem(elementId.IntegerValue.ToString());
        }
        public static void AppendItem(this StringBuilder sb, int i)
        {
            sb.AppendItem(i.ToString());
        }
        public static void AppendItem(this StringBuilder sb, string str)
        {
            sb.Append(GetFormatStr(str));
        }
        public static void AppendItem(this StringBuilder sb, IEnumerable<ElementId> items)
        {
            StringBuilder sub = new StringBuilder();
            foreach (var item in items)
                sub.Append(GetFormatStr(item.IntegerValue.ToString()));
            sb.Append(GetFormatStr(sub.ToString()));
        }
        public static void AppendItem(this StringBuilder sb, IEnumerable<string> items)
        {
            StringBuilder sub = new StringBuilder();
            foreach (var item in items)
                sub.Append(GetFormatStr(item));
            sb.Append(GetFormatStr(sub.ToString()));
        }
        public static void AppendItem(this StringBuilder sb, IEnumerable<XYZ> items)
        {
            StringBuilder sub = new StringBuilder();
            foreach (var item in items)
                sub.Append(GetFormatStr(item.X + InnerSplitterForXYZ + item.Y + InnerSplitterForXYZ + item.Z));
            sb.Append(GetFormatStr(sub.ToString()));
        }
        public static void AppendItem(this StringBuilder sb, XYZ item)
        {
            sb.Append(GetFormatStr(item.X + InnerSplitterForXYZ + item.Y + InnerSplitterForXYZ + item.Z));
        }
        public static string ToData(this StringBuilder sb)
        {
            return GetFormatStr(sb.ToString());
        }
        #endregion

        #region GetData
        public static string ReadFormatString(this StringReader sr)
        {
            char readOne;
            List<char> length = new List<char>();
            while (sr.Peek() >= 0)
            {
                readOne = (char)sr.Read();
                if (char.IsNumber(readOne))
                {
                    length.Add(readOne);
                }
                else
                {
                    var sLength = Convert.ToInt32(new string(length.ToArray()));
                    char[] buffer = new char[sLength];
                    sr.ReadBlock(buffer, 0, sLength);
                    return new string(buffer);
                }
            }
            return null;
        }
        public static ElementId ReadFormatStringAsElementId(this StringReader sr)
        {
            return new ElementId(Convert.ToInt32(ReadFormatString(sr)));
        }
        public static List<ElementId> ReadFormatStringAsElementIds(this StringReader sr)
        {
            List<ElementId> ids = new List<ElementId>();
            StringReader subSR = new StringReader(ReadFormatString(sr));
            var str = ReadFormatString(subSR);
            while (str != null)
            {
                ids.Add(new ElementId(Convert.ToInt32(str)));
                str = ReadFormatString(subSR);
            }
            return ids;
        }
        public static List<int> ReadFormatStringAsInt32s(this StringReader sr)
        {
            List<int> ids = new List<int>();
            StringReader subSR = new StringReader(ReadFormatString(sr));
            var str = ReadFormatString(subSR);
            while (str != null)
            {
                ids.Add(Convert.ToInt32(str));
                str = ReadFormatString(subSR);
            }
            return ids;
        }
        public static List<string> ReadFormatStringAsStrings(this StringReader sr)
        {
            List<string> ids = new List<string>();
            StringReader subSR = new StringReader(ReadFormatString(sr));
            var str = ReadFormatString(subSR);
            while (str != null)
            {
                ids.Add(str);
                str = ReadFormatString(subSR);
            }
            return ids;
        }
        public static T ReadFormatStringAsEnum<T>(this StringReader sr)
        {
            var str = ReadFormatString(sr);
            return (T)Enum.Parse(typeof(T), str);
        }
        public static XYZ ReadFormatStringAsXYZ(this StringReader sr)
        {
            var str = ReadFormatString(sr);
            var values = str.Split(InnerSplitterForXYZ_Char);
            return new XYZ(Convert.ToDouble(values[0]), Convert.ToDouble(values[1]), Convert.ToDouble(values[2]));
        }
        public static List<XYZ> ReadFormatStringAsXYZs(this StringReader sr)
        {
            List<XYZ> items = new List<XYZ>();
            StringReader subSR = new StringReader(ReadFormatString(sr));
            var str = ReadFormatString(subSR);
            while (str != null)
            {
                var values = str.Split(InnerSplitterForXYZ_Char);
                items.Add(new XYZ(Convert.ToDouble(values[0]), Convert.ToDouble(values[1]), Convert.ToDouble(values[2])));
                str = ReadFormatString(subSR);
            }
            return items;
        }

        #endregion
        private static string GetFormatStr(string str)
        {
            return str.Length + VLConstraints.Splitter + str;
        }
    }
}
