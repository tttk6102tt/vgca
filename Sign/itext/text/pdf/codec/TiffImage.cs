using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.text.exceptions;
using Sign.SystemItext.util.zlib;

namespace Sign.itext.text.pdf.codec
{
    public class TiffImage
    {
        public static int GetNumberOfPages(RandomAccessFileOrArray s)
        {
            return TIFFDirectory.GetNumDirectories(s);
        }

        private static int GetDpi(TIFFField fd, int resolutionUnit)
        {
            if (fd == null)
            {
                return 0;
            }

            long[] asRational = fd.GetAsRational(0);
            float num = (float)asRational[0] / (float)asRational[1];
            int result = 0;
            switch (resolutionUnit)
            {
                case 1:
                case 2:
                    result = (int)((double)num + 0.5);
                    break;
                case 3:
                    result = (int)((double)num * 2.54 + 0.5);
                    break;
            }

            return result;
        }

        public static Image GetTiffImage(RandomAccessFileOrArray s, bool recoverFromImageError, int page, bool direct)
        {
            if (page < 1)
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("the.page.number.must.be.gt.eq.1"));
            }

            TIFFDirectory tIFFDirectory = new TIFFDirectory(s, page - 1);
            if (tIFFDirectory.IsTagPresent(322))
            {
                throw new ArgumentException(MessageLocalization.GetComposedMessage("tiles.are.not.supported"));
            }

            int num = (int)tIFFDirectory.GetFieldAsLong(259);
            switch (num)
            {
                default:
                    return GetTiffImageColor(tIFFDirectory, s);
                case 2:
                case 3:
                case 4:
                case 32771:
                    {
                        float num2 = 0f;
                        if (tIFFDirectory.IsTagPresent(274))
                        {
                            switch (tIFFDirectory.GetFieldAsLong(274))
                            {
                                case 3L:
                                case 4L:
                                    num2 = (float)Math.PI;
                                    break;
                                case 5L:
                                case 8L:
                                    num2 = (float)Math.PI / 2f;
                                    break;
                                case 6L:
                                case 7L:
                                    num2 = -(float)Math.PI / 2f;
                                    break;
                            }
                        }

                        Image image = null;
                        long num3 = 0L;
                        long tiffT6Options = 0L;
                        int fillOrder = 1;
                        int num4 = (int)tIFFDirectory.GetFieldAsLong(257);
                        int num5 = (int)tIFFDirectory.GetFieldAsLong(256);
                        int num6 = 0;
                        int num7 = 0;
                        float xYRatio = 0f;
                        int num8 = 2;
                        if (tIFFDirectory.IsTagPresent(296))
                        {
                            num8 = (int)tIFFDirectory.GetFieldAsLong(296);
                        }

                        num6 = GetDpi(tIFFDirectory.GetField(282), num8);
                        num7 = GetDpi(tIFFDirectory.GetField(283), num8);
                        if (num8 == 1)
                        {
                            if (num7 != 0)
                            {
                                xYRatio = (float)num6 / (float)num7;
                            }

                            num6 = 0;
                            num7 = 0;
                        }

                        int num9 = num4;
                        if (tIFFDirectory.IsTagPresent(278))
                        {
                            num9 = (int)tIFFDirectory.GetFieldAsLong(278);
                        }

                        if (num9 <= 0 || num9 > num4)
                        {
                            num9 = num4;
                        }

                        long[] arrayLongShort = GetArrayLongShort(tIFFDirectory, 273);
                        long[] array = GetArrayLongShort(tIFFDirectory, 279);
                        if ((array == null || (array.Length == 1 && (array[0] == 0L || array[0] + arrayLongShort[0] > s.Length))) && num4 == num9)
                        {
                            array = new long[1] { s.Length - (int)arrayLongShort[0] };
                        }

                        TIFFField field = tIFFDirectory.GetField(266);
                        if (field != null)
                        {
                            fillOrder = field.GetAsInt(0);
                        }

                        int num10 = 0;
                        if (tIFFDirectory.IsTagPresent(262) && tIFFDirectory.GetFieldAsLong(262) == 1)
                        {
                            num10 |= 1;
                        }

                        int typeCCITT = 0;
                        switch (num)
                        {
                            case 2:
                            case 32771:
                                typeCCITT = 257;
                                num10 |= 0xA;
                                break;
                            case 3:
                                {
                                    typeCCITT = 257;
                                    num10 |= 0xC;
                                    TIFFField field3 = tIFFDirectory.GetField(292);
                                    if (field3 != null)
                                    {
                                        num3 = field3.GetAsLong(0);
                                        if ((num3 & 1) != 0L)
                                        {
                                            typeCCITT = 258;
                                        }

                                        if ((num3 & 4) != 0L)
                                        {
                                            num10 |= 2;
                                        }
                                    }

                                    break;
                                }
                            case 4:
                                {
                                    typeCCITT = 256;
                                    TIFFField field2 = tIFFDirectory.GetField(293);
                                    if (field2 != null)
                                    {
                                        tiffT6Options = field2.GetAsLong(0);
                                    }

                                    break;
                                }
                        }

                        if (direct && num9 == num4)
                        {
                            byte[] array2 = new byte[(int)array[0]];
                            s.Seek(arrayLongShort[0]);
                            s.ReadFully(array2);
                            image = Image.GetInstance(num5, num4, reverseBits: false, typeCCITT, num10, array2);
                            image.Inverted = true;
                        }
                        else
                        {
                            int num11 = num4;
                            CCITTG4Encoder cCITTG4Encoder = new CCITTG4Encoder(num5);
                            for (int i = 0; i < arrayLongShort.Length; i++)
                            {
                                byte[] array3 = new byte[(int)array[i]];
                                s.Seek(arrayLongShort[i]);
                                s.ReadFully(array3);
                                int num12 = Math.Min(num9, num11);
                                TIFFFaxDecoder tIFFFaxDecoder = new TIFFFaxDecoder(fillOrder, num5, num12);
                                tIFFFaxDecoder.RecoverFromImageError = recoverFromImageError;
                                byte[] array4 = new byte[(num5 + 7) / 8 * num12];
                                switch (num)
                                {
                                    case 2:
                                    case 32771:
                                        tIFFFaxDecoder.Decode1D(array4, array3, 0, num12);
                                        cCITTG4Encoder.Fax4Encode(array4, num12);
                                        break;
                                    case 3:
                                        try
                                        {
                                            tIFFFaxDecoder.Decode2D(array4, array3, 0, num12, num3);
                                        }
                                        catch (Exception ex2)
                                        {
                                            num3 ^= 4;
                                            try
                                            {
                                                tIFFFaxDecoder.Decode2D(array4, array3, 0, num12, num3);
                                            }
                                            catch (Exception)
                                            {
                                                if (!recoverFromImageError)
                                                {
                                                    throw ex2;
                                                }

                                                if (num9 == 1)
                                                {
                                                    throw ex2;
                                                }

                                                array3 = new byte[(int)array[0]];
                                                s.Seek(arrayLongShort[0]);
                                                s.ReadFully(array3);
                                                image = Image.GetInstance(num5, num4, reverseBits: false, typeCCITT, num10, array3);
                                                image.Inverted = true;
                                                image.SetDpi(num6, num7);
                                                image.XYRatio = xYRatio;
                                                image.OriginalType = 5;
                                                if (num2 != 0f)
                                                {
                                                    image.InitialRotation = num2;
                                                }

                                                return image;
                                            }
                                        }

                                        cCITTG4Encoder.Fax4Encode(array4, num12);
                                        break;
                                    case 4:
                                        try
                                        {
                                            tIFFFaxDecoder.DecodeT6(array4, array3, 0, num12, tiffT6Options);
                                        }
                                        catch (InvalidImageException ex)
                                        {
                                            if (!recoverFromImageError)
                                            {
                                                throw ex;
                                            }
                                        }

                                        cCITTG4Encoder.Fax4Encode(array4, num12);
                                        break;
                                }

                                num11 -= num9;
                            }

                            byte[] data = cCITTG4Encoder.Close();
                            image = Image.GetInstance(num5, num4, reverseBits: false, 256, num10 & 1, data);
                        }

                        image.SetDpi(num6, num7);
                        image.XYRatio = xYRatio;
                        if (tIFFDirectory.IsTagPresent(34675))
                        {
                            try
                            {
                                ICC_Profile instance = ICC_Profile.GetInstance(tIFFDirectory.GetField(34675).GetAsBytes());
                                if (instance.NumComponents == 1)
                                {
                                    image.TagICC = instance;
                                }
                            }
                            catch
                            {
                            }
                        }

                        image.OriginalType = 5;
                        if (num2 != 0f)
                        {
                            image.InitialRotation = num2;
                        }

                        return image;
                    }
            }
        }

        public static Image GetTiffImage(RandomAccessFileOrArray s, bool recoverFromImageError, int page)
        {
            return GetTiffImage(s, recoverFromImageError, page, direct: false);
        }

        public static Image GetTiffImage(RandomAccessFileOrArray s, int page)
        {
            return GetTiffImage(s, page, direct: false);
        }

        public static Image GetTiffImage(RandomAccessFileOrArray s, int page, bool direct)
        {
            return GetTiffImage(s, recoverFromImageError: false, page, direct);
        }

        protected static Image GetTiffImageColor(TIFFDirectory dir, RandomAccessFileOrArray s)
        {
            int num = 1;
            TIFFLZWDecoder tIFFLZWDecoder = null;
            int num2 = (int)dir.GetFieldAsLong(259);
            switch (num2)
            {
                default:
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("the.compression.1.is.not.supported", num2));
                case 1:
                case 5:
                case 6:
                case 7:
                case 8:
                case 32773:
                case 32946:
                    {
                        int num3 = (int)dir.GetFieldAsLong(262);
                        switch (num3)
                        {
                            default:
                                if (num2 != 6 && num2 != 7)
                                {
                                    throw new ArgumentException(MessageLocalization.GetComposedMessage("the.photometric.1.is.not.supported", num3));
                                }

                                break;
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 5:
                                break;
                        }

                        float num4 = 0f;
                        if (dir.IsTagPresent(274))
                        {
                            switch (dir.GetFieldAsLong(274))
                            {
                                case 3L:
                                case 4L:
                                    num4 = (float)Math.PI;
                                    break;
                                case 5L:
                                case 8L:
                                    num4 = (float)Math.PI / 2f;
                                    break;
                                case 6L:
                                case 7L:
                                    num4 = -(float)Math.PI / 2f;
                                    break;
                            }
                        }

                        if (dir.IsTagPresent(284) && dir.GetFieldAsLong(284) == 2)
                        {
                            throw new ArgumentException(MessageLocalization.GetComposedMessage("planar.images.are.not.supported"));
                        }

                        int num5 = 0;
                        if (dir.IsTagPresent(338))
                        {
                            num5 = 1;
                        }

                        int num6 = 1;
                        if (dir.IsTagPresent(277))
                        {
                            num6 = (int)dir.GetFieldAsLong(277);
                        }

                        int num7 = 1;
                        if (dir.IsTagPresent(258))
                        {
                            num7 = (int)dir.GetFieldAsLong(258);
                        }

                        switch (num7)
                        {
                            default:
                                throw new ArgumentException(MessageLocalization.GetComposedMessage("bits.per.sample.1.is.not.supported", num7));
                            case 1:
                            case 2:
                            case 4:
                            case 8:
                                {
                                    Image image = null;
                                    int num8 = (int)dir.GetFieldAsLong(257);
                                    int num9 = (int)dir.GetFieldAsLong(256);
                                    int num10 = 0;
                                    int num11 = 0;
                                    int resolutionUnit = 2;
                                    if (dir.IsTagPresent(296))
                                    {
                                        resolutionUnit = (int)dir.GetFieldAsLong(296);
                                    }

                                    num10 = GetDpi(dir.GetField(282), resolutionUnit);
                                    num11 = GetDpi(dir.GetField(283), resolutionUnit);
                                    int num12 = 1;
                                    bool flag = false;
                                    TIFFField field = dir.GetField(266);
                                    if (field != null)
                                    {
                                        num12 = field.GetAsInt(0);
                                    }

                                    flag = num12 == 2;
                                    int num13 = num8;
                                    if (dir.IsTagPresent(278))
                                    {
                                        num13 = (int)dir.GetFieldAsLong(278);
                                    }

                                    if (num13 <= 0 || num13 > num8)
                                    {
                                        num13 = num8;
                                    }

                                    long[] arrayLongShort = GetArrayLongShort(dir, 273);
                                    long[] array = GetArrayLongShort(dir, 279);
                                    if ((array == null || (array.Length == 1 && (array[0] == 0L || array[0] + arrayLongShort[0] > s.Length))) && num8 == num13)
                                    {
                                        array = new long[1] { s.Length - (int)arrayLongShort[0] };
                                    }

                                    if (num2 == 5 || num2 == 32946 || num2 == 8)
                                    {
                                        TIFFField field2 = dir.GetField(317);
                                        if (field2 != null)
                                        {
                                            num = field2.GetAsInt(0);
                                            if (num != 1 && num != 2)
                                            {
                                                throw new Exception(MessageLocalization.GetComposedMessage("illegal.value.for.predictor.in.tiff.file"));
                                            }

                                            if (num == 2 && num7 != 8)
                                            {
                                                throw new Exception(MessageLocalization.GetComposedMessage("1.bit.samples.are.not.supported.for.horizontal.differencing.predictor", num7));
                                            }
                                        }
                                    }

                                    if (num2 == 5)
                                    {
                                        tIFFLZWDecoder = new TIFFLZWDecoder(num9, num, num6);
                                    }

                                    int num14 = num8;
                                    MemoryStream memoryStream = null;
                                    MemoryStream memoryStream2 = null;
                                    ZDeflaterOutputStream zDeflaterOutputStream = null;
                                    ZDeflaterOutputStream zDeflaterOutputStream2 = null;
                                    if (num5 > 0)
                                    {
                                        memoryStream2 = new MemoryStream();
                                        zDeflaterOutputStream2 = new ZDeflaterOutputStream(memoryStream2);
                                    }

                                    CCITTG4Encoder cCITTG4Encoder = null;
                                    if (num7 == 1 && num6 == 1 && num3 != 3)
                                    {
                                        cCITTG4Encoder = new CCITTG4Encoder(num9);
                                    }
                                    else
                                    {
                                        memoryStream = new MemoryStream();
                                        if (num2 != 6 && num2 != 7)
                                        {
                                            zDeflaterOutputStream = new ZDeflaterOutputStream(memoryStream);
                                        }
                                    }

                                    switch (num2)
                                    {
                                        case 6:
                                            {
                                                if (!dir.IsTagPresent(513))
                                                {
                                                    throw new IOException(MessageLocalization.GetComposedMessage("missing.tag.s.for.ojpeg.compression"));
                                                }

                                                int num16 = (int)dir.GetFieldAsLong(513);
                                                int num17 = (int)s.Length - num16;
                                                if (dir.IsTagPresent(514))
                                                {
                                                    num17 = (int)dir.GetFieldAsLong(514) + (int)array[0];
                                                }

                                                byte[] array5 = new byte[Math.Min(num17, s.Length - num16)];
                                                int num18 = (int)s.FilePointer;
                                                num18 += num16;
                                                s.Seek(num18);
                                                s.ReadFully(array5);
                                                TIFFField field3 = dir.GetField(347);
                                                if (field3 != null)
                                                {
                                                    byte[] asBytes = field3.GetAsBytes();
                                                    int sourceIndex = 0;
                                                    int num19 = asBytes.Length;
                                                    if (asBytes[0] == byte.MaxValue && asBytes[1] == 216)
                                                    {
                                                        sourceIndex = 2;
                                                        num19 -= 2;
                                                    }

                                                    if (asBytes[^2] == byte.MaxValue && asBytes[^1] == 217)
                                                    {
                                                        num19 -= 2;
                                                    }

                                                    byte[] array6 = new byte[num19];
                                                    Array.Copy(asBytes, sourceIndex, array6, 0, num19);
                                                    byte[] array7 = new byte[array5.Length + array6.Length];
                                                    Array.Copy(array5, 0, array7, 0, 2);
                                                    Array.Copy(array6, 0, array7, 2, array6.Length);
                                                    Array.Copy(array5, 2, array7, array6.Length + 2, array5.Length - 2);
                                                    array5 = array7;
                                                }

                                                image = new Jpeg(array5);
                                                break;
                                            }
                                        case 7:
                                            {
                                                if (array.Length > 1)
                                                {
                                                    throw new IOException(MessageLocalization.GetComposedMessage("compression.jpeg.is.only.supported.with.a.single.strip.this.image.has.1.strips", array.Length));
                                                }

                                                byte[] array4 = new byte[(int)array[0]];
                                                s.Seek(arrayLongShort[0]);
                                                s.ReadFully(array4);
                                                image = new Jpeg(array4);
                                                if (num3 == 2)
                                                {
                                                    image.ColorTransform = 0;
                                                }

                                                break;
                                            }
                                        default:
                                            {
                                                for (int i = 0; i < arrayLongShort.Length; i++)
                                                {
                                                    byte[] array2 = new byte[(int)array[i]];
                                                    s.Seek(arrayLongShort[i]);
                                                    s.ReadFully(array2);
                                                    int num15 = Math.Min(num13, num14);
                                                    byte[] array3 = null;
                                                    if (num2 != 1)
                                                    {
                                                        array3 = new byte[(num9 * num7 * num6 + 7) / 8 * num15];
                                                    }

                                                    if (flag)
                                                    {
                                                        TIFFFaxDecoder.ReverseBits(array2);
                                                    }

                                                    switch (num2)
                                                    {
                                                        case 8:
                                                        case 32946:
                                                            Inflate(array2, array3);
                                                            ApplyPredictor(array3, num, num9, num15, num6);
                                                            break;
                                                        case 1:
                                                            array3 = array2;
                                                            break;
                                                        case 32773:
                                                            DecodePackbits(array2, array3);
                                                            break;
                                                        case 5:
                                                            tIFFLZWDecoder.Decode(array2, array3, num15);
                                                            break;
                                                    }

                                                    if (num7 == 1 && num6 == 1 && num3 != 3)
                                                    {
                                                        cCITTG4Encoder.Fax4Encode(array3, num15);
                                                    }
                                                    else if (num5 > 0)
                                                    {
                                                        ProcessExtraSamples(zDeflaterOutputStream, zDeflaterOutputStream2, array3, num6, num7, num9, num15);
                                                    }
                                                    else
                                                    {
                                                        zDeflaterOutputStream.Write(array3, 0, array3.Length);
                                                    }

                                                    num14 -= num13;
                                                }

                                                if (num7 == 1 && num6 == 1 && num3 != 3)
                                                {
                                                    image = Image.GetInstance(num9, num8, reverseBits: false, 256, (num3 == 1) ? 1 : 0, cCITTG4Encoder.Close());
                                                    break;
                                                }

                                                zDeflaterOutputStream.Close();
                                                image = new ImgRaw(num9, num8, num6 - num5, num7, memoryStream.ToArray());
                                                image.Deflated = true;
                                                break;
                                            }
                                    }

                                    image.SetDpi(num10, num11);
                                    if (num2 != 6 && num2 != 7)
                                    {
                                        if (dir.IsTagPresent(34675))
                                        {
                                            try
                                            {
                                                ICC_Profile instance = ICC_Profile.GetInstance(dir.GetField(34675).GetAsBytes());
                                                if (num6 - num5 == instance.NumComponents)
                                                {
                                                    image.TagICC = instance;
                                                }
                                            }
                                            catch
                                            {
                                            }
                                        }

                                        if (dir.IsTagPresent(320))
                                        {
                                            char[] asChars = dir.GetField(320).GetAsChars();
                                            byte[] array8 = new byte[asChars.Length];
                                            int num20 = asChars.Length / 3;
                                            int num21 = num20 * 2;
                                            for (int j = 0; j < num20; j++)
                                            {
                                                array8[j * 3] = (byte)((int)asChars[j] >> 8);
                                                array8[j * 3 + 1] = (byte)((int)asChars[j + num20] >> 8);
                                                array8[j * 3 + 2] = (byte)((int)asChars[j + num21] >> 8);
                                            }

                                            bool flag2 = true;
                                            for (int k = 0; k < array8.Length; k++)
                                            {
                                                if (array8[k] != 0)
                                                {
                                                    flag2 = false;
                                                    break;
                                                }
                                            }

                                            if (flag2)
                                            {
                                                for (int l = 0; l < num20; l++)
                                                {
                                                    array8[l * 3] = (byte)asChars[l];
                                                    array8[l * 3 + 1] = (byte)asChars[l + num20];
                                                    array8[l * 3 + 2] = (byte)asChars[l + num21];
                                                }
                                            }

                                            PdfArray pdfArray = new PdfArray();
                                            pdfArray.Add(PdfName.INDEXED);
                                            pdfArray.Add(PdfName.DEVICERGB);
                                            pdfArray.Add(new PdfNumber(num20 - 1));
                                            pdfArray.Add(new PdfString(array8));
                                            PdfDictionary pdfDictionary = new PdfDictionary();
                                            pdfDictionary.Put(PdfName.COLORSPACE, pdfArray);
                                            image.Additional = pdfDictionary;
                                        }

                                        image.OriginalType = 5;
                                    }

                                    if (num3 == 0)
                                    {
                                        image.Inverted = true;
                                    }

                                    if (num4 != 0f)
                                    {
                                        image.InitialRotation = num4;
                                    }

                                    if (num5 > 0)
                                    {
                                        zDeflaterOutputStream2.Close();
                                        Image instance2 = Image.GetInstance(num9, num8, 1, num7, memoryStream2.ToArray());
                                        instance2.MakeMask();
                                        instance2.Deflated = true;
                                        image.ImageMask = instance2;
                                    }

                                    return image;
                                }
                        }
                    }
            }
        }

        private static Image ProcessExtraSamples(ZDeflaterOutputStream zip, ZDeflaterOutputStream mzip, byte[] outBuf, int samplePerPixel, int bitsPerSample, int width, int height)
        {
            if (bitsPerSample == 8)
            {
                byte[] array = new byte[width * height];
                int count = 0;
                int count2 = 0;
                int num = width * height * samplePerPixel;
                for (int i = 0; i < num; i += samplePerPixel)
                {
                    for (int j = 0; j < samplePerPixel - 1; j++)
                    {
                        outBuf[count2++] = outBuf[i + j];
                    }

                    array[count++] = outBuf[i + samplePerPixel - 1];
                }

                zip.Write(outBuf, 0, count2);
                mzip.Write(array, 0, count);
                return null;
            }

            throw new ArgumentException(MessageLocalization.GetComposedMessage("extra.samples.are.not.supported"));
        }

        private static long[] GetArrayLongShort(TIFFDirectory dir, int tag)
        {
            TIFFField field = dir.GetField(tag);
            if (field == null)
            {
                return null;
            }

            long[] array;
            if (field.GetType() == 4)
            {
                array = field.GetAsLongs();
            }
            else
            {
                char[] asChars = field.GetAsChars();
                array = new long[asChars.Length];
                for (int i = 0; i < asChars.Length; i++)
                {
                    array[i] = asChars[i];
                }
            }

            return array;
        }

        public static void DecodePackbits(byte[] data, byte[] dst)
        {
            int num = 0;
            int num2 = 0;
            try
            {
                while (num2 < dst.Length)
                {
                    sbyte b = (sbyte)data[num++];
                    if (b >= 0 && b <= sbyte.MaxValue)
                    {
                        for (int i = 0; i < b + 1; i++)
                        {
                            dst[num2++] = data[num++];
                        }
                    }
                    else if (b <= -1 && b >= -127)
                    {
                        sbyte b2 = (sbyte)data[num++];
                        for (int j = 0; j < -b + 1; j++)
                        {
                            dst[num2++] = (byte)b2;
                        }
                    }
                    else
                    {
                        num++;
                    }
                }
            }
            catch
            {
            }
        }

        public static void Inflate(byte[] deflated, byte[] inflated)
        {
            byte[] array = PdfReader.FlateDecode(deflated);
            Array.Copy(array, 0, inflated, 0, Math.Min(array.Length, inflated.Length));
        }

        public static void ApplyPredictor(byte[] uncompData, int predictor, int w, int h, int samplesPerPixel)
        {
            if (predictor != 2)
            {
                return;
            }

            for (int i = 0; i < h; i++)
            {
                int num = samplesPerPixel * (i * w + 1);
                for (int j = samplesPerPixel; j < w * samplesPerPixel; j++)
                {
                    uncompData[num] += uncompData[num - samplesPerPixel];
                    num++;
                }
            }
        }
    }
}
