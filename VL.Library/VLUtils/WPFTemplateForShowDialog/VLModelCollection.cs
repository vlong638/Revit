using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VL.Library
{
    public abstract class VLModelCollection<T>: VLSerializable 
        where T: VLModel, new()
    {
        public List<T> Data = new List<T>();

        public VLModelCollection(string data)
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
