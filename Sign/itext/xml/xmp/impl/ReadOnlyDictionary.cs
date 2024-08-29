using System.Collections;

namespace Sign.itext.xml.xmp.impl
{
    public class ReadOnlyDictionary : IDictionary, ICollection, IEnumerable
    {
        private readonly IDictionary _originalDictionary;

        public virtual bool IsReadOnly => true;

        public object this[object key]
        {
            get
            {
                return _originalDictionary[key];
            }
            set
            {
                throw new NotSupportedException("Collection is read-only.");
            }
        }

        public virtual ICollection Values => _originalDictionary.Values;

        public virtual ICollection Keys => _originalDictionary.Keys;

        public virtual bool IsFixedSize => _originalDictionary.IsFixedSize;

        public virtual bool IsSynchronized => _originalDictionary.IsSynchronized;

        public virtual int Count => _originalDictionary.Count;

        public virtual object SyncRoot => _originalDictionary.SyncRoot;

        private ReadOnlyDictionary(IDictionary original)
        {
            _originalDictionary = original;
        }

        public static ReadOnlyDictionary ReadOnly(IDictionary dictionary)
        {
            return new ReadOnlyDictionary(dictionary);
        }

        private void ReportNotSupported()
        {
            throw new NotSupportedException("Collection is read-only.");
        }

        public virtual IDictionaryEnumerator GetEnumerator()
        {
            return _originalDictionary.GetEnumerator();
        }

        public virtual void Remove(object key)
        {
            ReportNotSupported();
        }

        public virtual bool Contains(object key)
        {
            return _originalDictionary.Contains(key);
        }

        public virtual void Clear()
        {
            ReportNotSupported();
        }

        public virtual void Add(object key, object value)
        {
            ReportNotSupported();
        }

        public virtual void CopyTo(Array array, int index)
        {
            _originalDictionary.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _originalDictionary.GetEnumerator();
        }
    }
}
