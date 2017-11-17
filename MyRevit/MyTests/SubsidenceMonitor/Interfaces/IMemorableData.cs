namespace MyRevit.SubsidenceMonitor.Interfaces
{
    /// <summary>
    /// (职责模型)
    /// 备份,提交,回退
    /// </summary>
    /// <typeparam name="IStorage"></typeparam>
    /// <typeparam name="IData"></typeparam>
    public abstract class IMemorableData<IData> where IData : ICloneableData<IData>
    {
        public IMemorableData(IData data)
        {
            Init(data);
        }
        void Init(IData data)
        {
            Data = data;
        }

        public IData Data { set; get; }
        protected IData Memo { set; get; }

        public void Start()
        {
            Memo = Data.Clone();
        }
        public void Rollback()
        {
            Data = Memo;
            Memo = Data.Clone();
        }
    }
}
