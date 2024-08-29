using Sign.itext.error_messages;

namespace Sign.itext.pdf
{
    public class IntHashtable
    {
        public class IntHashtableEntry
        {
            internal int hash;

            internal int key;

            internal int value;

            internal IntHashtableEntry next;

            public virtual int Key => key;

            public virtual int Value => value;

            protected internal virtual IntHashtableEntry Clone()
            {
                return new IntHashtableEntry
                {
                    hash = hash,
                    key = key,
                    value = value,
                    next = ((next != null) ? next.Clone() : null)
                };
            }
        }

        public class IntHashtableIterator
        {
            private int index;

            private IntHashtableEntry[] table;

            private IntHashtableEntry entry;

            internal IntHashtableIterator(IntHashtableEntry[] table)
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

            public virtual IntHashtableEntry Next()
            {
                if (entry == null)
                {
                    while (index-- > 0 && (entry = table[index]) == null)
                    {
                    }
                }

                if (entry != null)
                {
                    IntHashtableEntry intHashtableEntry = entry;
                    entry = intHashtableEntry.next;
                    return intHashtableEntry;
                }

                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("inthashtableiterator"));
            }
        }

        private IntHashtableEntry[] table;

        private int count;

        private int threshold;

        private float loadFactor;

        public virtual int Size => count;

        public int this[int key]
        {
            get
            {
                IntHashtableEntry[] array = table;
                int num = (key & 0x7FFFFFFF) % array.Length;
                for (IntHashtableEntry intHashtableEntry = array[num]; intHashtableEntry != null; intHashtableEntry = intHashtableEntry.next)
                {
                    if (intHashtableEntry.hash == key && intHashtableEntry.key == key)
                    {
                        return intHashtableEntry.value;
                    }
                }

                return 0;
            }
            set
            {
                IntHashtableEntry[] array = table;
                int num = (key & 0x7FFFFFFF) % array.Length;
                for (IntHashtableEntry intHashtableEntry = array[num]; intHashtableEntry != null; intHashtableEntry = intHashtableEntry.next)
                {
                    if (intHashtableEntry.hash == key && intHashtableEntry.key == key)
                    {
                        intHashtableEntry.value = value;
                        return;
                    }
                }

                if (count >= threshold)
                {
                    Rehash();
                    this[key] = value;
                    return;
                }

                IntHashtableEntry intHashtableEntry2 = new IntHashtableEntry();
                intHashtableEntry2.hash = key;
                intHashtableEntry2.key = key;
                intHashtableEntry2.value = value;
                intHashtableEntry2.next = array[num];
                array[num] = intHashtableEntry2;
                count++;
            }
        }

        public IntHashtable(int initialCapacity, float loadFactor)
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
            table = new IntHashtableEntry[initialCapacity];
            threshold = (int)((float)initialCapacity * loadFactor);
        }

        public IntHashtable(int initialCapacity)
            : this(initialCapacity, 0.75f)
        {
        }

        public IntHashtable()
            : this(101, 0.75f)
        {
        }

        public virtual bool IsEmpty()
        {
            return count == 0;
        }

        public virtual bool Contains(int value)
        {
            IntHashtableEntry[] array = table;
            int num = array.Length;
            while (num-- > 0)
            {
                for (IntHashtableEntry intHashtableEntry = array[num]; intHashtableEntry != null; intHashtableEntry = intHashtableEntry.next)
                {
                    if (intHashtableEntry.value == value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool ContainsKey(int key)
        {
            IntHashtableEntry[] array = table;
            int num = (key & 0x7FFFFFFF) % array.Length;
            for (IntHashtableEntry intHashtableEntry = array[num]; intHashtableEntry != null; intHashtableEntry = intHashtableEntry.next)
            {
                if (intHashtableEntry.hash == key && intHashtableEntry.key == key)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void Rehash()
        {
            int num = table.Length;
            IntHashtableEntry[] array = table;
            int num2 = num * 2 + 1;
            IntHashtableEntry[] array2 = new IntHashtableEntry[num2];
            threshold = (int)((float)num2 * loadFactor);
            table = array2;
            int num3 = num;
            while (num3-- > 0)
            {
                IntHashtableEntry intHashtableEntry = array[num3];
                while (intHashtableEntry != null)
                {
                    IntHashtableEntry intHashtableEntry2 = intHashtableEntry;
                    intHashtableEntry = intHashtableEntry.next;
                    int num4 = (intHashtableEntry2.hash & 0x7FFFFFFF) % num2;
                    intHashtableEntry2.next = array2[num4];
                    array2[num4] = intHashtableEntry2;
                }
            }
        }

        public virtual int Remove(int key)
        {
            IntHashtableEntry[] array = table;
            int num = (key & 0x7FFFFFFF) % array.Length;
            IntHashtableEntry intHashtableEntry = array[num];
            IntHashtableEntry intHashtableEntry2 = null;
            while (intHashtableEntry != null)
            {
                if (intHashtableEntry.hash == key && intHashtableEntry.key == key)
                {
                    if (intHashtableEntry2 != null)
                    {
                        intHashtableEntry2.next = intHashtableEntry.next;
                    }
                    else
                    {
                        array[num] = intHashtableEntry.next;
                    }

                    count--;
                    return intHashtableEntry.value;
                }

                intHashtableEntry2 = intHashtableEntry;
                intHashtableEntry = intHashtableEntry.next;
            }

            return 0;
        }

        public virtual void Clear()
        {
            IntHashtableEntry[] array = table;
            int num = array.Length;
            while (--num >= 0)
            {
                array[num] = null;
            }

            count = 0;
        }

        public virtual IntHashtable Clone()
        {
            IntHashtable intHashtable = new IntHashtable();
            intHashtable.count = count;
            intHashtable.loadFactor = loadFactor;
            intHashtable.threshold = threshold;
            intHashtable.table = new IntHashtableEntry[table.Length];
            int num = table.Length;
            while (num-- > 0)
            {
                intHashtable.table[num] = ((table[num] != null) ? table[num].Clone() : null);
            }

            return intHashtable;
        }

        public virtual int[] ToOrderedKeys()
        {
            int[] keys = GetKeys();
            Array.Sort(keys);
            return keys;
        }

        public virtual int[] GetKeys()
        {
            int[] array = new int[count];
            int num = 0;
            int num2 = table.Length;
            IntHashtableEntry intHashtableEntry = null;
            while (true)
            {
                if (intHashtableEntry == null)
                {
                    while (num2-- > 0 && (intHashtableEntry = table[num2]) == null)
                    {
                    }
                }

                if (intHashtableEntry == null)
                {
                    break;
                }

                IntHashtableEntry intHashtableEntry2 = intHashtableEntry;
                intHashtableEntry = intHashtableEntry2.next;
                array[num++] = intHashtableEntry2.key;
            }

            return array;
        }

        public virtual IntHashtableIterator GetEntryIterator()
        {
            return new IntHashtableIterator(table);
        }
    }
}
