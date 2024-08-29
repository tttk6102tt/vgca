using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public abstract class PdfObject : IComparable<PdfObject>
    {
        public const int BOOLEAN = 1;

        public const int NUMBER = 2;

        public const int STRING = 3;

        public const int NAME = 4;

        public const int ARRAY = 5;

        public const int DICTIONARY = 6;

        public const int STREAM = 7;

        public const int NULL = 8;

        public const int INDIRECT = 10;

        public const string NOTHING = "";

        public const string TEXT_PDFDOCENCODING = "PDF";

        public const string TEXT_UNICODE = "UnicodeBig";

        protected byte[] bytes;

        protected int type;

        protected PRIndirectReference indRef;

        private readonly int hashCode = IncrementObjCounter();

        private static int objCounter = 0;

        private static readonly object locker = new object();

        public virtual int Length => ToString().Length;

        protected virtual string Content
        {
            set
            {
                bytes = PdfEncodings.ConvertToBytes(value, null);
            }
        }

        public virtual int Type => type;

        public virtual PRIndirectReference IndRef
        {
            get
            {
                return indRef;
            }
            set
            {
                indRef = value;
            }
        }

        protected PdfObject(int type)
        {
            this.type = type;
        }

        protected PdfObject(int type, string content)
        {
            this.type = type;
            bytes = PdfEncodings.ConvertToBytes(content, null);
        }

        protected PdfObject(int type, byte[] bytes)
        {
            this.bytes = bytes;
            this.type = type;
        }

        public virtual void ToPdf(PdfWriter writer, Stream os)
        {
            if (bytes != null)
            {
                PdfWriter.CheckPdfIsoConformance(writer, 11, this);
                os.Write(bytes, 0, bytes.Length);
            }
        }

        public virtual byte[] GetBytes()
        {
            return bytes;
        }

        public virtual bool CanBeInObjStm()
        {
            switch (type)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 8:
                    return true;
                default:
                    return false;
            }
        }

        public override string ToString()
        {
            if (bytes == null)
            {
                return "";
            }

            return PdfEncodings.ConvertToString(bytes, null);
        }

        public virtual bool IsNull()
        {
            return type == 8;
        }

        public virtual bool IsBoolean()
        {
            return type == 1;
        }

        public virtual bool IsNumber()
        {
            return type == 2;
        }

        public virtual bool IsString()
        {
            return type == 3;
        }

        public virtual bool IsName()
        {
            return type == 4;
        }

        public virtual bool IsArray()
        {
            return type == 5;
        }

        public virtual bool IsDictionary()
        {
            return type == 6;
        }

        public virtual bool IsStream()
        {
            return type == 7;
        }

        public virtual bool IsIndirect()
        {
            return type == 10;
        }

        public virtual int CompareTo(PdfObject obj)
        {
            return GetHashCode().CompareTo(obj.GetHashCode());
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            PdfObject pdfObject = obj as PdfObject;
            if (pdfObject == null)
            {
                return false;
            }

            return CompareTo(pdfObject) == 0;
        }

        private static int IncrementObjCounter()
        {
            lock (locker)
            {
                objCounter++;
                return objCounter;
            }
        }
    }
}
