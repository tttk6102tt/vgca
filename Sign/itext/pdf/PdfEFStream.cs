using Sign.itext.text.pdf;
using Sign.SystemItext.util.zlib;

namespace Sign.itext.pdf
{
    public class PdfEFStream : PdfStream
    {
        public PdfEFStream(Stream inp, PdfWriter writer)
            : base(inp, writer)
        {
        }

        public PdfEFStream(byte[] fileStore)
            : base(fileStore)
        {
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
                        if (!pdfArray.IsEmpty() && PdfName.CRYPT.Equals(pdfArray[0]))
                        {
                            pdfEncryption = null;
                        }
                    }
                }
            }

            if (pdfEncryption != null && pdfEncryption.IsEmbeddedFilesOnly())
            {
                PdfArray pdfArray2 = new PdfArray();
                PdfArray pdfArray3 = new PdfArray();
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.NAME, PdfName.STDCF);
                pdfArray2.Add(PdfName.CRYPT);
                pdfArray3.Add(pdfDictionary);
                if (compressed)
                {
                    pdfArray2.Add(PdfName.FLATEDECODE);
                    pdfArray3.Add(new PdfNull());
                }

                Put(PdfName.FILTER, pdfArray2);
                Put(PdfName.DECODEPARMS, pdfArray3);
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

            os.Write(PdfStream.STARTSTREAM, 0, PdfStream.STARTSTREAM.Length);
            if (inputStream != null)
            {
                rawLength = 0;
                ZDeflaterOutputStream zDeflaterOutputStream = null;
                OutputStreamCounter outputStreamCounter = new OutputStreamCounter(os);
                OutputStreamEncryption outputStreamEncryption = null;
                Stream stream = outputStreamCounter;
                if (pdfEncryption != null)
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
            else if (pdfEncryption == null)
            {
                if (streamBytes != null)
                {
                    streamBytes.WriteTo(os);
                }
                else
                {
                    os.Write(bytes, 0, bytes.Length);
                }
            }
            else
            {
                byte[] array2 = ((streamBytes == null) ? pdfEncryption.EncryptByteArray(bytes) : pdfEncryption.EncryptByteArray(streamBytes.ToArray()));
                os.Write(array2, 0, array2.Length);
            }

            os.Write(PdfStream.ENDSTREAM, 0, PdfStream.ENDSTREAM.Length);
        }
    }
}
