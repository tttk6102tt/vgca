using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.text.exceptions;
using Sign.itext.text.pdf.codec;
using Sign.SystemItext.util.collections;

namespace Sign.itext.text.pdf
{
    public sealed class FilterHandlers
    {
        public interface IFilterHandler
        {
            byte[] Decode(byte[] b, PdfName filterName, PdfObject decodeParams, PdfDictionary streamDictionary);
        }

        private class Filter_FLATEDECODE : IFilterHandler
        {
            public byte[] Decode(byte[] b, PdfName filterName, PdfObject decodeParams, PdfDictionary streamDictionary)
            {
                b = PdfReader.FlateDecode(b);
                b = PdfReader.DecodePredictor(b, decodeParams);
                return b;
            }
        }

        private class Filter_ASCIIHEXDECODE : IFilterHandler
        {
            public byte[] Decode(byte[] b, PdfName filterName, PdfObject decodeParams, PdfDictionary streamDictionary)
            {
                b = PdfReader.ASCIIHexDecode(b);
                return b;
            }
        }

        private class Filter_ASCII85DECODE : IFilterHandler
        {
            public byte[] Decode(byte[] b, PdfName filterName, PdfObject decodeParams, PdfDictionary streamDictionary)
            {
                b = PdfReader.ASCII85Decode(b);
                return b;
            }
        }

        private class Filter_LZWDECODE : IFilterHandler
        {
            public byte[] Decode(byte[] b, PdfName filterName, PdfObject decodeParams, PdfDictionary streamDictionary)
            {
                b = PdfReader.LZWDecode(b);
                b = PdfReader.DecodePredictor(b, decodeParams);
                return b;
            }
        }

        private class Filter_CCITTFAXDECODE : IFilterHandler
        {
            public byte[] Decode(byte[] b, PdfName filterName, PdfObject decodeParams, PdfDictionary streamDictionary)
            {
                PdfNumber pdfNumber = (PdfNumber)PdfReader.GetPdfObjectRelease(streamDictionary.Get(PdfName.WIDTH));
                PdfNumber pdfNumber2 = (PdfNumber)PdfReader.GetPdfObjectRelease(streamDictionary.Get(PdfName.HEIGHT));
                if (pdfNumber == null || pdfNumber2 == null)
                {
                    throw new UnsupportedPdfException(MessageLocalization.GetComposedMessage("filter.ccittfaxdecode.is.only.supported.for.images"));
                }

                int intValue = pdfNumber.IntValue;
                int intValue2 = pdfNumber2.IntValue;
                PdfDictionary pdfDictionary = ((decodeParams is PdfDictionary) ? ((PdfDictionary)decodeParams) : null);
                int num = 0;
                bool flag = false;
                bool flag2 = false;
                if (pdfDictionary != null)
                {
                    PdfNumber asNumber = pdfDictionary.GetAsNumber(PdfName.K);
                    if (asNumber != null)
                    {
                        num = asNumber.IntValue;
                    }

                    PdfBoolean asBoolean = pdfDictionary.GetAsBoolean(PdfName.BLACKIS1);
                    if (asBoolean != null)
                    {
                        flag = asBoolean.BooleanValue;
                    }

                    asBoolean = pdfDictionary.GetAsBoolean(PdfName.ENCODEDBYTEALIGN);
                    if (asBoolean != null)
                    {
                        flag2 = asBoolean.BooleanValue;
                    }
                }

                byte[] array = new byte[(intValue + 7) / 8 * intValue2];
                TIFFFaxDecompressor tIFFFaxDecompressor = new TIFFFaxDecompressor();
                if (num == 0 || num > 0)
                {
                    int num2 = ((num > 0) ? 1 : 0);
                    num2 |= (flag2 ? 4 : 0);
                    tIFFFaxDecompressor.SetOptions(1, 3, num2, 0);
                    tIFFFaxDecompressor.DecodeRaw(array, b, intValue, intValue2);
                    if (tIFFFaxDecompressor.fails > 0)
                    {
                        byte[] array2 = new byte[(intValue + 7) / 8 * intValue2];
                        int fails = tIFFFaxDecompressor.fails;
                        tIFFFaxDecompressor.SetOptions(1, 2, num2, 0);
                        tIFFFaxDecompressor.DecodeRaw(array2, b, intValue, intValue2);
                        if (tIFFFaxDecompressor.fails < fails)
                        {
                            array = array2;
                        }
                    }
                }
                else
                {
                    new TIFFFaxDecoder(1, intValue, intValue2).DecodeT6(array, b, 0, intValue2, 0L);
                }

                if (!flag)
                {
                    int num3 = array.Length;
                    for (int i = 0; i < num3; i++)
                    {
                        array[i] ^= byte.MaxValue;
                    }
                }

                b = array;
                return b;
            }
        }

        private class Filter_DoNothing : IFilterHandler
        {
            public byte[] Decode(byte[] b, PdfName filterName, PdfObject decodeParams, PdfDictionary streamDictionary)
            {
                return b;
            }
        }

        private class Filter_RUNLENGTHDECODE : IFilterHandler
        {
            public byte[] Decode(byte[] b, PdfName filterName, PdfObject decodeParams, PdfDictionary streamDictionary)
            {
                MemoryStream memoryStream = new MemoryStream();
                sbyte b2 = -1;
                int num;
                for (num = 0; num < b.Length; num++)
                {
                    b2 = (sbyte)b[num];
                    if (b2 == sbyte.MinValue)
                    {
                        break;
                    }

                    if (b2 >= 0 && b2 <= sbyte.MaxValue)
                    {
                        int num2 = b2 + 1;
                        memoryStream.Write(b, num, num2);
                        num += num2;
                    }
                    else
                    {
                        num++;
                        for (int i = 0; i < 1 - b2; i++)
                        {
                            memoryStream.WriteByte(b[num]);
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        private static IDictionary<PdfName, IFilterHandler> defaults;

        static FilterHandlers()
        {
            defaults = new ReadOnlyDictionary<PdfName, IFilterHandler>(new Dictionary<PdfName, IFilterHandler>
            {
                [PdfName.FLATEDECODE] = new Filter_FLATEDECODE(),
                [PdfName.FL] = new Filter_FLATEDECODE(),
                [PdfName.ASCIIHEXDECODE] = new Filter_ASCIIHEXDECODE(),
                [PdfName.AHX] = new Filter_ASCIIHEXDECODE(),
                [PdfName.ASCII85DECODE] = new Filter_ASCII85DECODE(),
                [PdfName.A85] = new Filter_ASCII85DECODE(),
                [PdfName.LZWDECODE] = new Filter_LZWDECODE(),
                [PdfName.CCITTFAXDECODE] = new Filter_CCITTFAXDECODE(),
                [PdfName.CRYPT] = new Filter_DoNothing(),
                [PdfName.RUNLENGTHDECODE] = new Filter_RUNLENGTHDECODE()
            });
        }

        public static IDictionary<PdfName, IFilterHandler> GetDefaultFilterHandlers()
        {
            return defaults;
        }
    }
}
