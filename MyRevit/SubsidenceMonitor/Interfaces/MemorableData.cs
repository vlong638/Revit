using MyRevit.SubsidenceMonitor.Operators;

namespace MyRevit.SubsidenceMonitor.Interfaces
{
    /// <summary>
    /// (职责模型)
    /// 备份,提交,回退
    /// </summary>
    /// <typeparam name="IStorage"></typeparam>
    /// <typeparam name="IData"></typeparam>
    public abstract class MemorableData<IStorage, IData, ITemporaryData>
        where ITemporaryData : new()
    {
        public MemorableData(IStorage storage, IData data)
        {
            Init(storage, data);
        }
        void Init(IStorage storage, IData data)
        {
            Storage = storage;
            Data = data;
        }

        public IData Data { set; get; }
        public ITemporaryData TemporaryData { set; get; } = new ITemporaryData();
        protected IStorage Storage { set; get; }
        protected IData Memo { set; get; }

        protected abstract IData Clone();
        public void Start()
        {
            Memo = Clone();
        }
        public abstract BLLResult Commit(bool isCreateNew);
        public abstract BLLResult Delete();
        public abstract void Rollback();
    }
}
