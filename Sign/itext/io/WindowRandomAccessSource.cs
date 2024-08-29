namespace Sign.itext.io
{
    public class WindowRandomAccessSource : IRandomAccessSource, IDisposable
    {
        private readonly IRandomAccessSource source;

        private readonly long offset;

        private readonly long length;

        public virtual long Length => length;

        public WindowRandomAccessSource(IRandomAccessSource source, long offset)
            : this(source, offset, source.Length - offset)
        {
        }

        public WindowRandomAccessSource(IRandomAccessSource source, long offset, long length)
        {
            this.source = source;
            this.offset = offset;
            this.length = length;
        }

        public virtual int Get(long position)
        {
            if (position >= length)
            {
                return -1;
            }

            return source.Get(offset + position);
        }

        public virtual int Get(long position, byte[] bytes, int off, int len)
        {
            if (position >= length)
            {
                return -1;
            }

            long num = Math.Min(len, length - position);
            return source.Get(offset + position, bytes, off, (int)num);
        }

        public virtual void Close()
        {
            source.Close();
        }

        public virtual void Dispose()
        {
            Close();
        }
    }
}
