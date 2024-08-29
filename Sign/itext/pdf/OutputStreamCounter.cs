namespace Sign.itext.pdf
{
    public class OutputStreamCounter : Stream
    {
        protected Stream outc;

        protected long counter;

        public virtual long Counter => counter;

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public OutputStreamCounter(Stream _outc)
        {
            outc = _outc;
        }

        public virtual void ResetCounter()
        {
            counter = 0L;
        }

        public override void Flush()
        {
            outc.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            counter += count;
            outc.Write(buffer, offset, count);
        }

        public override void Close()
        {
            outc.Close();
        }
    }
}
