using Sign.itext.error_messages;

namespace Sign.itext.text.pdf
{
    public class LongHashtable
    {
        public class LongHashtableEntry
        {
            internal int hash;

            internal long key;

            internal long value;

            internal LongHashtableEntry next;

            public virtual long Key => key;

            public virtual long Value => value;

            protected internal virtual LongHashtableEntry Clone()
            {
                return new LongHashtableEntry
                {
                    hash = hash,
                    key = key,
                    value = value,
                    next = ((next != null) ? next.Clone() : null)
                };
            }
        }

        public class LongHashtableIterator
        {
            private int index;

            private LongHashtableEntry[] table;

            private LongHashtableEntry entry;

            internal LongHashtableIterator(LongHashtableEntry[] table)
            {
                this.table = table;
                index = table.Length;
            }

            public virtual bool HasNext()
            {
                if (entry != null)
                {
                    return true;
                }

                while (index-- > 0)
                {
                    if ((entry = table[index]) != null)
                    {
                        return true;
                    }
                }

                return false;
            }

            public virtual LongHashtableEntry Next()
            {
                if (entry == null)
                {
                    while (index-- > 0 && (entry = table[index]) == null)
                    {
                    }
                }

                if (entry != null)
                {
                    LongHashtableEntry longHashtableEntry = entry;
                    entry = longHashtableEntry.next;
                    return longHashtableEntry;
                }

                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("inthashtableiterator"));
            }
        }

        private LongHashtableEntry[] table;

        private int count;

        private int threshold;

        private float loadFactor;

        public virtual int Size => count;

        public long this[long key]
        {
            get
            {
                LongHashtableEntry[] array = table;
                int hashCode = key.GetHashCode();
                int num = (hashCode & 0x7FFFFFFF) % array.Length;
                for (LongHashtableEntry longHashtableEntry = array[num]; longHashtableEntry != null; longHashtableEntry = longHashtableEntry.next)
                {
                    if (longHashtableEntry.hash == hashCode && longHashtableEntry.key == key)
                    {
                        return longHashtableEntry.value;
                    }
                }

                return 0L;
            }
            set
            {
                LongHashtableEntry[] array = table;
                int hashCode = key.GetHashCode();
                int num = (hashCode & 0x7FFFFFFF) % array.Length;
                for (LongHashtableEntry longHashtableEntry = array[num]; longHashtableEntry != null; longHashtableEntry = longHashtableEntry.next)
                {
                    if (longHashtableEntry.hash == hashCode && longHashtableEntry.key == key)
                    {
                        longHashtableEntry.value = value;
                        return;
                    }
                }

                if (count >= threshold)
                {
                    Rehash();
                    this[key] = value;
                    return;
                }

                LongHashtableEntry longHashtableEntry2 = new LongHashtableEntry();
                longHashtableEntry2.hash = hashCode;
                longHashtableEntry2.key = key;
                longHashtableEntry2.value = value;
                longHashtableEntry2.next = array[num];
                array[num] = longHashtableEntry2;
                count++;
            }
        }

        public LongHashtable(int initialCapacity, float loadFactor)
        {
            if (initialCapacity < 0)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.capacity.1", initialCapacity));
            }

            if (loadFactor <= 0f)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("illegal.load.1", loadFactor));
            }

            this.loadFactor = loadFactor;
            table = new LongHashtableEntry[initialCapacity];
            threshold = (int)((float)initialCapacity * loadFactor);
        }

        public LongHashtable(int initialCapacity)
            : this(initialCapacity, 0.75f)
        {
        }

        public LongHashtable()
            : this(101, 0.75f)
        {
        }

        public virtual bool IsEmpty()
        {
            return count == 0;
        }

        public virtual bool Contains(long value)
        {
            LongHashtableEntry[] array = table;
            int num = array.Length;
            while (num-- > 0)
            {
                for (LongHashtableEntry longHashtableEntry = array[num]; longHashtableEntry != null; longHashtableEntry = longHashtableEntry.next)
                {
                    if (longHashtableEntry.value == value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool ContainsKey(long key)
        {
            LongHashtableEntry[] array = table;
            int hashCode = key.GetHashCode();
            int num = (hashCode & 0x7FFFFFFF) % array.Length;
            for (LongHashtableEntry longHashtableEntry = array[num]; longHashtableEntry != null; longHashtableEntry = longHashtableEntry.next)
            {
                if (longHashtableEntry.hash == hashCode && longHashtableEntry.key == key)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void Rehash()
        {
            int num = table.Length;
            LongHashtableEntry[] array = table;
            int num2 = num * 2 + 1;
            LongHashtableEntry[] array2 = new LongHashtableEntry[num2];
            threshold = (int)((float)num2 * loadFactor);
            table = array2;
            int num3 = num;
            while (num3-- > 0)
            {
                LongHashtableEntry longHashtableEntry = array[num3];
                while (longHashtableEntry != null)
                {
                    LongHashtableEntry longHashtableEntry2 = longHashtableEntry;
                    longHashtableEntry = longHashtableEntry.next;
                    int num4 = (longHashtableEntry2.hash & 0x7FFFFFFF) % num2;
                    longHashtableEntry2.next = array2[num4];
                    array2[num4] = longHashtableEntry2;
                }
            }
        }

        public virtual long Remove(long key)
        {
            LongHashtableEntry[] array = table;
            int hashCode = key.GetHashCode();
            int num = (hashCode & 0x7FFFFFFF) % array.Length;
            LongHashtableEntry longHashtableEntry = array[num];
            LongHashtableEntry longHashtableEntry2 = null;
            while (longHashtableEntry != null)
            {
                if (longHashtableEntry.hash == hashCode && longHashtableEntry.key == key)
                {
                    if (longHashtableEntry2 != null)
                    {
                        longHashtableEntry2.next = longHashtableEntry.next;
                    }
                    else
                    {
                        array[num] = longHashtableEntry.next;
                    }

                    count--;
                    return longHashtableEntry.value;
                }

                longHashtableEntry2 = longHashtableEntry;
                longHashtableEntry = longHashtableEntry.next;
            }

            return 0L;
        }

        public virtual void Clear()
        {
            LongHashtableEntry[] array = table;
            int num = array.Length;
            while (--num >= 0)
            {
                array[num] = null;
            }

            count = 0;
        }

        public virtual LongHashtable Clone()
        {
            LongHashtable longHashtable = new LongHashtable();
            longHashtable.count = count;
            longHashtable.loadFactor = loadFactor;
            longHashtable.threshold = threshold;
            longHashtable.table = new LongHashtableEntry[table.Length];
            int num = table.Length;
            while (num-- > 0)
            {
                longHashtable.table[num] = ((table[num] != null) ? table[num].Clone() : null);
            }

            return longHashtable;
        }

        public virtual long[] ToOrderedKeys()
        {
            long[] keys = GetKeys();
            Array.Sort(keys);
            return keys;
        }

        public virtual long[] GetKeys()
        {
            long[] array = new long[count];
            int num = 0;
            int num2 = table.Length;
            LongHashtableEntry longHashtableEntry = null;
            while (true)
            {
                if (longHashtableEntry == null)
                {
                    while (num2-- > 0 && (longHashtableEntry = table[num2]) == null)
                    {
                    }
                }

                if (longHashtableEntry == null)
                {
                    break;
                }

                LongHashtableEntry longHashtableEntry2 = longHashtableEntry;
                longHashtableEntry = longHashtableEntry2.next;
                array[num++] = longHashtableEntry2.key;
            }

            return array;
        }

        public virtual LongHashtableIterator GetEntryIterator()
        {
            return new LongHashtableIterator(table);
        }
    }
}
