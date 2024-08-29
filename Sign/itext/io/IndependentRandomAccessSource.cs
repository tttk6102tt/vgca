namespace Sign.itext.io
{
    public class IndependentRandomAccessSource : IRandomAccessSource, IDisposable
    {
        private readonly IRandomAccessSource source;

        public virtual long Length => source.Length;

        public IndependentRandomAccessSource(IRandomAccessSource source)
        {
            this.source = source;
        }

        public virtual int Get(long position)
        {
            return source.Get(position);
        }

        public virtual int Get(long position, byte[] bytes, int off, int len)
        {
            return source.Get(position, bytes, off, len);
        }

        public virtual void Close()
        {
        }

        public virtual void Dispose()
        {
            Close();
        }
    }
}
