namespace MyRevit.EarthWork.Entity
{
    public abstract class MemorableData<TStorage, TData>
    {
        protected TData Memo { set; get; }
        protected int MemoHashCode { set; get; }
        protected abstract TData Clone();
        public abstract int GetSimpleHashCode();
        public bool IsChanged { get { return MemoHashCode != GetSimpleHashCode(); } }
        public void Start()
        {
            Memo = Clone();
            MemoHashCode = Memo.GetHashCode();
        }
        public abstract void Commit(TStorage storage);
        public abstract void Rollback();
    }
}
