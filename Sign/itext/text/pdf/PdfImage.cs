using Sign.itext.error_messages;
using Sign.itext.pdf;
using System.Net;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class PdfImage : PdfStream
    {
        internal const int TRANSFERSIZE = 4096;

        protected PdfName name;

        protected Image image;

        public virtual PdfName Name => name;

        public virtual Image Image => image;

        public PdfImage(Image image, string name, PdfIndirectReference maskRef)
        {
            this.image = image;
            if (name == null)
            {
                GenerateImgResName(image);
            }
            else
            {
                this.name = new PdfName(name);
            }

            Put(PdfName.TYPE, PdfName.XOBJECT);
            Put(PdfName.SUBTYPE, PdfName.IMAGE);
            Put(PdfName.WIDTH, new PdfNumber(image.Width));
            Put(PdfName.HEIGHT, new PdfNumber(image.Height));
            if (image.Layer != null)
            {
                Put(PdfName.OC, image.Layer.Ref);
            }

            if (image.IsMask() && (image.Bpc == 1 || image.Bpc > 255))
            {
                Put(PdfName.IMAGEMASK, PdfBoolean.PDFTRUE);
            }

            if (maskRef != null)
            {
                if (image.Smask)
                {
                    Put(PdfName.SMASK, maskRef);
                }
                else
                {
                    Put(PdfName.MASK, maskRef);
                }
            }

            if (image.IsMask() && image.Inverted)
            {
                Put(PdfName.DECODE, new PdfLiteral("[1 0]"));
            }

            if (image.Interpolation)
            {
                Put(PdfName.INTERPOLATE, PdfBoolean.PDFTRUE);
            }

            Stream stream = null;
            try
            {
                int[] transparency = image.Transparency;
                if (transparency != null && !image.IsMask() && maskRef == null)
                {
                    StringBuilder stringBuilder = new StringBuilder("[");
                    for (int i = 0; i < transparency.Length; i++)
                    {
                        stringBuilder.Append(transparency[i]).Append(' ');
                    }

                    stringBuilder.Append(']');
                    Put(PdfName.MASK, new PdfLiteral(stringBuilder.ToString()));
                }

                if (image.IsImgRaw())
                {
                    int colorspace = image.Colorspace;
                    bytes = image.RawData;
                    Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
                    int bpc = image.Bpc;
                    if (bpc > 255)
                    {
                        if (!image.IsMask())
                        {
                            Put(PdfName.COLORSPACE, PdfName.DEVICEGRAY);
                        }

                        Put(PdfName.BITSPERCOMPONENT, new PdfNumber(1));
                        Put(PdfName.FILTER, PdfName.CCITTFAXDECODE);
                        int num = bpc - 257;
                        PdfDictionary pdfDictionary = new PdfDictionary();
                        if (num != 0)
                        {
                            pdfDictionary.Put(PdfName.K, new PdfNumber(num));
                        }

                        if (((uint)colorspace & (true ? 1u : 0u)) != 0)
                        {
                            pdfDictionary.Put(PdfName.BLACKIS1, PdfBoolean.PDFTRUE);
                        }

                        if (((uint)colorspace & 2u) != 0)
                        {
                            pdfDictionary.Put(PdfName.ENCODEDBYTEALIGN, PdfBoolean.PDFTRUE);
                        }

                        if (((uint)colorspace & 4u) != 0)
                        {
                            pdfDictionary.Put(PdfName.ENDOFLINE, PdfBoolean.PDFTRUE);
                        }

                        if (((uint)colorspace & 8u) != 0)
                        {
                            pdfDictionary.Put(PdfName.ENDOFBLOCK, PdfBoolean.PDFFALSE);
                        }

                        pdfDictionary.Put(PdfName.COLUMNS, new PdfNumber(image.Width));
                        pdfDictionary.Put(PdfName.ROWS, new PdfNumber(image.Height));
                        Put(PdfName.DECODEPARMS, pdfDictionary);
                        return;
                    }

                    switch (colorspace)
                    {
                        case 1:
                            Put(PdfName.COLORSPACE, PdfName.DEVICEGRAY);
                            if (image.Inverted)
                            {
                                Put(PdfName.DECODE, new PdfLiteral("[1 0]"));
                            }

                            break;
                        case 3:
                            Put(PdfName.COLORSPACE, PdfName.DEVICERGB);
                            if (image.Inverted)
                            {
                                Put(PdfName.DECODE, new PdfLiteral("[1 0 1 0 1 0]"));
                            }

                            break;
                        default:
                            Put(PdfName.COLORSPACE, PdfName.DEVICECMYK);
                            if (image.Inverted)
                            {
                                Put(PdfName.DECODE, new PdfLiteral("[1 0 1 0 1 0 1 0]"));
                            }

                            break;
                    }

                    PdfDictionary additional = image.Additional;
                    if (additional != null)
                    {
                        Merge(additional);
                    }

                    if (image.IsMask() && (image.Bpc == 1 || image.Bpc > 8))
                    {
                        Remove(PdfName.COLORSPACE);
                    }

                    Put(PdfName.BITSPERCOMPONENT, new PdfNumber(image.Bpc));
                    if (image.Deflated)
                    {
                        Put(PdfName.FILTER, PdfName.FLATEDECODE);
                    }
                    else
                    {
                        FlateCompress(image.CompressionLevel);
                    }

                    return;
                }

                string text;
                if (image.RawData == null)
                {
                    WebRequest webRequest = WebRequest.Create(image.Url);
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    stream = webRequest.GetResponse().GetResponseStream();
                    text = image.Url.ToString();
                }
                else
                {
                    stream = new MemoryStream(image.RawData);
                    text = "Byte array";
                }

                switch (image.Type)
                {
                    case 32:
                        Put(PdfName.FILTER, PdfName.DCTDECODE);
                        if (image.ColorTransform == 0)
                        {
                            PdfDictionary pdfDictionary2 = new PdfDictionary();
                            pdfDictionary2.Put(PdfName.COLORTRANSFORM, new PdfNumber(0));
                            Put(PdfName.DECODEPARMS, pdfDictionary2);
                        }

                        switch (image.Colorspace)
                        {
                            case 1:
                                Put(PdfName.COLORSPACE, PdfName.DEVICEGRAY);
                                break;
                            case 3:
                                Put(PdfName.COLORSPACE, PdfName.DEVICERGB);
                                break;
                            default:
                                Put(PdfName.COLORSPACE, PdfName.DEVICECMYK);
                                if (image.Inverted)
                                {
                                    Put(PdfName.DECODE, new PdfLiteral("[1 0 1 0 1 0 1 0]"));
                                }

                                break;
                        }

                        Put(PdfName.BITSPERCOMPONENT, new PdfNumber(8));
                        if (image.RawData != null)
                        {
                            bytes = image.RawData;
                            Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
                            return;
                        }

                        streamBytes = new MemoryStream();
                        TransferBytes(stream, streamBytes, -1);
                        break;
                    case 33:
                        Put(PdfName.FILTER, PdfName.JPXDECODE);
                        if (image.Colorspace > 0)
                        {
                            switch (image.Colorspace)
                            {
                                case 1:
                                    Put(PdfName.COLORSPACE, PdfName.DEVICEGRAY);
                                    break;
                                case 3:
                                    Put(PdfName.COLORSPACE, PdfName.DEVICERGB);
                                    break;
                                default:
                                    Put(PdfName.COLORSPACE, PdfName.DEVICECMYK);
                                    break;
                            }

                            Put(PdfName.BITSPERCOMPONENT, new PdfNumber(image.Bpc));
                        }

                        if (image.RawData != null)
                        {
                            bytes = image.RawData;
                            Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
                            return;
                        }

                        streamBytes = new MemoryStream();
                        TransferBytes(stream, streamBytes, -1);
                        break;
                    case 36:
                        Put(PdfName.FILTER, PdfName.JBIG2DECODE);
                        Put(PdfName.COLORSPACE, PdfName.DEVICEGRAY);
                        Put(PdfName.BITSPERCOMPONENT, new PdfNumber(1));
                        if (image.RawData != null)
                        {
                            bytes = image.RawData;
                            Put(PdfName.LENGTH, new PdfNumber(bytes.Length));
                            return;
                        }

                        streamBytes = new MemoryStream();
                        TransferBytes(stream, streamBytes, -1);
                        break;
                    default:
                        throw new IOException(MessageLocalization.GetComposedMessage("1.is.an.unknown.image.format", text));
                }

                if (image.CompressionLevel > 0)
                {
                    FlateCompress(image.CompressionLevel);
                }

                Put(PdfName.LENGTH, new PdfNumber(streamBytes.Length));
            }
            finally
            {
                if (stream != null)
                {
                    try
                    {
                        stream.Close();
                    }
                    catch
                    {
                    }
                }
            }
        }

        internal static void TransferBytes(Stream inp, Stream outp, int len)
        {
            byte[] buffer = new byte[4096];
            if (len < 0)
            {
                len = 2147418112;
            }

            while (len != 0)
            {
                int num = inp.Read(buffer, 0, Math.Min(len, 4096));
                if (num <= 0)
                {
                    break;
                }

                outp.Write(buffer, 0, num);
                len -= num;
            }
        }

        protected virtual void ImportAll(PdfImage dup)
        {
            name = dup.name;
            compressed = dup.compressed;
            compressionLevel = dup.compressionLevel;
            streamBytes = dup.streamBytes;
            bytes = dup.bytes;
            hashMap = dup.hashMap;
        }

        private void GenerateImgResName(Image img)
        {
            name = new PdfName("img" + img.MySerialId.ToString("X"));
        }
    }
}
