using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.SystemItext.util;
using System.Net;

namespace Sign.itext.text.pdf.codec
{
    public class BmpImage
    {
        private Stream inputStream;

        private long bitmapFileSize;

        private long bitmapOffset;

        private long compression;

        private long imageSize;

        private byte[] palette;

        private int imageType;

        private int numBands;

        private bool isBottomUp;

        private int bitsPerPixel;

        private int redMask;

        private int greenMask;

        private int blueMask;

        private int alphaMask;

        public Dictionary<string, object> properties = new Dictionary<string, object>();

        private long xPelsPerMeter;

        private long yPelsPerMeter;

        private const int VERSION_2_1_BIT = 0;

        private const int VERSION_2_4_BIT = 1;

        private const int VERSION_2_8_BIT = 2;

        private const int VERSION_2_24_BIT = 3;

        private const int VERSION_3_1_BIT = 4;

        private const int VERSION_3_4_BIT = 5;

        private const int VERSION_3_8_BIT = 6;

        private const int VERSION_3_24_BIT = 7;

        private const int VERSION_3_NT_16_BIT = 8;

        private const int VERSION_3_NT_32_BIT = 9;

        private const int VERSION_4_1_BIT = 10;

        private const int VERSION_4_4_BIT = 11;

        private const int VERSION_4_8_BIT = 12;

        private const int VERSION_4_16_BIT = 13;

        private const int VERSION_4_24_BIT = 14;

        private const int VERSION_4_32_BIT = 15;

        private const int LCS_CALIBRATED_RGB = 0;

        private const int LCS_sRGB = 1;

        private const int LCS_CMYK = 2;

        private const int BI_RGB = 0;

        private const int BI_RLE8 = 1;

        private const int BI_RLE4 = 2;

        private const int BI_BITFIELDS = 3;

        private int width;

        private int height;

        internal BmpImage(Stream isp, bool noHeader, int size)
        {
            bitmapFileSize = size;
            bitmapOffset = 0L;
            Process(isp, noHeader);
        }

        public static Image GetImage(Uri url)
        {
            Stream stream = null;
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                stream = webRequest.GetResponse().GetResponseStream();
                Image image = GetImage(stream);
                image.Url = url;
                return image;
            }
            finally
            {
                stream?.Close();
            }
        }

        public static Image GetImage(Stream isp)
        {
            return GetImage(isp, noHeader: false, 0);
        }

        public static Image GetImage(Stream isp, bool noHeader, int size)
        {
            BmpImage bmpImage = new BmpImage(isp, noHeader, size);
            Image image = bmpImage.GetImage();
            image.SetDpi((int)((double)bmpImage.xPelsPerMeter * 0.0254 + 0.5), (int)((double)bmpImage.yPelsPerMeter * 0.0254 + 0.5));
            image.OriginalType = 4;
            return image;
        }

        public static Image GetImage(string file)
        {
            return GetImage(Utilities.ToURL(file));
        }

        public static Image GetImage(byte[] data)
        {
            Image image = GetImage(new MemoryStream(data));
            image.OriginalData = data;
            return image;
        }

        protected virtual void Process(Stream stream, bool noHeader)
        {
            if (noHeader || stream is BufferedStream)
            {
                inputStream = stream;
            }
            else
            {
                inputStream = new BufferedStream(stream);
            }

            if (!noHeader)
            {
                if (ReadUnsignedByte(inputStream) != 66 || ReadUnsignedByte(inputStream) != 77)
                {
                    throw new Exception(MessageLocalization.GetComposedMessage("invalid.magic.value.for.bmp.file"));
                }

                bitmapFileSize = ReadDWord(inputStream);
                ReadWord(inputStream);
                ReadWord(inputStream);
                bitmapOffset = ReadDWord(inputStream);
            }

            long num = ReadDWord(inputStream);
            if (num == 12)
            {
                width = ReadWord(inputStream);
                height = ReadWord(inputStream);
            }
            else
            {
                width = ReadLong(inputStream);
                height = ReadLong(inputStream);
            }

            int num2 = ReadWord(inputStream);
            bitsPerPixel = ReadWord(inputStream);
            properties["color_planes"] = num2;
            properties["bits_per_pixel"] = bitsPerPixel;
            numBands = 3;
            if (bitmapOffset == 0L)
            {
                bitmapOffset = num;
            }

            if (num == 12)
            {
                properties["bmp_version"] = "BMP v. 2.x";
                if (bitsPerPixel == 1)
                {
                    imageType = 0;
                }
                else if (bitsPerPixel == 4)
                {
                    imageType = 1;
                }
                else if (bitsPerPixel == 8)
                {
                    imageType = 2;
                }
                else if (bitsPerPixel == 24)
                {
                    imageType = 3;
                }

                int num3 = (int)((bitmapOffset - 14 - num) / 3) * 3;
                if (bitmapOffset == num)
                {
                    switch (imageType)
                    {
                        case 0:
                            num3 = 6;
                            break;
                        case 1:
                            num3 = 48;
                            break;
                        case 2:
                            num3 = 768;
                            break;
                        case 3:
                            num3 = 0;
                            break;
                    }

                    bitmapOffset = num + num3;
                }

                ReadPalette(num3);
            }
            else
            {
                compression = ReadDWord(inputStream);
                imageSize = ReadDWord(inputStream);
                xPelsPerMeter = ReadLong(inputStream);
                yPelsPerMeter = ReadLong(inputStream);
                long num4 = ReadDWord(inputStream);
                long num5 = ReadDWord(inputStream);
                switch (compression)
                {
                    case 0L:
                        properties["compression"] = "BI_RGB";
                        break;
                    case 1L:
                        properties["compression"] = "BI_RLE8";
                        break;
                    case 2L:
                        properties["compression"] = "BI_RLE4";
                        break;
                    case 3L:
                        properties["compression"] = "BI_BITFIELDS";
                        break;
                }

                properties["x_pixels_per_meter"] = xPelsPerMeter;
                properties["y_pixels_per_meter"] = yPelsPerMeter;
                properties["colors_used"] = num4;
                properties["colors_important"] = num5;
                switch (num)
                {
                    case 40L:
                    case 52L:
                    case 56L:
                        switch (compression)
                        {
                            case 0L:
                            case 1L:
                            case 2L:
                                {
                                    if (bitsPerPixel == 1)
                                    {
                                        imageType = 4;
                                    }
                                    else if (bitsPerPixel == 4)
                                    {
                                        imageType = 5;
                                    }
                                    else if (bitsPerPixel == 8)
                                    {
                                        imageType = 6;
                                    }
                                    else if (bitsPerPixel == 24)
                                    {
                                        imageType = 7;
                                    }
                                    else if (bitsPerPixel == 16)
                                    {
                                        imageType = 8;
                                        redMask = 31744;
                                        greenMask = 992;
                                        blueMask = 31;
                                        properties["red_mask"] = redMask;
                                        properties["green_mask"] = greenMask;
                                        properties["blue_mask"] = blueMask;
                                    }
                                    else if (bitsPerPixel == 32)
                                    {
                                        imageType = 9;
                                        redMask = 16711680;
                                        greenMask = 65280;
                                        blueMask = 255;
                                        properties["red_mask"] = redMask;
                                        properties["green_mask"] = greenMask;
                                        properties["blue_mask"] = blueMask;
                                    }

                                    if (num >= 52)
                                    {
                                        redMask = (int)ReadDWord(inputStream);
                                        greenMask = (int)ReadDWord(inputStream);
                                        blueMask = (int)ReadDWord(inputStream);
                                        properties["red_mask"] = redMask;
                                        properties["green_mask"] = greenMask;
                                        properties["blue_mask"] = blueMask;
                                    }

                                    if (num == 56)
                                    {
                                        alphaMask = (int)ReadDWord(inputStream);
                                        properties["alpha_mask"] = alphaMask;
                                    }

                                    int sizeOfPalette = (int)((bitmapOffset - 14 - num) / 4) * 4;
                                    if (bitmapOffset == num)
                                    {
                                        sizeOfPalette = imageType switch
                                        {
                                            4 => (int)((num4 == 0L) ? 2 : num4) * 4,
                                            5 => (int)((num4 == 0L) ? 16 : num4) * 4,
                                            6 => (int)((num4 == 0L) ? 256 : num4) * 4,
                                            _ => 0,
                                        };
                                        bitmapOffset = num + sizeOfPalette;
                                    }

                                    ReadPalette(sizeOfPalette);
                                    properties["bmp_version"] = "BMP v. 3.x";
                                    break;
                                }
                            case 3L:
                                if (bitsPerPixel == 16)
                                {
                                    imageType = 8;
                                }
                                else if (bitsPerPixel == 32)
                                {
                                    imageType = 9;
                                }

                                redMask = (int)ReadDWord(inputStream);
                                greenMask = (int)ReadDWord(inputStream);
                                blueMask = (int)ReadDWord(inputStream);
                                if (num == 56)
                                {
                                    alphaMask = (int)ReadDWord(inputStream);
                                    properties["alpha_mask"] = alphaMask;
                                }

                                properties["red_mask"] = redMask;
                                properties["green_mask"] = greenMask;
                                properties["blue_mask"] = blueMask;
                                if (num4 != 0L)
                                {
                                    int sizeOfPalette = (int)num4 * 4;
                                    ReadPalette(sizeOfPalette);
                                }

                                properties["bmp_version"] = "BMP v. 3.x NT";
                                break;
                            default:
                                throw new Exception("Invalid compression specified in BMP file.");
                        }

                        break;
                    case 108L:
                        {
                            properties["bmp_version"] = "BMP v. 4.x";
                            redMask = (int)ReadDWord(inputStream);
                            greenMask = (int)ReadDWord(inputStream);
                            blueMask = (int)ReadDWord(inputStream);
                            alphaMask = (int)ReadDWord(inputStream);
                            long num6 = ReadDWord(inputStream);
                            int num7 = ReadLong(inputStream);
                            int num8 = ReadLong(inputStream);
                            int num9 = ReadLong(inputStream);
                            int num10 = ReadLong(inputStream);
                            int num11 = ReadLong(inputStream);
                            int num12 = ReadLong(inputStream);
                            int num13 = ReadLong(inputStream);
                            int num14 = ReadLong(inputStream);
                            int num15 = ReadLong(inputStream);
                            long num16 = ReadDWord(inputStream);
                            long num17 = ReadDWord(inputStream);
                            long num18 = ReadDWord(inputStream);
                            if (bitsPerPixel == 1)
                            {
                                imageType = 10;
                            }
                            else if (bitsPerPixel == 4)
                            {
                                imageType = 11;
                            }
                            else if (bitsPerPixel == 8)
                            {
                                imageType = 12;
                            }
                            else if (bitsPerPixel == 16)
                            {
                                imageType = 13;
                                if ((int)compression == 0)
                                {
                                    redMask = 31744;
                                    greenMask = 992;
                                    blueMask = 31;
                                }
                            }
                            else if (bitsPerPixel == 24)
                            {
                                imageType = 14;
                            }
                            else if (bitsPerPixel == 32)
                            {
                                imageType = 15;
                                if ((int)compression == 0)
                                {
                                    redMask = 16711680;
                                    greenMask = 65280;
                                    blueMask = 255;
                                }
                            }

                            properties["red_mask"] = redMask;
                            properties["green_mask"] = greenMask;
                            properties["blue_mask"] = blueMask;
                            properties["alpha_mask"] = alphaMask;
                            int num19 = (int)((bitmapOffset - 14 - num) / 4) * 4;
                            if (bitmapOffset == num)
                            {
                                num19 = imageType switch
                                {
                                    10 => (int)((num4 == 0L) ? 2 : num4) * 4,
                                    11 => (int)((num4 == 0L) ? 16 : num4) * 4,
                                    12 => (int)((num4 == 0L) ? 256 : num4) * 4,
                                    _ => 0,
                                };
                                bitmapOffset = num + num19;
                            }

                            ReadPalette(num19);
                            switch (num6)
                            {
                                case 0L:
                                    properties["color_space"] = "LCS_CALIBRATED_RGB";
                                    properties["redX"] = num7;
                                    properties["redY"] = num8;
                                    properties["redZ"] = num9;
                                    properties["greenX"] = num10;
                                    properties["greenY"] = num11;
                                    properties["greenZ"] = num12;
                                    properties["blueX"] = num13;
                                    properties["blueY"] = num14;
                                    properties["blueZ"] = num15;
                                    properties["gamma_red"] = num16;
                                    properties["gamma_green"] = num17;
                                    properties["gamma_blue"] = num18;
                                    throw new Exception("Not implemented yet.");
                                case 1L:
                                    properties["color_space"] = "LCS_sRGB";
                                    break;
                                case 2L:
                                    properties["color_space"] = "LCS_CMYK";
                                    throw new Exception("Not implemented yet.");
                            }

                            break;
                        }
                    default:
                        properties["bmp_version"] = "BMP v. 5.x";
                        throw new Exception("BMP version 5 not implemented yet.");
                }
            }

            if (height > 0)
            {
                isBottomUp = true;
            }
            else
            {
                isBottomUp = false;
                height = Math.Abs(height);
            }

            if (bitsPerPixel == 1 || bitsPerPixel == 4 || bitsPerPixel == 8)
            {
                numBands = 1;
                if (imageType == 0 || imageType == 1 || imageType == 2)
                {
                    int num20 = palette.Length / 3;
                    if (num20 > 256)
                    {
                        num20 = 256;
                    }

                    byte[] array = new byte[num20];
                    byte[] array2 = new byte[num20];
                    byte[] array3 = new byte[num20];
                    for (int i = 0; i < num20; i++)
                    {
                        int num21 = 3 * i;
                        array3[i] = palette[num21];
                        array2[i] = palette[num21 + 1];
                        array[i] = palette[num21 + 2];
                    }
                }
                else
                {
                    int num20 = palette.Length / 4;
                    if (num20 > 256)
                    {
                        num20 = 256;
                    }

                    byte[] array = new byte[num20];
                    byte[] array2 = new byte[num20];
                    byte[] array3 = new byte[num20];
                    for (int j = 0; j < num20; j++)
                    {
                        int num22 = 4 * j;
                        array3[j] = palette[num22];
                        array2[j] = palette[num22 + 1];
                        array[j] = palette[num22 + 2];
                    }
                }
            }
            else if (bitsPerPixel == 16)
            {
                numBands = 3;
            }
            else if (bitsPerPixel == 32)
            {
                numBands = ((alphaMask == 0) ? 3 : 4);
            }
            else
            {
                numBands = 3;
            }
        }

        private byte[] GetPalette(int group)
        {
            if (palette == null)
            {
                return null;
            }

            byte[] array = new byte[palette.Length / group * 3];
            int num = palette.Length / group;
            for (int i = 0; i < num; i++)
            {
                int num2 = i * group;
                int num3 = i * 3;
                array[num3 + 2] = palette[num2++];
                array[num3 + 1] = palette[num2++];
                array[num3] = palette[num2];
            }

            return array;
        }

        private Image GetImage()
        {
            byte[] array = null;
            switch (imageType)
            {
                case 0:
                    return Read1Bit(3);
                case 1:
                    return Read4Bit(3);
                case 2:
                    return Read8Bit(3);
                case 3:
                    array = new byte[width * height * 3];
                    Read24Bit(array);
                    return new ImgRaw(width, height, 3, 8, array);
                case 4:
                    return Read1Bit(4);
                case 5:
                    return compression switch
                    {
                        0L => Read4Bit(4),
                        2L => ReadRLE4(),
                        _ => throw new Exception("Invalid compression specified for BMP file."),
                    };
                case 6:
                    return compression switch
                    {
                        0L => Read8Bit(4),
                        1L => ReadRLE8(),
                        _ => throw new Exception("Invalid compression specified for BMP file."),
                    };
                case 7:
                    array = new byte[width * height * 3];
                    Read24Bit(array);
                    return new ImgRaw(width, height, 3, 8, array);
                case 8:
                    return Read1632Bit(is32: false);
                case 9:
                    return Read1632Bit(is32: true);
                case 10:
                    return Read1Bit(4);
                case 11:
                    return compression switch
                    {
                        0L => Read4Bit(4),
                        2L => ReadRLE4(),
                        _ => throw new Exception("Invalid compression specified for BMP file."),
                    };
                case 12:
                    return compression switch
                    {
                        0L => Read8Bit(4),
                        1L => ReadRLE8(),
                        _ => throw new Exception("Invalid compression specified for BMP file."),
                    };
                case 13:
                    return Read1632Bit(is32: false);
                case 14:
                    array = new byte[width * height * 3];
                    Read24Bit(array);
                    return new ImgRaw(width, height, 3, 8, array);
                case 15:
                    return Read1632Bit(is32: true);
                default:
                    return null;
            }
        }

        private Image IndexedModel(byte[] bdata, int bpc, int paletteEntries)
        {
            ImgRaw imgRaw = new ImgRaw(width, height, 1, bpc, bdata);
            PdfArray pdfArray = new PdfArray
            {
                PdfName.INDEXED,
                PdfName.DEVICERGB
            };
            byte[] array = GetPalette(paletteEntries);
            int num = array.Length;
            pdfArray.Add(new PdfNumber(num / 3 - 1));
            pdfArray.Add(new PdfString(array));
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.COLORSPACE, pdfArray);
            imgRaw.Additional = pdfDictionary;
            return imgRaw;
        }

        private void ReadPalette(int sizeOfPalette)
        {
            if (sizeOfPalette == 0)
            {
                return;
            }

            palette = new byte[sizeOfPalette];
            int num;
            for (int i = 0; i < sizeOfPalette; i += num)
            {
                num = inputStream.Read(palette, i, sizeOfPalette - i);
                if (num <= 0)
                {
                    throw new IOException(MessageLocalization.GetComposedMessage("incomplete.palette"));
                }
            }

            properties["palette"] = palette;
        }

        private Image Read1Bit(int paletteEntries)
        {
            byte[] array = new byte[(width + 7) / 8 * height];
            int num = 0;
            int num2 = (int)Math.Ceiling((double)width / 8.0);
            int num3 = num2 % 4;
            if (num3 != 0)
            {
                num = 4 - num3;
            }

            int num4 = (num2 + num) * height;
            byte[] array2 = new byte[num4];
            for (int i = 0; i < num4; i += inputStream.Read(array2, i, num4 - i))
            {
            }

            if (isBottomUp)
            {
                for (int j = 0; j < height; j++)
                {
                    Array.Copy(array2, num4 - (j + 1) * (num2 + num), array, j * num2, num2);
                }
            }
            else
            {
                for (int k = 0; k < height; k++)
                {
                    Array.Copy(array2, k * (num2 + num), array, k * num2, num2);
                }
            }

            return IndexedModel(array, 1, paletteEntries);
        }

        private Image Read4Bit(int paletteEntries)
        {
            byte[] array = new byte[(width + 1) / 2 * height];
            int num = 0;
            int num2 = (int)Math.Ceiling((double)width / 2.0);
            int num3 = num2 % 4;
            if (num3 != 0)
            {
                num = 4 - num3;
            }

            int num4 = (num2 + num) * height;
            byte[] array2 = new byte[num4];
            for (int i = 0; i < num4; i += inputStream.Read(array2, i, num4 - i))
            {
            }

            if (isBottomUp)
            {
                for (int j = 0; j < height; j++)
                {
                    Array.Copy(array2, num4 - (j + 1) * (num2 + num), array, j * num2, num2);
                }
            }
            else
            {
                for (int k = 0; k < height; k++)
                {
                    Array.Copy(array2, k * (num2 + num), array, k * num2, num2);
                }
            }

            return IndexedModel(array, 4, paletteEntries);
        }

        private Image Read8Bit(int paletteEntries)
        {
            byte[] array = new byte[width * height];
            int num = 0;
            int num2 = width * 8;
            if (num2 % 32 != 0)
            {
                num = (num2 / 32 + 1) * 32 - num2;
                num = (int)Math.Ceiling((double)num / 8.0);
            }

            int num3 = (width + num) * height;
            byte[] array2 = new byte[num3];
            for (int i = 0; i < num3; i += inputStream.Read(array2, i, num3 - i))
            {
            }

            if (isBottomUp)
            {
                for (int j = 0; j < height; j++)
                {
                    Array.Copy(array2, num3 - (j + 1) * (width + num), array, j * width, width);
                }
            }
            else
            {
                for (int k = 0; k < height; k++)
                {
                    Array.Copy(array2, k * (width + num), array, k * width, width);
                }
            }

            return IndexedModel(array, 8, paletteEntries);
        }

        private void Read24Bit(byte[] bdata)
        {
            int num = 0;
            int num2 = width * 24;
            if (num2 % 32 != 0)
            {
                num = (num2 / 32 + 1) * 32 - num2;
                num = (int)Math.Ceiling((double)num / 8.0);
            }

            int num3 = (width * 3 + 3) / 4 * 4 * height;
            byte[] array = new byte[num3];
            int num4;
            for (int i = 0; i < num3; i += num4)
            {
                num4 = inputStream.Read(array, i, num3 - i);
                if (num4 < 0)
                {
                    break;
                }
            }

            int num5 = 0;
            int num7;
            if (isBottomUp)
            {
                int num6 = width * height * 3 - 1;
                num7 = -num;
                for (int j = 0; j < height; j++)
                {
                    num5 = num6 - (j + 1) * width * 3 + 1;
                    num7 += num;
                    for (int k = 0; k < width; k++)
                    {
                        bdata[num5 + 2] = array[num7++];
                        bdata[num5 + 1] = array[num7++];
                        bdata[num5] = array[num7++];
                        num5 += 3;
                    }
                }

                return;
            }

            num7 = -num;
            for (int l = 0; l < height; l++)
            {
                num7 += num;
                for (int m = 0; m < width; m++)
                {
                    bdata[num5 + 2] = array[num7++];
                    bdata[num5 + 1] = array[num7++];
                    bdata[num5] = array[num7++];
                    num5 += 3;
                }
            }
        }

        private int FindMask(int mask)
        {
            for (int i = 0; i < 32; i++)
            {
                if ((mask & 1) == 1)
                {
                    break;
                }

                mask = Util.USR(mask, 1);
            }

            return mask;
        }

        private int FindShift(int mask)
        {
            int i;
            for (i = 0; i < 32; i++)
            {
                if ((mask & 1) == 1)
                {
                    break;
                }

                mask = Util.USR(mask, 1);
            }

            return i;
        }

        private Image Read1632Bit(bool is32)
        {
            int num = FindMask(redMask);
            int op = FindShift(redMask);
            int num2 = num + 1;
            int num3 = FindMask(greenMask);
            int op2 = FindShift(greenMask);
            int num4 = num3 + 1;
            int num5 = FindMask(blueMask);
            int op3 = FindShift(blueMask);
            int num6 = num5 + 1;
            byte[] array = new byte[width * height * 3];
            int num7 = 0;
            if (!is32)
            {
                int num8 = width * 16;
                if (num8 % 32 != 0)
                {
                    num7 = (num8 / 32 + 1) * 32 - num8;
                    num7 = (int)Math.Ceiling((double)num7 / 8.0);
                }
            }

            if ((int)imageSize == 0)
            {
                _ = bitmapFileSize;
                _ = bitmapOffset;
            }

            int num9 = 0;
            if (isBottomUp)
            {
                for (int num10 = height - 1; num10 >= 0; num10--)
                {
                    num9 = width * 3 * num10;
                    for (int i = 0; i < width; i++)
                    {
                        int op4 = (int)((!is32) ? ReadWord(inputStream) : ReadDWord(inputStream));
                        array[num9++] = (byte)((Util.USR(op4, op) & num) * 256 / num2);
                        array[num9++] = (byte)((Util.USR(op4, op2) & num3) * 256 / num4);
                        array[num9++] = (byte)((Util.USR(op4, op3) & num5) * 256 / num6);
                    }

                    for (int j = 0; j < num7; j++)
                    {
                        inputStream.ReadByte();
                    }
                }
            }
            else
            {
                for (int k = 0; k < height; k++)
                {
                    for (int l = 0; l < width; l++)
                    {
                        int op4 = (int)((!is32) ? ReadWord(inputStream) : ReadDWord(inputStream));
                        array[num9++] = (byte)((Util.USR(op4, op) & num) * 256 / num2);
                        array[num9++] = (byte)((Util.USR(op4, op2) & num3) * 256 / num4);
                        array[num9++] = (byte)((Util.USR(op4, op3) & num5) * 256 / num6);
                    }

                    for (int m = 0; m < num7; m++)
                    {
                        inputStream.ReadByte();
                    }
                }
            }

            return new ImgRaw(width, height, 3, 8, array);
        }

        private Image ReadRLE8()
        {
            int num = (int)imageSize;
            if (num == 0)
            {
                num = (int)(bitmapFileSize - bitmapOffset);
            }

            byte[] array = new byte[num];
            for (int i = 0; i < num; i += inputStream.Read(array, i, num - i))
            {
            }

            byte[] array2 = DecodeRLE(is8: true, array);
            num = width * height;
            if (isBottomUp)
            {
                byte[] array3 = new byte[array2.Length];
                int num2 = width;
                for (int j = 0; j < height; j++)
                {
                    Array.Copy(array2, num - (j + 1) * num2, array3, j * num2, num2);
                }

                array2 = array3;
            }

            return IndexedModel(array2, 8, 4);
        }

        private Image ReadRLE4()
        {
            int num = (int)imageSize;
            if (num == 0)
            {
                num = (int)(bitmapFileSize - bitmapOffset);
            }

            byte[] array = new byte[num];
            for (int i = 0; i < num; i += inputStream.Read(array, i, num - i))
            {
            }

            byte[] array2 = DecodeRLE(is8: false, array);
            if (isBottomUp)
            {
                byte[] array3 = array2;
                array2 = new byte[width * height];
                int num2 = 0;
                for (int num3 = height - 1; num3 >= 0; num3--)
                {
                    int num4 = num3 * width;
                    int num5 = num2 + width;
                    while (num2 != num5)
                    {
                        array2[num2++] = array3[num4++];
                    }
                }
            }

            int num6 = (width + 1) / 2;
            byte[] array4 = new byte[num6 * height];
            int num7 = 0;
            int num8 = 0;
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < width; k++)
                {
                    if ((k & 1) == 0)
                    {
                        array4[num8 + k / 2] = (byte)(array2[num7++] << 4);
                    }
                    else
                    {
                        array4[num8 + k / 2] |= (byte)(array2[num7++] & 0xF);
                    }
                }

                num8 += num6;
            }

            return IndexedModel(array4, 4, 4);
        }

        private byte[] DecodeRLE(bool is8, byte[] values)
        {
            byte[] array = new byte[width * height];
            try
            {
                int num = 0;
                int num2 = 0;
                int num3 = 0;
                int num4 = 0;
                while (num4 < height)
                {
                    if (num < values.Length)
                    {
                        int num5 = values[num++] & 0xFF;
                        if (num5 != 0)
                        {
                            int num6 = values[num++] & 0xFF;
                            if (is8)
                            {
                                for (int num7 = num5; num7 != 0; num7--)
                                {
                                    array[num3++] = (byte)num6;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < num5; i++)
                                {
                                    array[num3++] = (byte)(((i & 1) == 1) ? ((uint)num6 & 0xFu) : ((uint)(num6 >> 4) & 0xFu));
                                }
                            }

                            num2 += num5;
                            continue;
                        }

                        num5 = values[num++] & 0xFF;
                        switch (num5)
                        {
                            case 0:
                                num2 = 0;
                                num4++;
                                num3 = num4 * width;
                                continue;
                            case 2:
                                num2 += values[num++] & 0xFF;
                                num4 += values[num++] & 0xFF;
                                num3 = num4 * width + num2;
                                continue;
                            case 1:
                                return array;
                        }

                        if (is8)
                        {
                            for (int num8 = num5; num8 != 0; num8--)
                            {
                                array[num3++] = (byte)(values[num++] & 0xFFu);
                            }
                        }
                        else
                        {
                            int num9 = 0;
                            for (int j = 0; j < num5; j++)
                            {
                                if ((j & 1) == 0)
                                {
                                    num9 = values[num++] & 0xFF;
                                }

                                array[num3++] = (byte)(((j & 1) == 1) ? ((uint)num9 & 0xFu) : ((uint)(num9 >> 4) & 0xFu));
                            }
                        }

                        num2 += num5;
                        if (is8)
                        {
                            if ((num5 & 1) == 1)
                            {
                                num++;
                            }
                        }
                        else if ((num5 & 3) == 1 || (num5 & 3) == 2)
                        {
                            num++;
                        }

                        continue;
                    }

                    return array;
                }

                return array;
            }
            catch
            {
                return array;
            }
        }

        private int ReadUnsignedByte(Stream stream)
        {
            return stream.ReadByte() & 0xFF;
        }

        private int ReadUnsignedShort(Stream stream)
        {
            int num = ReadUnsignedByte(stream);
            return ((ReadUnsignedByte(stream) << 8) | num) & 0xFFFF;
        }

        private int ReadShort(Stream stream)
        {
            int num = ReadUnsignedByte(stream);
            return (ReadUnsignedByte(stream) << 8) | num;
        }

        private int ReadWord(Stream stream)
        {
            return ReadUnsignedShort(stream);
        }

        private long ReadUnsignedInt(Stream stream)
        {
            int num = ReadUnsignedByte(stream);
            int num2 = ReadUnsignedByte(stream);
            int num3 = ReadUnsignedByte(stream);
            return ((ReadUnsignedByte(stream) << 24) | (num3 << 16) | (num2 << 8) | num) & 0xFFFFFFFFu;
        }

        private int ReadInt(Stream stream)
        {
            int num = ReadUnsignedByte(stream);
            int num2 = ReadUnsignedByte(stream);
            int num3 = ReadUnsignedByte(stream);
            return (ReadUnsignedByte(stream) << 24) | (num3 << 16) | (num2 << 8) | num;
        }

        private long ReadDWord(Stream stream)
        {
            return ReadUnsignedInt(stream);
        }

        private int ReadLong(Stream stream)
        {
            return ReadInt(stream);
        }
    }
}
