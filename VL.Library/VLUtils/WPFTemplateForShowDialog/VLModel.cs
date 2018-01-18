namespace VL.Library
{
    public abstract class VLModel : VLSerializable
    {
        public VLModel(string data)
        {
            LoadData(data);
        }

        public abstract bool LoadData(string data);
        public abstract string ToData();
    }
}
