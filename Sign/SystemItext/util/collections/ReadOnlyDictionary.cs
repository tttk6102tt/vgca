using System.Collections;

namespace Sign.SystemItext.util.collections
{
    [Serializable]
    public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, ICollection
    {
        private readonly IDictionary<TKey, TValue> source;

        private object syncRoot;

        public virtual int Count => source.Count;

        public virtual ICollection<TKey> Keys => source.Keys;

        public virtual ICollection<TValue> Values => source.Values;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot
        {
            get
            {
                if (syncRoot == null)
                {
                    ICollection collection = source as ICollection;
                    if (collection != null)
                    {
                        syncRoot = collection.SyncRoot;
                    }
                    else
                    {
                        Interlocked.CompareExchange(ref syncRoot, new object(), null);
                    }
                }

                return syncRoot;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return source[key];
            }
            set
            {
                ThrowNotSupportedException();
            }
        }

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionaryToWrap)
        {
            if (dictionaryToWrap == null)
            {
                throw new ArgumentNullException("dictionaryToWrap");
            }

            source = dictionaryToWrap;
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            ThrowNotSupportedException();
        }

        public virtual bool ContainsKey(TKey key)
        {
            return source.ContainsKey(key);
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            ThrowNotSupportedException();
            return false;
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            return source.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            ThrowNotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            ThrowNotSupportedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return source.Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            source.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            ThrowNotSupportedException();
            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return source.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)new List<KeyValuePair<TKey, TValue>>(source)).CopyTo(array, index);
        }

        private static void ThrowNotSupportedException()
        {
            throw new NotSupportedException("This Dictionary is read-only");
        }
    }
}
