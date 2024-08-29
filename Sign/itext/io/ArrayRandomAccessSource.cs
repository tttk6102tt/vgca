namespace Sign.itext.io
{
    internal class ArrayRandomAccessSource : IRandomAccessSource, IDisposable
    {
        private byte[] array;

        public virtual long Length => array.Length;

        public ArrayRandomAccessSource(byte[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            this.array = array;
        }

        public virtual int Get(long offset)
        {
            if (offset >= array.Length)
            {
                return -1;
            }

            return 0xFF & array[(int)offset];
        }

        public virtual int Get(long offset, byte[] bytes, int off, int len)
        {
            if (array == null)
            {
                throw new InvalidOperationException("Already closed");
            }

            if (offset >= array.Length)
            {
                return -1;
            }

            if (offset + len > array.Length)
            {
                len = (int)(array.Length - offset);
            }

            Array.Copy(array, (int)offset, bytes, off, len);
            return len;
        }

        public virtual void Close()
        {
            array = null;
        }

        public virtual void Dispose()
        {
            Close();
        }
    }
}
