namespace Sign.itext.io
{
    internal class GroupedRandomAccessSource : IRandomAccessSource, IDisposable
    {
        private sealed class SourceEntry
        {
            internal readonly IRandomAccessSource source;

            internal readonly long firstByte;

            internal readonly long lastByte;

            internal readonly int index;

            internal SourceEntry(int index, IRandomAccessSource source, long offset)
            {
                this.index = index;
                this.source = source;
                firstByte = offset;
                lastByte = offset + source.Length - 1;
            }

            internal long OffsetN(long absoluteOffset)
            {
                return absoluteOffset - firstByte;
            }
        }

        private readonly SourceEntry[] sources;

        private SourceEntry currentSourceEntry;

        private readonly long size;

        public virtual long Length => size;

        public GroupedRandomAccessSource(ICollection<IRandomAccessSource> sources)
        {
            this.sources = new SourceEntry[sources.Count];
            long num = 0L;
            int num2 = 0;
            foreach (IRandomAccessSource source in sources)
            {
                this.sources[num2] = new SourceEntry(num2, source, num);
                num2++;
                num += source.Length;
            }

            size = num;
            currentSourceEntry = this.sources[sources.Count - 1];
            SourceInUse(currentSourceEntry.source);
        }

        protected internal virtual int GetStartingSourceIndex(long offset)
        {
            if (offset >= currentSourceEntry.firstByte)
            {
                return currentSourceEntry.index;
            }

            return 0;
        }

        private SourceEntry GetSourceEntryForOffset(long offset)
        {
            if (offset >= size)
            {
                return null;
            }

            if (offset >= currentSourceEntry.firstByte && offset <= currentSourceEntry.lastByte)
            {
                return currentSourceEntry;
            }

            SourceReleased(currentSourceEntry.source);
            for (int i = GetStartingSourceIndex(offset); i < sources.Length; i++)
            {
                if (offset >= sources[i].firstByte && offset <= sources[i].lastByte)
                {
                    currentSourceEntry = sources[i];
                    SourceInUse(currentSourceEntry.source);
                    return currentSourceEntry;
                }
            }

            return null;
        }

        protected internal virtual void SourceReleased(IRandomAccessSource source)
        {
        }

        protected internal virtual void SourceInUse(IRandomAccessSource source)
        {
        }

        public virtual int Get(long position)
        {
            SourceEntry sourceEntryForOffset = GetSourceEntryForOffset(position);
            return sourceEntryForOffset?.source.Get(sourceEntryForOffset.OffsetN(position)) ?? (-1);
        }

        public virtual int Get(long position, byte[] bytes, int off, int len)
        {
            SourceEntry sourceEntryForOffset = GetSourceEntryForOffset(position);
            if (sourceEntryForOffset == null)
            {
                return -1;
            }

            long num = sourceEntryForOffset.OffsetN(position);
            int num2 = len;
            while (num2 > 0 && sourceEntryForOffset != null && num <= sourceEntryForOffset.source.Length)
            {
                int num3 = sourceEntryForOffset.source.Get(num, bytes, off, num2);
                if (num3 == -1)
                {
                    break;
                }

                off += num3;
                position += num3;
                num2 -= num3;
                num = 0L;
                sourceEntryForOffset = GetSourceEntryForOffset(position);
            }

            if (num2 != len)
            {
                return len - num2;
            }

            return -1;
        }

        public virtual void Close()
        {
            SourceEntry[] array = sources;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].source.Close();
            }
        }

        public virtual void Dispose()
        {
            Close();
        }
    }
}
