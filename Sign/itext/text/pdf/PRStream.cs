using Sign.itext.pdf;
using Sign.SystemItext.util.zlib;

namespace Sign.itext.text.pdf
{
    public class PRStream : PdfStream
    {
        protected PdfReader reader;

        protected long offset;

        protected int length;

        protected int objNum;

        protected int objGen;

        public new int Length
        {
            get
            {
                return length;
            }
            set
            {
                length = value;
                Put(PdfName.LENGTH, new PdfNumber(length));
            }
        }

        public virtual long Offset => offset;

        public virtual PdfReader Reader => reader;

        public virtual int ObjNum
        {
            get
            {
                return objNum;
            }
            set
            {
                objNum = value;
            }
        }

        public virtual int ObjGen
        {
            get
            {
                return objGen;
            }
            set
            {
                objGen = value;
            }
        }

        public PRStream(PRStream stream, PdfDictionary newDic)
        {
            reader = stream.reader;
            offset = stream.offset;
            length = stream.Length;
            compressed = stream.compressed;
            compressionLevel = stream.compressionLevel;
            streamBytes = stream.streamBytes;
            bytes = stream.bytes;
            objNum = stream.objNum;
            objGen = stream.objGen;
            if (newDic != null)
            {
                Merge(newDic);
            }
            else
            {
                Merge(stream);
            }
        }

        public PRStream(PRStream stream, PdfDictionary newDic, PdfReader reader)
            : this(stream, newDic)
        {
            this.reader = reader;
        }

        public PRStream(PdfReader reader, long offset)
        {
            this.reader = reader;
            this.offset = offset;
        }

        public PRStream(PdfReader reader, byte[] conts)
            : this(reader, conts, -1)
        {
        }

        public PRStream(PdfReader reader, byte[] conts, int compressionLevel)
        {
            this.reader = reader;
            offset = -1L;
            if (Document.Compress)
            {
                MemoryStream memoryStream = new MemoryStream();
                ZDeflaterOutputStream zDeflaterOutputStream = new ZDeflaterOutputStream(memoryStream, compressionLevel);
                zDeflaterOutputStream.Write(conts, 0, conts.Length);
                zDeflaterOutputStream.Close();
                bytes = memoryStream.ToArray();
                Put(PdfName.FILTER, PdfName.FLATEDECODE);
            }
            else
            {
                bytes = conts;
            }

            Length = bytes.Length;
        }

        public virtual void SetData(byte[] data, bool compress)
        {
            SetData(data, compress, -1);
        }

        public virtual void SetData(byte[] data, bool compress, int compressionLevel)
        {
            Remove(PdfName.FILTER);
            offset = -1L;
            if (Document.Compress && compress)
            {
                MemoryStream memoryStream = new MemoryStream();
                ZDeflaterOutputStream zDeflaterOutputStream = new ZDeflaterOutputStream(memoryStream, compressionLevel);
                zDeflaterOutputStream.Write(data, 0, data.Length);
                zDeflaterOutputStream.Close();
                bytes = memoryStream.ToArray();
                base.compressionLevel = compressionLevel;
                Put(PdfName.FILTER, PdfName.FLATEDECODE);
            }
            else
            {
                bytes = data;
            }

            Length = bytes.Length;
        }

        public virtual void SetDataRaw(byte[] data)
        {
            offset = -1L;
            bytes = data;
            Length = bytes.Length;
        }

        public virtual void SetData(byte[] data)
        {
            SetData(data, compress: true);
        }

        public new byte[] GetBytes()
        {
            return bytes;
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            byte[] array = PdfReader.GetStreamBytesRaw(this);
            PdfEncryption pdfEncryption = null;
            if (writer != null)
            {
                pdfEncryption = writer.Encryption;
            }

            PdfObject value = Get(PdfName.LENGTH);
            int num = array.Length;
            if (pdfEncryption != null)
            {
                num = pdfEncryption.CalculateStreamSize(num);
            }

            Put(PdfName.LENGTH, new PdfNumber(num));
            SuperToPdf(writer, os);
            Put(PdfName.LENGTH, value);
            os.Write(PdfStream.STARTSTREAM, 0, PdfStream.STARTSTREAM.Length);
            if (length > 0)
            {
                if (pdfEncryption != null && !pdfEncryption.IsEmbeddedFilesOnly())
                {
                    array = pdfEncryption.EncryptByteArray(array);
                }

                os.Write(array, 0, array.Length);
            }

            os.Write(PdfStream.ENDSTREAM, 0, PdfStream.ENDSTREAM.Length);
        }
    }
}
