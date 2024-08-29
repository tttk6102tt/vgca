namespace Sign.itext.xml.xmp.impl
{
    public sealed class CountOutputStream : Stream
    {
        private readonly Stream _outp;

        private int _bytesWritten;

        public int BytesWritten => _bytesWritten;

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => BytesWritten;

        public override long Position
        {
            get
            {
                return Length;
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        internal CountOutputStream(Stream outp)
        {
            _outp = outp;
        }

        public override void Write(byte[] buf, int off, int len)
        {
            _outp.Write(buf, off, len);
            _bytesWritten += len;
        }

        public void Write(byte[] buf)
        {
            Write(buf, 0, buf.Length);
        }

        public void Write(int b)
        {
            _outp.WriteByte((byte)b);
            _bytesWritten++;
        }

        public override void Flush()
        {
            _outp.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetLength(long value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
