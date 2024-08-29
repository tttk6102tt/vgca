using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.SystemItext.util;
using Sign.SystemItext.util.zlib;
using System.Net;
using System.Text;

namespace Sign.itext.text.pdf.codec
{
    public class PngImage
    {
        public static int[] PNGID = new int[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

        public const string IHDR = "IHDR";

        public const string PLTE = "PLTE";

        public const string IDAT = "IDAT";

        public const string IEND = "IEND";

        public const string tRNS = "tRNS";

        public const string pHYs = "pHYs";

        public const string gAMA = "gAMA";

        public const string cHRM = "cHRM";

        public const string sRGB = "sRGB";

        public const string iCCP = "iCCP";

        private const int TRANSFERSIZE = 4096;

        private const int PNG_FILTER_NONE = 0;

        private const int PNG_FILTER_SUB = 1;

        private const int PNG_FILTER_UP = 2;

        private const int PNG_FILTER_AVERAGE = 3;

        private const int PNG_FILTER_PAETH = 4;

        private static PdfName[] intents = new PdfName[4]
        {
            PdfName.PERCEPTUAL,
            PdfName.RELATIVECOLORIMETRIC,
            PdfName.SATURATION,
            PdfName.ABSOLUTECOLORIMETRIC
        };

        private Stream isp;

        private Stream dataStream;

        private int width;

        private int height;

        private int bitDepth;

        private int colorType;

        private int compressionMethod;

        private int filterMethod;

        private int interlaceMethod;

        private PdfDictionary additional = new PdfDictionary();

        private byte[] image;

        private byte[] smask;

        private byte[] trans;

        private MemoryStream idat = new MemoryStream();

        private int dpiX;

        private int dpiY;

        private float XYRatio;

        private bool genBWMask;

        private bool palShades;

        private int transRedGray = -1;

        private int transGreen = -1;

        private int transBlue = -1;

        private int inputBands;

        private int bytesPerPixel;

        private byte[] colorTable;

        private float gamma = 1f;

        private bool hasCHRM;

        private float xW;

        private float yW;

        private float xR;

        private float yR;

        private float xG;

        private float yG;

        private float xB;

        private float yB;

        private PdfName intent;

        private ICC_Profile icc_profile;

        private PngImage(Stream isp)
        {
            this.isp = isp;
        }

        public static Image GetImage(Uri url)
        {
            Stream stream = null;
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                stream = webRequest.GetResponse().GetResponseStream();
                Image obj = GetImage(stream);
                obj.Url = url;
                return obj;
            }
            finally
            {
                stream?.Close();
            }
        }

        public static Image GetImage(Stream isp)
        {
            return new PngImage(isp).GetImage();
        }

        public static Image GetImage(string file)
        {
            return GetImage(Utilities.ToURL(file));
        }

        public static Image GetImage(byte[] data)
        {
            Image obj = GetImage(new MemoryStream(data));
            obj.OriginalData = data;
            return obj;
        }

        private static bool CheckMarker(string s)
        {
            if (s.Length != 4)
            {
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                char c = s[i];
                if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z'))
                {
                    return false;
                }
            }

            return true;
        }

        private void ReadPng()
        {
            for (int i = 0; i < PNGID.Length; i++)
            {
                if (PNGID[i] != isp.ReadByte())
                {
                    throw new IOException(MessageLocalization.GetComposedMessage("file.is.not.a.valid.png"));
                }
            }

            byte[] buffer = new byte[4096];
            while (true)
            {
                int num = GetInt(isp);
                string @string = GetString(isp);
                if (num < 0 || !CheckMarker(@string))
                {
                    throw new IOException(MessageLocalization.GetComposedMessage("corrupted.png.file"));
                }

                if ("IDAT".Equals(@string))
                {
                    while (num != 0)
                    {
                        int num2 = isp.Read(buffer, 0, Math.Min(num, 4096));
                        if (num2 <= 0)
                        {
                            return;
                        }

                        idat.Write(buffer, 0, num2);
                        num -= num2;
                    }
                }
                else if ("tRNS".Equals(@string))
                {
                    switch (colorType)
                    {
                        case 0:
                            if (num >= 2)
                            {
                                num -= 2;
                                int word = GetWord(isp);
                                if (bitDepth == 16)
                                {
                                    transRedGray = word;
                                    break;
                                }

                                additional.Put(PdfName.MASK, new PdfLiteral("[" + word + " " + word + "]"));
                            }

                            break;
                        case 2:
                            if (num >= 6)
                            {
                                num -= 6;
                                int word2 = GetWord(isp);
                                int word3 = GetWord(isp);
                                int word4 = GetWord(isp);
                                if (bitDepth == 16)
                                {
                                    transRedGray = word2;
                                    transGreen = word3;
                                    transBlue = word4;
                                    break;
                                }

                                additional.Put(PdfName.MASK, new PdfLiteral("[" + word2 + " " + word2 + " " + word3 + " " + word3 + " " + word4 + " " + word4 + "]"));
                            }

                            break;
                        case 3:
                            if (num > 0)
                            {
                                trans = new byte[num];
                                for (int j = 0; j < num; j++)
                                {
                                    trans[j] = (byte)isp.ReadByte();
                                }

                                num = 0;
                            }

                            break;
                    }

                    Utilities.Skip(isp, num);
                }
                else if ("IHDR".Equals(@string))
                {
                    width = GetInt(isp);
                    height = GetInt(isp);
                    bitDepth = isp.ReadByte();
                    colorType = isp.ReadByte();
                    compressionMethod = isp.ReadByte();
                    filterMethod = isp.ReadByte();
                    interlaceMethod = isp.ReadByte();
                }
                else if ("PLTE".Equals(@string))
                {
                    if (colorType == 3)
                    {
                        PdfArray pdfArray = new PdfArray();
                        pdfArray.Add(PdfName.INDEXED);
                        pdfArray.Add(GetColorspace());
                        pdfArray.Add(new PdfNumber(num / 3 - 1));
                        ByteBuffer byteBuffer = new ByteBuffer();
                        while (num-- > 0)
                        {
                            byteBuffer.Append_i(isp.ReadByte());
                        }

                        pdfArray.Add(new PdfString(colorTable = byteBuffer.ToByteArray()));
                        additional.Put(PdfName.COLORSPACE, pdfArray);
                    }
                    else
                    {
                        Utilities.Skip(isp, num);
                    }
                }
                else if ("pHYs".Equals(@string))
                {
                    int @int = GetInt(isp);
                    int int2 = GetInt(isp);
                    if (isp.ReadByte() == 1)
                    {
                        dpiX = (int)((float)@int * 0.0254f + 0.5f);
                        dpiY = (int)((float)int2 * 0.0254f + 0.5f);
                    }
                    else if (int2 != 0)
                    {
                        XYRatio = (float)@int / (float)int2;
                    }
                }
                else if ("cHRM".Equals(@string))
                {
                    xW = (float)GetInt(isp) / 100000f;
                    yW = (float)GetInt(isp) / 100000f;
                    xR = (float)GetInt(isp) / 100000f;
                    yR = (float)GetInt(isp) / 100000f;
                    xG = (float)GetInt(isp) / 100000f;
                    yG = (float)GetInt(isp) / 100000f;
                    xB = (float)GetInt(isp) / 100000f;
                    yB = (float)GetInt(isp) / 100000f;
                    hasCHRM = !(Math.Abs(xW) < 0.0001f) && !(Math.Abs(yW) < 0.0001f) && !(Math.Abs(xR) < 0.0001f) && !(Math.Abs(yR) < 0.0001f) && !(Math.Abs(xG) < 0.0001f) && !(Math.Abs(yG) < 0.0001f) && !(Math.Abs(xB) < 0.0001f) && !(Math.Abs(yB) < 0.0001f);
                }
                else if ("sRGB".Equals(@string))
                {
                    int num3 = isp.ReadByte();
                    intent = intents[num3];
                    gamma = 2.2f;
                    xW = 0.3127f;
                    yW = 0.329f;
                    xR = 0.64f;
                    yR = 0.33f;
                    xG = 0.3f;
                    yG = 0.6f;
                    xB = 0.15f;
                    yB = 0.06f;
                    hasCHRM = true;
                }
                else if ("gAMA".Equals(@string))
                {
                    int int3 = GetInt(isp);
                    if (int3 != 0)
                    {
                        gamma = 100000f / (float)int3;
                        if (!hasCHRM)
                        {
                            xW = 0.3127f;
                            yW = 0.329f;
                            xR = 0.64f;
                            yR = 0.33f;
                            xG = 0.3f;
                            yG = 0.6f;
                            xB = 0.15f;
                            yB = 0.06f;
                            hasCHRM = true;
                        }
                    }
                }
                else if ("iCCP".Equals(@string))
                {
                    do
                    {
                        num--;
                    }
                    while (isp.ReadByte() != 0);
                    isp.ReadByte();
                    num--;
                    byte[] array = new byte[num];
                    int num4 = 0;
                    while (num > 0)
                    {
                        int num5 = isp.Read(array, num4, num);
                        if (num5 < 0)
                        {
                            throw new IOException(MessageLocalization.GetComposedMessage("premature.end.of.file"));
                        }

                        num4 += num5;
                        num -= num5;
                    }

                    byte[] data = PdfReader.FlateDecode(array, strict: true);
                    array = null;
                    try
                    {
                        icc_profile = ICC_Profile.GetInstance(data);
                    }
                    catch
                    {
                        icc_profile = null;
                    }
                }
                else
                {
                    if ("IEND".Equals(@string))
                    {
                        break;
                    }

                    Utilities.Skip(isp, num);
                }

                Utilities.Skip(isp, 4);
            }
        }

        private PdfObject GetColorspace()
        {
            if (icc_profile != null)
            {
                if ((colorType & 2) == 0)
                {
                    return PdfName.DEVICEGRAY;
                }

                return PdfName.DEVICERGB;
            }

            if (gamma == 1f && !hasCHRM)
            {
                if ((colorType & 2) == 0)
                {
                    return PdfName.DEVICEGRAY;
                }

                return PdfName.DEVICERGB;
            }

            PdfArray pdfArray = new PdfArray();
            PdfDictionary pdfDictionary = new PdfDictionary();
            if ((colorType & 2) == 0)
            {
                if (gamma == 1f)
                {
                    return PdfName.DEVICEGRAY;
                }

                pdfArray.Add(PdfName.CALGRAY);
                pdfDictionary.Put(PdfName.GAMMA, new PdfNumber(gamma));
                pdfDictionary.Put(PdfName.WHITEPOINT, new PdfLiteral("[1 1 1]"));
                pdfArray.Add(pdfDictionary);
            }
            else
            {
                PdfObject value = new PdfLiteral("[1 1 1]");
                pdfArray.Add(PdfName.CALRGB);
                if (gamma != 1f)
                {
                    PdfArray pdfArray2 = new PdfArray();
                    PdfNumber obj = new PdfNumber(gamma);
                    pdfArray2.Add(obj);
                    pdfArray2.Add(obj);
                    pdfArray2.Add(obj);
                    pdfDictionary.Put(PdfName.GAMMA, pdfArray2);
                }

                if (hasCHRM)
                {
                    float num = yW * ((xG - xB) * yR - (xR - xB) * yG + (xR - xG) * yB);
                    float num2 = yR * ((xG - xB) * yW - (xW - xB) * yG + (xW - xG) * yB) / num;
                    float num3 = num2 * xR / yR;
                    float num4 = num2 * ((1f - xR) / yR - 1f);
                    float num5 = (0f - yG) * ((xR - xB) * yW - (xW - xB) * yR + (xW - xR) * yB) / num;
                    float num6 = num5 * xG / yG;
                    float num7 = num5 * ((1f - xG) / yG - 1f);
                    float num8 = yB * ((xR - xG) * yW - (xW - xG) * yW + (xW - xR) * yG) / num;
                    float num9 = num8 * xB / yB;
                    float num10 = num8 * ((1f - xB) / yB - 1f);
                    float value2 = num3 + num6 + num9;
                    float value3 = 1f;
                    float value4 = num4 + num7 + num10;
                    value = new PdfArray
                    {
                        new PdfNumber(value2),
                        new PdfNumber(value3),
                        new PdfNumber(value4)
                    };
                    PdfArray pdfArray3 = new PdfArray();
                    pdfArray3.Add(new PdfNumber(num3));
                    pdfArray3.Add(new PdfNumber(num2));
                    pdfArray3.Add(new PdfNumber(num4));
                    pdfArray3.Add(new PdfNumber(num6));
                    pdfArray3.Add(new PdfNumber(num5));
                    pdfArray3.Add(new PdfNumber(num7));
                    pdfArray3.Add(new PdfNumber(num9));
                    pdfArray3.Add(new PdfNumber(num8));
                    pdfArray3.Add(new PdfNumber(num10));
                    pdfDictionary.Put(PdfName.MATRIX, pdfArray3);
                }

                pdfDictionary.Put(PdfName.WHITEPOINT, value);
                pdfArray.Add(pdfDictionary);
            }

            return pdfArray;
        }

        private Image GetImage()
        {
            ReadPng();
            int num = 0;
            int num2 = 0;
            palShades = false;
            if (trans != null)
            {
                for (int i = 0; i < trans.Length; i++)
                {
                    int num3 = trans[i] & 0xFF;
                    if (num3 == 0)
                    {
                        num++;
                        num2 = i;
                    }

                    if (num3 != 0 && num3 != 255)
                    {
                        palShades = true;
                        break;
                    }
                }
            }

            if (((uint)colorType & 4u) != 0)
            {
                palShades = true;
            }

            genBWMask = !palShades && (num > 1 || transRedGray >= 0);
            if (!palShades && !genBWMask && num == 1)
            {
                additional.Put(PdfName.MASK, new PdfLiteral("[" + num2 + " " + num2 + "]"));
            }

            bool flag = interlaceMethod == 1 || bitDepth == 16 || ((uint)colorType & 4u) != 0 || palShades || genBWMask;
            switch (colorType)
            {
                case 0:
                    inputBands = 1;
                    break;
                case 2:
                    inputBands = 3;
                    break;
                case 3:
                    inputBands = 1;
                    break;
                case 4:
                    inputBands = 2;
                    break;
                case 6:
                    inputBands = 4;
                    break;
            }

            if (flag)
            {
                DecodeIdat();
            }

            int num4 = inputBands;
            if (((uint)colorType & 4u) != 0)
            {
                num4--;
            }

            int num5 = bitDepth;
            if (num5 == 16)
            {
                num5 = 8;
            }

            Image image;
            if (this.image != null)
            {
                image = ((colorType != 3) ? Image.GetInstance(width, height, num4, num5, this.image) : new ImgRaw(width, height, num4, num5, this.image));
            }
            else
            {
                image = new ImgRaw(width, height, num4, num5, idat.ToArray());
                image.Deflated = true;
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.BITSPERCOMPONENT, new PdfNumber(bitDepth));
                pdfDictionary.Put(PdfName.PREDICTOR, new PdfNumber(15));
                pdfDictionary.Put(PdfName.COLUMNS, new PdfNumber(width));
                pdfDictionary.Put(PdfName.COLORS, new PdfNumber((colorType == 3 || (colorType & 2) == 0) ? 1 : 3));
                additional.Put(PdfName.DECODEPARMS, pdfDictionary);
            }

            if (additional.Get(PdfName.COLORSPACE) == null)
            {
                additional.Put(PdfName.COLORSPACE, GetColorspace());
            }

            if (intent != null)
            {
                additional.Put(PdfName.INTENT, intent);
            }

            if (additional.Size > 0)
            {
                image.Additional = additional;
            }

            if (icc_profile != null)
            {
                image.TagICC = icc_profile;
            }

            if (palShades)
            {
                Image instance = Image.GetInstance(width, height, 1, 8, smask);
                instance.MakeMask();
                image.ImageMask = instance;
            }

            if (genBWMask)
            {
                Image instance2 = Image.GetInstance(width, height, 1, 1, smask);
                instance2.MakeMask();
                image.ImageMask = instance2;
            }

            image.SetDpi(dpiX, dpiY);
            image.XYRatio = XYRatio;
            image.OriginalType = 2;
            return image;
        }

        private void DecodeIdat()
        {
            int num = bitDepth;
            if (num == 16)
            {
                num = 8;
            }

            int num2 = -1;
            bytesPerPixel = ((bitDepth != 16) ? 1 : 2);
            switch (colorType)
            {
                case 0:
                    num2 = (num * width + 7) / 8 * height;
                    break;
                case 2:
                    num2 = width * 3 * height;
                    bytesPerPixel *= 3;
                    break;
                case 3:
                    if (interlaceMethod == 1)
                    {
                        num2 = (num * width + 7) / 8 * height;
                    }

                    bytesPerPixel = 1;
                    break;
                case 4:
                    num2 = width * height;
                    bytesPerPixel *= 2;
                    break;
                case 6:
                    num2 = width * 3 * height;
                    bytesPerPixel *= 4;
                    break;
            }

            if (num2 >= 0)
            {
                image = new byte[num2];
            }

            if (palShades)
            {
                smask = new byte[width * height];
            }
            else if (genBWMask)
            {
                smask = new byte[(width + 7) / 8 * height];
            }

            idat.Position = 0L;
            dataStream = new ZInflaterInputStream(idat);
            if (interlaceMethod != 1)
            {
                DecodePass(0, 0, 1, 1, width, height);
                return;
            }

            DecodePass(0, 0, 8, 8, (width + 7) / 8, (height + 7) / 8);
            DecodePass(4, 0, 8, 8, (width + 3) / 8, (height + 7) / 8);
            DecodePass(0, 4, 4, 8, (width + 3) / 4, (height + 3) / 8);
            DecodePass(2, 0, 4, 4, (width + 1) / 4, (height + 3) / 4);
            DecodePass(0, 2, 2, 4, (width + 1) / 2, (height + 1) / 4);
            DecodePass(1, 0, 2, 2, width / 2, (height + 1) / 2);
            DecodePass(0, 1, 1, 2, width, height / 2);
        }

        private void DecodePass(int xOffset, int yOffset, int xStep, int yStep, int passWidth, int passHeight)
        {
            if (passWidth == 0 || passHeight == 0)
            {
                return;
            }

            int num = (inputBands * passWidth * bitDepth + 7) / 8;
            byte[] array = new byte[num];
            byte[] array2 = new byte[num];
            int num2 = 0;
            int num3 = yOffset;
            while (num2 < passHeight)
            {
                int num4 = 0;
                try
                {
                    num4 = dataStream.ReadByte();
                    ReadFully(dataStream, array, 0, num);
                }
                catch
                {
                }

                switch (num4)
                {
                    case 1:
                        DecodeSubFilter(array, num, bytesPerPixel);
                        break;
                    case 2:
                        DecodeUpFilter(array, array2, num);
                        break;
                    case 3:
                        DecodeAverageFilter(array, array2, num, bytesPerPixel);
                        break;
                    case 4:
                        DecodePaethFilter(array, array2, num, bytesPerPixel);
                        break;
                    default:
                        throw new Exception(MessageLocalization.GetComposedMessage("png.filter.unknown"));
                    case 0:
                        break;
                }

                ProcessPixels(array, xOffset, xStep, num3, passWidth);
                byte[] array3 = array2;
                array2 = array;
                array = array3;
                num2++;
                num3 += yStep;
            }
        }

        private void ProcessPixels(byte[] curr, int xOffset, int step, int y, int width)
        {
            int[] pixel = GetPixel(curr);
            int num = 0;
            switch (colorType)
            {
                case 0:
                case 3:
                case 4:
                    num = 1;
                    break;
                case 2:
                case 6:
                    num = 3;
                    break;
            }

            if (image != null)
            {
                int num2 = xOffset;
                int bytesPerRow = (num * this.width * ((bitDepth == 16) ? 8 : bitDepth) + 7) / 8;
                for (int i = 0; i < width; i++)
                {
                    SetPixel(image, pixel, inputBands * i, num, num2, y, bitDepth, bytesPerRow);
                    num2 += step;
                }
            }

            if (palShades)
            {
                int num2;
                if (((uint)colorType & 4u) != 0)
                {
                    if (bitDepth == 16)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            int num3 = j * inputBands + num;
                            pixel[num3] = Util.USR(pixel[num3], 8);
                        }
                    }

                    int bytesPerRow2 = this.width;
                    num2 = xOffset;
                    for (int i = 0; i < width; i++)
                    {
                        SetPixel(smask, pixel, inputBands * i + num, 1, num2, y, 8, bytesPerRow2);
                        num2 += step;
                    }

                    return;
                }

                int bytesPerRow3 = this.width;
                int[] array = new int[1];
                num2 = xOffset;
                for (int i = 0; i < width; i++)
                {
                    int num4 = pixel[i];
                    if (num4 < trans.Length)
                    {
                        array[0] = trans[num4];
                    }
                    else
                    {
                        array[0] = 255;
                    }

                    SetPixel(smask, array, 0, 1, num2, y, 8, bytesPerRow3);
                    num2 += step;
                }
            }
            else
            {
                if (!genBWMask)
                {
                    return;
                }

                switch (colorType)
                {
                    case 3:
                        {
                            int bytesPerRow5 = (this.width + 7) / 8;
                            int[] array3 = new int[1];
                            int num2 = xOffset;
                            for (int i = 0; i < width; i++)
                            {
                                int num6 = pixel[i];
                                array3[0] = ((num6 < trans.Length && trans[num6] == 0) ? 1 : 0);
                                SetPixel(smask, array3, 0, 1, num2, y, 1, bytesPerRow5);
                                num2 += step;
                            }

                            break;
                        }
                    case 0:
                        {
                            int bytesPerRow6 = (this.width + 7) / 8;
                            int[] array4 = new int[1];
                            int num2 = xOffset;
                            for (int i = 0; i < width; i++)
                            {
                                int num7 = pixel[i];
                                array4[0] = ((num7 == transRedGray) ? 1 : 0);
                                SetPixel(smask, array4, 0, 1, num2, y, 1, bytesPerRow6);
                                num2 += step;
                            }

                            break;
                        }
                    case 2:
                        {
                            int bytesPerRow4 = (this.width + 7) / 8;
                            int[] array2 = new int[1];
                            int num2 = xOffset;
                            for (int i = 0; i < width; i++)
                            {
                                int num5 = inputBands * i;
                                array2[0] = ((pixel[num5] == transRedGray && pixel[num5 + 1] == transGreen && pixel[num5 + 2] == transBlue) ? 1 : 0);
                                SetPixel(smask, array2, 0, 1, num2, y, 1, bytesPerRow4);
                                num2 += step;
                            }

                            break;
                        }
                    case 1:
                        break;
                }
            }
        }

        private static int GetPixel(byte[] image, int x, int y, int bitDepth, int bytesPerRow)
        {
            if (bitDepth == 8)
            {
                int num = bytesPerRow * y + x;
                return image[num] & 0xFF;
            }

            int num2 = bytesPerRow * y + x / (8 / bitDepth);
            return (image[num2] >> 8 - bitDepth * (x % (8 / bitDepth)) - bitDepth) & ((1 << bitDepth) - 1);
        }

        private static void SetPixel(byte[] image, int[] data, int offset, int size, int x, int y, int bitDepth, int bytesPerRow)
        {
            switch (bitDepth)
            {
                case 8:
                    {
                        int num4 = bytesPerRow * y + size * x;
                        for (int j = 0; j < size; j++)
                        {
                            image[num4 + j] = (byte)data[j + offset];
                        }

                        break;
                    }
                case 16:
                    {
                        int num3 = bytesPerRow * y + size * x;
                        for (int i = 0; i < size; i++)
                        {
                            image[num3 + i] = (byte)(data[i + offset] >> 8);
                        }

                        break;
                    }
                default:
                    {
                        int num = bytesPerRow * y + x / (8 / bitDepth);
                        int num2 = data[offset] << 8 - bitDepth * (x % (8 / bitDepth)) - bitDepth;
                        image[num] |= (byte)num2;
                        break;
                    }
            }
        }

        private int[] GetPixel(byte[] curr)
        {
            switch (bitDepth)
            {
                case 8:
                    {
                        int[] array3 = new int[curr.Length];
                        for (int k = 0; k < array3.Length; k++)
                        {
                            array3[k] = curr[k] & 0xFF;
                        }

                        return array3;
                    }
                case 16:
                    {
                        int[] array2 = new int[curr.Length / 2];
                        for (int j = 0; j < array2.Length; j++)
                        {
                            array2[j] = ((curr[j * 2] & 0xFF) << 8) + (curr[j * 2 + 1] & 0xFF);
                        }

                        return array2;
                    }
                default:
                    {
                        int[] array = new int[curr.Length * 8 / bitDepth];
                        int num = 0;
                        int num2 = 8 / bitDepth;
                        int num3 = (1 << bitDepth) - 1;
                        for (int i = 0; i < curr.Length; i++)
                        {
                            for (int num4 = num2 - 1; num4 >= 0; num4--)
                            {
                                array[num++] = Util.USR(curr[i], bitDepth * num4) & num3;
                            }
                        }

                        return array;
                    }
            }
        }

        private static void DecodeSubFilter(byte[] curr, int count, int bpp)
        {
            for (int i = bpp; i < count; i++)
            {
                int num = curr[i] & 0xFF;
                num += curr[i - bpp] & 0xFF;
                curr[i] = (byte)num;
            }
        }

        private static void DecodeUpFilter(byte[] curr, byte[] prev, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int num = curr[i] & 0xFF;
                int num2 = prev[i] & 0xFF;
                curr[i] = (byte)(num + num2);
            }
        }

        private static void DecodeAverageFilter(byte[] curr, byte[] prev, int count, int bpp)
        {
            for (int i = 0; i < bpp; i++)
            {
                int num = curr[i] & 0xFF;
                int num2 = prev[i] & 0xFF;
                curr[i] = (byte)(num + num2 / 2);
            }

            for (int j = bpp; j < count; j++)
            {
                int num = curr[j] & 0xFF;
                int num3 = curr[j - bpp] & 0xFF;
                int num2 = prev[j] & 0xFF;
                curr[j] = (byte)(num + (num3 + num2) / 2);
            }
        }

        private static int PaethPredictor(int a, int b, int c)
        {
            int num = a + b - c;
            int num2 = Math.Abs(num - a);
            int num3 = Math.Abs(num - b);
            int num4 = Math.Abs(num - c);
            if (num2 <= num3 && num2 <= num4)
            {
                return a;
            }

            if (num3 <= num4)
            {
                return b;
            }

            return c;
        }

        private static void DecodePaethFilter(byte[] curr, byte[] prev, int count, int bpp)
        {
            for (int i = 0; i < bpp; i++)
            {
                int num = curr[i] & 0xFF;
                int num2 = prev[i] & 0xFF;
                curr[i] = (byte)(num + num2);
            }

            for (int j = bpp; j < count; j++)
            {
                int num = curr[j] & 0xFF;
                int a = curr[j - bpp] & 0xFF;
                int num2 = prev[j] & 0xFF;
                int c = prev[j - bpp] & 0xFF;
                curr[j] = (byte)(num + PaethPredictor(a, num2, c));
            }
        }

        public static int GetInt(Stream isp)
        {
            return (isp.ReadByte() << 24) + (isp.ReadByte() << 16) + (isp.ReadByte() << 8) + isp.ReadByte();
        }

        public static int GetWord(Stream isp)
        {
            return (isp.ReadByte() << 8) + isp.ReadByte();
        }

        public static string GetString(Stream isp)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                stringBuilder.Append((char)isp.ReadByte());
            }

            return stringBuilder.ToString();
        }

        private static void ReadFully(Stream inp, byte[] b, int offset, int count)
        {
            while (count > 0)
            {
                int num = inp.Read(b, offset, count);
                if (num <= 0)
                {
                    throw new IOException(MessageLocalization.GetComposedMessage("insufficient.data"));
                }

                count -= num;
                offset += num;
            }
        }
    }
}
