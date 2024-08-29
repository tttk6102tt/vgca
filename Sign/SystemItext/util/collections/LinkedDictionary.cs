using System.Collections;

namespace Sign.SystemItext.util.collections
{
    public class LinkedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, IEnumerable
        {
            public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
            {
                private IEnumerator<KeyValuePair<TKey, TValue>> enu;

                public TValue Current => enu.Current.Value;

                object IEnumerator.Current => enu.Current.Value;

                public Enumerator(LinkedList<KeyValuePair<TKey, TValue>> link)
                {
                    enu = link.GetEnumerator();
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    return enu.MoveNext();
                }

                public void Reset()
                {
                    enu.Reset();
                }
            }

            private LinkedList<KeyValuePair<TKey, TValue>> link;

            public int Count => link.Count;

            public bool IsReadOnly => true;

            public ValueCollection(LinkedList<KeyValuePair<TKey, TValue>> link)
            {
                this.link = link;
            }

            public void Add(TValue item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TValue item)
            {
                foreach (KeyValuePair<TKey, TValue> item2 in link)
                {
                    if (EqualityComparer<TValue>.Default.Equals(item, item2.Value))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                foreach (KeyValuePair<TKey, TValue> item in link)
                {
                    array[arrayIndex++] = item.Value;
                }
            }

            public bool Remove(TValue item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<TValue> GetEnumerator()
            {
                return new Enumerator(link);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(link);
            }
        }

        public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, IEnumerable
        {
            public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator
            {
                private IEnumerator<KeyValuePair<TKey, TValue>> enu;

                public TKey Current => enu.Current.Key;

                object IEnumerator.Current => enu.Current.Key;

                public Enumerator(LinkedList<KeyValuePair<TKey, TValue>> link)
                {
                    enu = link.GetEnumerator();
                }

                public void Dispose()
                {
                }

                public bool MoveNext()
                {
                    return enu.MoveNext();
                }

                public void Reset()
                {
                    enu.Reset();
                }
            }

            private LinkedList<KeyValuePair<TKey, TValue>> link;

            private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> dic;

            public int Count => link.Count;

            public bool IsReadOnly => true;

            public KeyCollection(LinkedList<KeyValuePair<TKey, TValue>> link, Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> dic)
            {
                this.link = link;
                this.dic = dic;
            }

            public void Add(TKey item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TKey item)
            {
                return dic.ContainsKey(item);
            }

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                foreach (KeyValuePair<TKey, TValue> item in link)
                {
                    array[arrayIndex++] = item.Key;
                }
            }

            public bool Remove(TKey item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<TKey> GetEnumerator()
            {
                return new Enumerator(link);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(link);
            }
        }

        private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> dic;

        private LinkedList<KeyValuePair<TKey, TValue>> link;

        public virtual ICollection<TKey> Keys => new KeyCollection(link, dic);

        public virtual ICollection<TValue> Values => new ValueCollection(link);

        public TValue this[TKey key]
        {
            get
            {
                return dic[key].Value.Value;
            }
            set
            {
                if (dic.ContainsKey(key))
                {
                    LinkedListNode<KeyValuePair<TKey, TValue>> node = dic[key];
                    LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, value));
                    link.AddAfter(node, linkedListNode);
                    link.Remove(node);
                    dic[key] = linkedListNode;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count => dic.Count;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        public LinkedDictionary()
        {
            dic = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
            link = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        public virtual void Add(TKey key, TValue value)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, value));
            dic.Add(key, linkedListNode);
            link.AddLast(linkedListNode);
        }

        public virtual bool ContainsKey(TKey key)
        {
            return dic.ContainsKey(key);
        }

        public virtual bool Remove(TKey key)
        {
            if (dic.ContainsKey(key))
            {
                link.Remove(dic[key]);
                dic.Remove(key);
                return true;
            }

            return false;
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            if (dic.ContainsKey(key))
            {
                value = dic[key].Value.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            dic.Clear();
            link.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!dic.ContainsKey(item.Key))
            {
                return false;
            }

            return EqualityComparer<TValue>.Default.Equals(dic[item.Key].Value.Value, item.Value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            link.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (dic.ContainsKey(item.Key) && EqualityComparer<TValue>.Default.Equals(dic[item.Key].Value.Value, item.Value))
            {
                Remove(item.Key);
                return true;
            }

            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return link.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return link.GetEnumerator();
        }
    }
}
