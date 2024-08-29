using System.Collections;

namespace Sign.SystemItext.util.collections
{
    public class HashSet2<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private Dictionary<T, object> set;

        public virtual int Count => set.Count;

        public virtual bool IsReadOnly => false;

        internal virtual Dictionary<T, object> InternalSet => set;

        public HashSet2()
        {
            set = new Dictionary<T, object>();
        }

        public HashSet2(IEnumerable<T> set)
            : this()
        {
            foreach (T item in set)
            {
                Add(item);
            }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return set.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(T item)
        {
            set[item] = null;
        }

        public virtual void AddAll(IEnumerable<T> set)
        {
            foreach (T item in set)
            {
                Add(item);
            }
        }

        public virtual void Clear()
        {
            set.Clear();
        }

        public virtual bool Contains(T item)
        {
            return set.ContainsKey(item);
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            set.Keys.CopyTo(array, arrayIndex);
        }

        public virtual bool Remove(T item)
        {
            return set.Remove(item);
        }

        public virtual bool IsEmpty()
        {
            return set.Count == 0;
        }

        public virtual bool RetainAll(ICollection<T> collection)
        {
            bool result = false;
            List<T> list = new List<T>();
            using (IEnumerator<T> enumerator = GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    if (!collection.Contains(current))
                    {
                        list.Add(current);
                    }
                }
            }

            foreach (T item in list)
            {
                Remove(item);
                result = true;
            }

            return result;
        }
    }
}
