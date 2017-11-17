using System.Collections.Generic;

namespace MyRevit.EarthWork.Entity
{
    public abstract class SectionalData<TStorage, TSelf, TNode>
    {
        protected TSelf Memo { set; get; }
        protected int MemoHashCode { set; get; }
        protected abstract TSelf Clone();
        public abstract int GetSimpleHashCode();
        protected List<TNode> Adds { set; get; } = new List<TNode>();
        protected List<TNode> Deletes { set; get; } = new List<TNode>();
        public void Add(List<TNode> elements)
        {
            if (elements != null && elements.Count > 0)
            {
                for (int i = elements.Count; i > 0; i--)
                {
                    Add(elements[i - 1]);
                }
            }
        }
        public abstract void Add(TNode element);
        public void Delete(List<TNode> elements)
        {
            if (elements != null && elements.Count > 0)
            {
                for (int i = elements.Count; i > 0; i--)
                {
                    Delete(elements[i - 1]);
                }
            }
        }
        public abstract void Delete(TNode element);
        public bool IsChanged { get { return MemoHashCode != GetSimpleHashCode(); } }
        public abstract void Start();
        public abstract void Commit(TStorage storage);
        public abstract void Rollback();
    }
    public abstract class SectionalData<TStorage, TRoot, TNode, TElement>
    {
        protected TNode Memo { set; get; }
        protected int MemoHashCode { set; get; }
        protected abstract TNode Clone();
        public abstract int GetSimpleHashCode();
        protected List<TElement> Adds { set; get; } = new List<TElement>();
        protected List<TElement> Deletes { set; get; } = new List<TElement>();
        public void Add(TRoot root, List<TElement> elements)
        {
            if (elements != null && elements.Count > 0)
            {
                for (int i = elements.Count; i > 0; i--)
                {
                    Add(root, elements[i - 1]);
                }
            }
        }
        public abstract void Add(TRoot root, TElement element);
        public void Delete(TRoot root, List<TElement> elements)
        {
            if (elements != null && elements.Count > 0)
            {
                for (int i = elements.Count; i > 0; i--)
                {
                    Delete(root, elements[i - 1]);
                }
            }
        }
        public abstract void Delete(TRoot root, TElement element);
        public bool IsChanged { get { return Adds.Count > 0 || Deletes.Count > 0 || MemoHashCode != GetSimpleHashCode(); } }
        public void Start()
        {
            Memo = Clone();
            MemoHashCode = GetSimpleHashCode();
        }
        public abstract void Commit(TStorage storage);
        public abstract void Rollback();
    }
}
