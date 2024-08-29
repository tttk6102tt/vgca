
using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class PdfString : PdfObject
    {
        protected string value = "";

        protected string originalValue;

        protected string encoding = "PDF";

        protected int objNum;

        protected int objGen;

        protected bool hexWriting;

        public virtual string Encoding => encoding;

        public PdfString()
            : base(3)
        {
        }

        public PdfString(string value)
            : base(3)
        {
            this.value = value;
        }

        public PdfString(string value, string encoding)
            : base(3)
        {
            this.value = value;
            this.encoding = encoding;
        }

        public PdfString(byte[] bytes)
            : base(3)
        {
            value = PdfEncodings.ConvertToString(bytes, null);
            encoding = "";
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            PdfWriter.CheckPdfIsoConformance(writer, 11, this);
            byte[] array = GetBytes();
            PdfEncryption pdfEncryption = null;
            if (writer != null)
            {
                pdfEncryption = writer.Encryption;
            }

            if (pdfEncryption != null && !pdfEncryption.IsEmbeddedFilesOnly())
            {
                array = pdfEncryption.EncryptByteArray(array);
            }

            if (hexWriting)
            {
                ByteBuffer byteBuffer = new ByteBuffer();
                byteBuffer.Append('<');
                int num = array.Length;
                for (int i = 0; i < num; i++)
                {
                    byteBuffer.AppendHex(array[i]);
                }

                byteBuffer.Append('>');
                os.Write(byteBuffer.ToByteArray(), 0, byteBuffer.Size);
            }
            else
            {
                array = StringUtils.EscapeString(array);
                os.Write(array, 0, array.Length);
            }
        }

        public override string ToString()
        {
            return value;
        }

        public virtual string ToUnicodeString()
        {
            if (encoding != null && encoding.Length != 0)
            {
                return value;
            }

            GetBytes();
            if (bytes.Length >= 2 && bytes[0] == 254 && bytes[1] == byte.MaxValue)
            {
                return PdfEncodings.ConvertToString(bytes, "UnicodeBig");
            }

            return PdfEncodings.ConvertToString(bytes, "PDF");
        }

        internal void SetObjNum(int objNum, int objGen)
        {
            this.objNum = objNum;
            this.objGen = objGen;
        }

        internal void Decrypt(PdfReader reader)
        {
            PdfEncryption decrypt = reader.Decrypt;
            if (decrypt != null)
            {
                originalValue = value;
                decrypt.SetHashKey(objNum, objGen);
                bytes = PdfEncodings.ConvertToBytes(value, null);
                bytes = decrypt.DecryptByteArray(bytes);
                value = PdfEncodings.ConvertToString(bytes, null);
            }
        }

        public override byte[] GetBytes()
        {
            if (bytes == null)
            {
                if (encoding != null && encoding.Equals("UnicodeBig") && PdfEncodings.IsPdfDocEncoding(value))
                {
                    bytes = PdfEncodings.ConvertToBytes(value, "PDF");
                }
                else
                {
                    bytes = PdfEncodings.ConvertToBytes(value, encoding);
                }
            }

            return bytes;
        }

        public virtual byte[] GetOriginalBytes()
        {
            if (originalValue == null)
            {
                return GetBytes();
            }

            return PdfEncodings.ConvertToBytes(originalValue, null);
        }

        public virtual PdfString SetHexWriting(bool hexWriting)
        {
            this.hexWriting = hexWriting;
            return this;
        }

        public virtual bool IsHexWriting()
        {
            return hexWriting;
        }
    }
}
