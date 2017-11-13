using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyRevit.MyTests.CompoundStructureAnnotation
{
    public abstract class VLModelCollectionBase<T>: VLSerialize 
        where T: VLModelBase<T>, new()
    {
        public List<T> Data = new List<T>();

        public VLModelCollectionBase(string data)
        {
            LoadData(data);
        }

        public string ToData()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Data)
                sb.Append(item.ToData());
            return sb.ToString();
        }

        //public abstract bool LoadData(string data);
        public bool LoadData(string dataStr)
        {
            Data = new List<T>();
            var sr = new StringReader(dataStr);
            var subData= sr.ReadFormatString();
            while (subData != null)
            {
                var t = new T();
                if (t.LoadData(subData))
                    Data.Add(t);
                subData = sr.ReadFormatString();
            }
            return true;
        }
    }
}
