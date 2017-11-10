using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    public abstract class VLModelCollectionBase<T>: VLSerialize 
        where T: VLModelBase<T>, new()
    {
        public static string EntitySplitter { get { return VLConstraints.EntitySplitter; } }
        public static char EntitySplitter_Char { get { return VLConstraints.EntitySplitter_Char; } }
        public List<T> Datas = new List<T>();

        public VLModelCollectionBase(string data)
        {
            LoadData(data);
        }

        public  string ToData()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Datas)
            {
                sb.AppendItem(item.ToData());
            }
            return sb.ToData();
        }
        //public abstract bool LoadData(string data);
        public bool LoadData(string dataStr)
        {
            Datas = new List<T>();
            var sr = new StringReader(dataStr);
            var subData= sr.ReadFormatString();
            while (subData != null)
            {
                var t = new T();
                if (t.LoadData(subData))
                    Datas.Add(t);
            }
            return true;
        }
    }
}
