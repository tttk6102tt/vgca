using Sign.itext.pdf.crypto;
using Sign.itext.text.pdf.crypto;

namespace Sign.itext.text.pdf
{
    public class OutputStreamEncryption : Stream
    {
        protected Stream outc;

        protected ARCFOUREncryption arcfour;

        protected AESCipher cipher;

        private byte[] buf = new byte[1];

        private const int AES_128 = 4;

        private const int AES_256 = 5;

        private bool aes;

        private bool finished;

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

        public OutputStreamEncryption(Stream outc, byte[] key, int off, int len, int revision)
        {
            this.outc = outc;
            aes = revision == 4 || revision == 5;
            if (aes)
            {
                byte[] iV = IVGenerator.GetIV();
                byte[] array = new byte[len];
                Array.Copy(key, off, array, 0, len);
                cipher = new AESCipher(forEncryption: true, array, iV);
                Write(iV, 0, iV.Length);
            }
            else
            {
                arcfour = new ARCFOUREncryption();
                arcfour.PrepareARCFOURKey(key, off, len);
            }
        }

        public OutputStreamEncryption(Stream outc, byte[] key, int revision)
            : this(outc, key, 0, key.Length, revision)
        {
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

        public override void Write(byte[] b, int off, int len)
        {
            if (aes)
            {
                byte[] array = cipher.Update(b, off, len);
                if (array != null && array.Length != 0)
                {
                    outc.Write(array, 0, array.Length);
                }

                return;
            }

            byte[] array2 = new byte[Math.Min(len, 4192)];
            while (len > 0)
            {
                int num = Math.Min(len, array2.Length);
                arcfour.EncryptARCFOUR(b, off, num, array2, 0);
                outc.Write(array2, 0, num);
                len -= num;
                off += num;
            }
        }

        public override void Close()
        {
            Finish();
            outc.Close();
        }

        public override void WriteByte(byte value)
        {
            buf[0] = value;
            Write(buf, 0, 1);
        }

        public virtual void Finish()
        {
            if (!finished)
            {
                finished = true;
                if (aes)
                {
                    byte[] array = cipher.DoFinal();
                    outc.Write(array, 0, array.Length);
                }
            }
        }
    }
}
