using Sign.itext.error_messages;
using Sign.itext.text;
using Sign.itext.text.pdf;
using Sign.SystemItext.util.zlib;

namespace Sign.itext.pdf
{
    public class PdfStream : PdfDictionary
    {
        public const int DEFAULT_COMPRESSION = -1;

        public const int NO_COMPRESSION = 0;

        public const int BEST_SPEED = 1;

        public const int BEST_COMPRESSION = 9;

        protected bool compressed;

        protected int compressionLevel;

        protected MemoryStream streamBytes;

        protected Stream inputStream;

        protected PdfIndirectReference iref;

        protected int inputStreamLength = -1;

        protected PdfWriter writer;

        protected int rawLength;

        internal static byte[] STARTSTREAM = DocWriter.GetISOBytes("stream\n");

        internal static byte[] ENDSTREAM = DocWriter.GetISOBytes("\nendstream");

        internal static int SIZESTREAM = STARTSTREAM.Length + ENDSTREAM.Length;

        public virtual int RawLength => rawLength;

        public PdfStream(byte[] bytes)
        {
            type = 7;
            base.bytes = bytes;
            rawLength = bytes.Length;
            Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
        }

        public PdfStream(Stream inputStream, PdfWriter writer)
        {
            type = 7;
            this.inputStream = inputStream;
            this.writer = writer;
            iref = writer.PdfIndirectReference;
            Put(PdfName.LENGTH, iref);
        }

        protected PdfStream()
        {
            type = 7;
        }

        public virtual void WriteLength()
        {
            if (inputStream == null)
            {
                throw new PdfException(MessageLocalization.GetComposedMessage("writelength.can.only.be.called.in.a.contructed.pdfstream.inputstream.pdfwriter"));
            }

            if (inputStreamLength == -1)
            {
                throw new PdfException(MessageLocalization.GetComposedMessage("writelength.can.only.be.called.after.output.of.the.stream.body"));
            }

            writer.AddToBody(new PdfNumber(inputStreamLength), iref, inObjStm: false);
        }

        public virtual void FlateCompress()
        {
            FlateCompress(-1);
        }

        public virtual void FlateCompress(int compressionLevel)
        {
            if (!Document.Compress || compressed)
            {
                return;
            }

            this.compressionLevel = compressionLevel;
            if (inputStream != null)
            {
                compressed = true;
                return;
            }

            PdfObject pdfObject = PdfReader.GetPdfObject(Get(PdfName.FILTER));
            if (pdfObject != null)
            {
                if (pdfObject.IsName())
                {
                    if (PdfName.FLATEDECODE.Equals(pdfObject))
                    {
                        return;
                    }
                }
                else
                {
                    if (!pdfObject.IsArray())
                    {
                        throw new PdfException(MessageLocalization.GetComposedMessage("stream.could.not.be.compressed.filter.is.not.a.name.or.array"));
                    }

                    if (((PdfArray)pdfObject).Contains(PdfName.FLATEDECODE))
                    {
                        return;
                    }
                }
            }

            MemoryStream outp = new MemoryStream();
            ZDeflaterOutputStream zDeflaterOutputStream = new ZDeflaterOutputStream(outp, compressionLevel);
            if (streamBytes != null)
            {
                streamBytes.WriteTo(zDeflaterOutputStream);
            }
            else
            {
                zDeflaterOutputStream.Write(bytes, 0, bytes.Length);
            }

            zDeflaterOutputStream.Finish();
            streamBytes = outp;
            bytes = null;
            Put(PdfName.LENGTH, new PdfNumber(streamBytes.Length));
            if (pdfObject == null)
            {
                Put(PdfName.FILTER, PdfName.FLATEDECODE);
            }
            else
            {
                PdfArray pdfArray = new PdfArray(pdfObject);
                pdfArray.Add(0, PdfName.FLATEDECODE);
                Put(PdfName.FILTER, pdfArray);
            }

            compressed = true;
        }

        protected virtual void SuperToPdf(PdfWriter writer, Stream os)
        {
            base.ToPdf(writer, os);
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            if (inputStream != null && compressed)
            {
                Put(PdfName.FILTER, PdfName.FLATEDECODE);
            }

            PdfEncryption pdfEncryption = null;
            if (writer != null)
            {
                pdfEncryption = writer.Encryption;
            }

            if (pdfEncryption != null)
            {
                PdfObject pdfObject = Get(PdfName.FILTER);
                if (pdfObject != null)
                {
                    if (PdfName.CRYPT.Equals(pdfObject))
                    {
                        pdfEncryption = null;
                    }
                    else if (pdfObject.IsArray())
                    {
                        PdfArray pdfArray = (PdfArray)pdfObject;
                        if (pdfArray.Size > 0 && PdfName.CRYPT.Equals(pdfArray[0]))
                        {
                            pdfEncryption = null;
                        }
                    }
                }
            }

            PdfObject pdfObject2 = Get(PdfName.LENGTH);
            if (pdfEncryption != null && pdfObject2 != null && pdfObject2.IsNumber())
            {
                int intValue = ((PdfNumber)pdfObject2).IntValue;
                Put(PdfName.LENGTH, new PdfNumber(pdfEncryption.CalculateStreamSize(intValue)));
                SuperToPdf(writer, os);
                Put(PdfName.LENGTH, pdfObject2);
            }
            else
            {
                SuperToPdf(writer, os);
            }

            PdfWriter.CheckPdfIsoConformance(writer, 9, this);
            os.Write(STARTSTREAM, 0, STARTSTREAM.Length);
            if (inputStream != null)
            {
                rawLength = 0;
                ZDeflaterOutputStream zDeflaterOutputStream = null;
                OutputStreamCounter outputStreamCounter = new OutputStreamCounter(os);
                OutputStreamEncryption outputStreamEncryption = null;
                Stream stream = outputStreamCounter;
                if (pdfEncryption != null && !pdfEncryption.IsEmbeddedFilesOnly())
                {
                    stream = (outputStreamEncryption = pdfEncryption.GetEncryptionStream(stream));
                }

                if (compressed)
                {
                    stream = (zDeflaterOutputStream = new ZDeflaterOutputStream(stream, compressionLevel));
                }

                byte[] array = new byte[4192];
                while (true)
                {
                    int num = inputStream.Read(array, 0, array.Length);
                    if (num <= 0)
                    {
                        break;
                    }

                    stream.Write(array, 0, num);
                    rawLength += num;
                }

                zDeflaterOutputStream?.Finish();
                outputStreamEncryption?.Finish();
                inputStreamLength = (int)outputStreamCounter.Counter;
            }
            else if (pdfEncryption != null && !pdfEncryption.IsEmbeddedFilesOnly())
            {
                byte[] array2 = ((streamBytes == null) ? pdfEncryption.EncryptByteArray(bytes) : pdfEncryption.EncryptByteArray(streamBytes.ToArray()));
                os.Write(array2, 0, array2.Length);
            }
            else if (streamBytes != null)
            {
                streamBytes.WriteTo(os);
            }
            else
            {
                os.Write(bytes, 0, bytes.Length);
            }

            os.Write(ENDSTREAM, 0, ENDSTREAM.Length);
        }

        public virtual void WriteContent(Stream os)
        {
            if (streamBytes != null)
            {
                streamBytes.WriteTo(os);
            }
            else if (bytes != null)
            {
                os.Write(bytes, 0, bytes.Length);
            }
        }

        public override string ToString()
        {
            if (Get(PdfName.TYPE) == null)
            {
                return "Stream";
            }

            return "Stream of type: " + Get(PdfName.TYPE);
        }
    }
}
