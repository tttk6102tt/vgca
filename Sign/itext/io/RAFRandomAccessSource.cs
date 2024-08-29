namespace Sign.itext.io
{
    internal class RAFRandomAccessSource : IRandomAccessSource, IDisposable
    {
        private readonly FileStream raf;

        private readonly long length;

        public virtual long Length => length;

        public RAFRandomAccessSource(FileStream raf)
        {
            this.raf = raf;
            length = raf.Length;
        }

        public virtual int Get(long position)
        {
            if (position > length)
            {
                return -1;
            }

            if (raf.Position != position)
            {
                raf.Seek(position, SeekOrigin.Begin);
            }

            return raf.ReadByte();
        }

        public virtual int Get(long position, byte[] bytes, int off, int len)
        {
            if (position > length)
            {
                return -1;
            }

            if (raf.Position != position)
            {
                raf.Seek(position, SeekOrigin.Begin);
            }

            int num = raf.Read(bytes, off, len);
            if (num != 0)
            {
                return num;
            }

            return -1;
        }

        public virtual void Close()
        {
            raf.Close();
        }

        public virtual void Dispose()
        {
            Close();
        }
    }
}
