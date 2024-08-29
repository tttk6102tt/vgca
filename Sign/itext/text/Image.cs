using Sign.itext.error_messages;
using Sign.itext.io;
using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.itext.text.api;
using Sign.itext.text.pdf;
using Sign.itext.text.pdf.codec;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Reflection;

namespace Sign.itext.text
{
    public abstract class Image : Rectangle, IIndentable, ISpaceable, IAccessibleElement, IAlternateDescription
    {
        public const int DEFAULT = 0;

        public const int RIGHT_ALIGN = 2;

        public const int LEFT_ALIGN = 0;

        public const int MIDDLE_ALIGN = 1;

        public const int TEXTWRAP = 4;

        public const int UNDERLYING = 8;

        public const int AX = 0;

        public const int AY = 1;

        public const int BX = 2;

        public const int BY = 3;

        public const int CX = 4;

        public const int CY = 5;

        public const int DX = 6;

        public const int DY = 7;

        public const int ORIGINAL_NONE = 0;

        public const int ORIGINAL_JPEG = 1;

        public const int ORIGINAL_PNG = 2;

        public const int ORIGINAL_GIF = 3;

        public const int ORIGINAL_BMP = 4;

        public const int ORIGINAL_TIFF = 5;

        public const int ORIGINAL_WMF = 6;

        public const int ORIGINAL_JPEG2000 = 8;

        public const int ORIGINAL_JBIG2 = 9;

        protected bool invert;

        protected int type;

        protected Uri url;

        protected byte[] rawData;

        protected PdfTemplate[] template = new PdfTemplate[1];

        protected int alignment;

        protected string alt;

        protected float absoluteX = float.NaN;

        protected float absoluteY = float.NaN;

        protected float plainWidth;

        protected float plainHeight;

        protected float scaledWidth;

        protected float scaledHeight;

        protected int compressionLevel = -1;

        protected float rotationRadians;

        protected int colorspace = -1;

        protected int colortransform = 1;

        protected int bpc = 1;

        protected int[] transparency;

        protected float indentationLeft;

        protected float indentationRight;

        protected long mySerialId = GetSerialId();

        private static object serialId = 0L;

        protected PdfName role = PdfName.FIGURE;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected AccessibleElementId id = new AccessibleElementId();

        protected int dpiX;

        protected int dpiY;

        protected bool mask;

        protected Image imageMask;

        protected bool interpolation;

        protected Annotation annotation;

        protected ICC_Profile profile;

        protected bool deflated;

        private PdfDictionary additional;

        private bool smask;

        private float xyRatio;

        protected int originalType;

        protected byte[] originalData;

        protected float spacingBefore;

        protected float spacingAfter;

        private float widthPercentage = 100f;

        protected IPdfOCG layer;

        private float initialRotation;

        private PdfIndirectReference directReference;

        protected internal bool scaleToFitLineWhenOverflow;

        protected bool scaleToFitHeight = true;

        public new float Rotation
        {
            set
            {
                base.Rotation = (int)value;
                double num = Math.PI;
                rotationRadians = (float)((double)(value + initialRotation) % (2.0 * num));
                if (rotationRadians < 0f)
                {
                    rotationRadians += (float)(2.0 * num);
                }

                float[] matrix = GetMatrix();
                scaledWidth = matrix[6] - matrix[4];
                scaledHeight = matrix[7] - matrix[5];
            }
        }

        public virtual float RotationDegrees
        {
            set
            {
                Rotation = value / 180f * (float)Math.PI;
            }
        }

        public virtual Annotation Annotation
        {
            get
            {
                return annotation;
            }
            set
            {
                annotation = value;
            }
        }

        public virtual int Bpc => bpc;

        public virtual byte[] RawData => rawData;

        public virtual PdfTemplate TemplateData
        {
            get
            {
                return template[0];
            }
            set
            {
                template[0] = value;
            }
        }

        public virtual float AbsoluteX => absoluteX;

        public virtual float AbsoluteY => absoluteY;

        public override int Type => type;

        public virtual Uri Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
            }
        }

        public virtual int Alignment
        {
            get
            {
                return alignment;
            }
            set
            {
                alignment = value;
            }
        }

        public virtual string Alt
        {
            get
            {
                return alt;
            }
            set
            {
                alt = value;
                SetAccessibleAttribute(PdfName.ALT, new PdfString(alt));
            }
        }

        public virtual float ScaledWidth => scaledWidth;

        public virtual float ScaledHeight => scaledHeight;

        public virtual int Colorspace => colorspace;

        public virtual int ColorTransform
        {
            get
            {
                return colortransform;
            }
            set
            {
                colortransform = value;
            }
        }

        public virtual int[] Transparency
        {
            get
            {
                return transparency;
            }
            set
            {
                transparency = value;
            }
        }

        public virtual float PlainWidth => plainWidth;

        public virtual float PlainHeight => plainHeight;

        public virtual long MySerialId => mySerialId;

        public virtual int DpiX => dpiX;

        public virtual int DpiY => dpiY;

        public virtual Image ImageMask
        {
            get
            {
                return imageMask;
            }
            set
            {
                if (mask)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("an.image.mask.cannot.contain.another.image.mask"));
                }

                if (!value.mask)
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("the.image.mask.is.not.a.mask.did.you.do.makemask"));
                }

                imageMask = value;
                smask = value.bpc > 1 && value.bpc <= 8;
            }
        }

        public virtual bool Inverted
        {
            get
            {
                return invert;
            }
            set
            {
                invert = value;
            }
        }

        public virtual bool Interpolation
        {
            get
            {
                return interpolation;
            }
            set
            {
                interpolation = value;
            }
        }

        public virtual ICC_Profile TagICC
        {
            get
            {
                return profile;
            }
            set
            {
                profile = value;
            }
        }

        public virtual bool Deflated
        {
            get
            {
                return deflated;
            }
            set
            {
                deflated = value;
            }
        }

        public virtual PdfDictionary Additional
        {
            get
            {
                return additional;
            }
            set
            {
                additional = value;
            }
        }

        public virtual bool Smask
        {
            get
            {
                return smask;
            }
            set
            {
                smask = value;
            }
        }

        public virtual float XYRatio
        {
            get
            {
                return xyRatio;
            }
            set
            {
                xyRatio = value;
            }
        }

        public virtual float IndentationLeft
        {
            get
            {
                return indentationLeft;
            }
            set
            {
                indentationLeft = value;
            }
        }

        public virtual float IndentationRight
        {
            get
            {
                return indentationRight;
            }
            set
            {
                indentationRight = value;
            }
        }

        public virtual int OriginalType
        {
            get
            {
                return originalType;
            }
            set
            {
                originalType = value;
            }
        }

        public virtual byte[] OriginalData
        {
            get
            {
                return originalData;
            }
            set
            {
                originalData = value;
            }
        }

        public virtual float SpacingBefore
        {
            get
            {
                return spacingBefore;
            }
            set
            {
                spacingBefore = value;
            }
        }

        public virtual float SpacingAfter
        {
            get
            {
                return spacingAfter;
            }
            set
            {
                spacingAfter = value;
            }
        }

        public virtual float WidthPercentage
        {
            get
            {
                return widthPercentage;
            }
            set
            {
                widthPercentage = value;
            }
        }

        public virtual bool ScaleToFitLineWhenOverflow
        {
            get
            {
                return scaleToFitLineWhenOverflow;
            }
            set
            {
                scaleToFitLineWhenOverflow = value;
            }
        }

        public virtual bool ScaleToFitHeight
        {
            get
            {
                return scaleToFitHeight;
            }
            set
            {
                scaleToFitHeight = value;
            }
        }

        public virtual IPdfOCG Layer
        {
            get
            {
                return layer;
            }
            set
            {
                layer = value;
            }
        }

        public virtual float InitialRotation
        {
            get
            {
                return initialRotation;
            }
            set
            {
                float num = rotationRadians - initialRotation;
                initialRotation = value;
                Rotation = num;
            }
        }

        public virtual PdfIndirectReference DirectReference
        {
            get
            {
                return directReference;
            }
            set
            {
                directReference = value;
            }
        }

        public virtual int CompressionLevel
        {
            get
            {
                return compressionLevel;
            }
            set
            {
                if (value < 0 || value > 9)
                {
                    compressionLevel = -1;
                }
                else
                {
                    compressionLevel = value;
                }
            }
        }

        public virtual PdfName Role
        {
            get
            {
                return role;
            }
            set
            {
                role = value;
            }
        }

        public virtual AccessibleElementId ID
        {
            get
            {
                if (id == null)
                {
                    id = new AccessibleElementId();
                }

                return id;
            }
            set
            {
                id = value;
            }
        }

        public virtual bool IsInline => true;

        public Image(Uri url)
            : base(0f, 0f)
        {
            this.url = url;
            alignment = 0;
            rotationRadians = 0f;
        }

        public Image(Image image)
            : base(image)
        {
            type = image.type;
            url = image.url;
            rawData = image.rawData;
            bpc = image.bpc;
            template = image.template;
            alignment = image.alignment;
            alt = image.alt;
            absoluteX = image.absoluteX;
            absoluteY = image.absoluteY;
            plainWidth = image.plainWidth;
            plainHeight = image.plainHeight;
            scaledWidth = image.scaledWidth;
            scaledHeight = image.scaledHeight;
            mySerialId = image.mySerialId;
            directReference = image.directReference;
            rotationRadians = image.rotationRadians;
            initialRotation = image.initialRotation;
            indentationLeft = image.indentationLeft;
            indentationRight = image.indentationRight;
            spacingBefore = image.spacingBefore;
            spacingAfter = image.spacingAfter;
            widthPercentage = image.widthPercentage;
            scaleToFitLineWhenOverflow = image.scaleToFitLineWhenOverflow;
            scaleToFitHeight = image.scaleToFitHeight;
            annotation = image.annotation;
            layer = image.layer;
            interpolation = image.interpolation;
            originalType = image.originalType;
            originalData = image.originalData;
            deflated = image.deflated;
            dpiX = image.dpiX;
            dpiY = image.dpiY;
            XYRatio = image.XYRatio;
            colorspace = image.colorspace;
            invert = image.invert;
            profile = image.profile;
            additional = image.additional;
            mask = image.mask;
            imageMask = image.imageMask;
            smask = image.smask;
            transparency = image.transparency;
            role = image.role;
            if (image.accessibleAttributes != null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>(image.accessibleAttributes);
            }

            ID = image.ID;
        }

        public static Image GetInstance(Image image)
        {
            if (image == null)
            {
                return null;
            }

            return (Image)image.GetType().GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(Image) }, null)!.Invoke(new object[1] { image });
        }

        public static Image GetInstance(Uri url)
        {
            return GetInstance(url, recoverFromImageError: false);
        }

        public static Image GetInstance(Uri url, bool recoverFromImageError)
        {
            Stream stream = null;
            RandomAccessSourceFactory randomAccessSourceFactory = new RandomAccessSourceFactory();
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                stream = webRequest.GetResponse().GetResponseStream();
                int num = stream.ReadByte();
                int num2 = stream.ReadByte();
                int num3 = stream.ReadByte();
                int num4 = stream.ReadByte();
                int num5 = stream.ReadByte();
                int num6 = stream.ReadByte();
                int num7 = stream.ReadByte();
                int num8 = stream.ReadByte();
                stream.Close();
                stream = null;
                if (num == 71 && num2 == 73 && num3 == 70)
                {
                    return new GifImage(url).GetImage(1);
                }

                if (num == 255 && num2 == 216)
                {
                    return new Jpeg(url);
                }

                if (num == 0 && num2 == 0 && num3 == 0 && num4 == 12)
                {
                    return new Jpeg2000(url);
                }

                if (num == 255 && num2 == 79 && num3 == 255 && num4 == 81)
                {
                    return new Jpeg2000(url);
                }

                if (num == PngImage.PNGID[0] && num2 == PngImage.PNGID[1] && num3 == PngImage.PNGID[2] && num4 == PngImage.PNGID[3])
                {
                    return PngImage.GetImage(url);
                }

                if (num == 215 && num2 == 205)
                {
                    return new ImgWMF(url);
                }

                if (num == 66 && num2 == 77)
                {
                    return BmpImage.GetImage(url);
                }

                if ((num == 77 && num2 == 77 && num3 == 0 && num4 == 42) || (num == 73 && num2 == 73 && num3 == 42 && num4 == 0))
                {
                    RandomAccessFileOrArray randomAccessFileOrArray = null;
                    try
                    {
                        if (url.IsFile)
                        {
                            string localPath = url.LocalPath;
                            randomAccessFileOrArray = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateBestSource(localPath));
                        }
                        else
                        {
                            randomAccessFileOrArray = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateSource(url));
                        }

                        Image tiffImage = TiffImage.GetTiffImage(randomAccessFileOrArray, 1);
                        tiffImage.url = url;
                        return tiffImage;
                    }
                    catch (Exception ex)
                    {
                        if (recoverFromImageError)
                        {
                            Image tiffImage2 = TiffImage.GetTiffImage(randomAccessFileOrArray, recoverFromImageError, 1);
                            tiffImage2.url = url;
                            return tiffImage2;
                        }

                        throw ex;
                    }
                    finally
                    {
                        randomAccessFileOrArray?.Close();
                    }
                }

                if (num == 151 && num2 == 74 && num3 == 66 && num4 == 50 && num5 == 13 && num6 == 10 && num7 == 26 && num8 == 10)
                {
                    RandomAccessFileOrArray randomAccessFileOrArray2 = null;
                    try
                    {
                        if (url.IsFile)
                        {
                            string localPath2 = url.LocalPath;
                            randomAccessFileOrArray2 = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateBestSource(localPath2));
                        }
                        else
                        {
                            randomAccessFileOrArray2 = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateSource(url));
                        }

                        Image jbig2Image = JBIG2Image.GetJbig2Image(randomAccessFileOrArray2, 1);
                        jbig2Image.url = url;
                        return jbig2Image;
                    }
                    finally
                    {
                        randomAccessFileOrArray2?.Close();
                    }
                }

                throw new IOException(MessageLocalization.GetComposedMessage("unknown.image.format", url.ToString()));
            }
            finally
            {
                stream?.Close();
            }
        }

        public static Image GetInstance(Stream s)
        {
            return GetInstance(StreamUtil.InputStreamToArray(s));
        }

        public static Image GetInstance(string filename, bool recoverFromImageError)
        {
            return GetInstance(Utilities.ToURL(filename), recoverFromImageError);
        }

        public static Image GetInstance(byte[] imgb)
        {
            return GetInstance(imgb, recoverFromImageError: false);
        }

        public static Image GetInstance(byte[] imgb, bool recoverFromImageError)
        {
            RandomAccessSourceFactory randomAccessSourceFactory = new RandomAccessSourceFactory();
            int num = imgb[0];
            int num2 = imgb[1];
            int num3 = imgb[2];
            int num4 = imgb[3];
            if (num == 71 && num2 == 73 && num3 == 70)
            {
                return new GifImage(imgb).GetImage(1);
            }

            if (num == 255 && num2 == 216)
            {
                return new Jpeg(imgb);
            }

            if (num == 0 && num2 == 0 && num3 == 0 && num4 == 12)
            {
                return new Jpeg2000(imgb);
            }

            if (num == 255 && num2 == 79 && num3 == 255 && num4 == 81)
            {
                return new Jpeg2000(imgb);
            }

            if (num == PngImage.PNGID[0] && num2 == PngImage.PNGID[1] && num3 == PngImage.PNGID[2] && num4 == PngImage.PNGID[3])
            {
                return PngImage.GetImage(imgb);
            }

            if (num == 215 && num2 == 205)
            {
                return new ImgWMF(imgb);
            }

            if (num == 66 && num2 == 77)
            {
                return BmpImage.GetImage(imgb);
            }

            if ((num == 77 && num2 == 77 && num3 == 0 && num4 == 42) || (num == 73 && num2 == 73 && num3 == 42 && num4 == 0))
            {
                RandomAccessFileOrArray randomAccessFileOrArray = null;
                try
                {
                    randomAccessFileOrArray = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateSource(imgb));
                    Image tiffImage = TiffImage.GetTiffImage(randomAccessFileOrArray, 1);
                    if (tiffImage.OriginalData == null)
                    {
                        tiffImage.OriginalData = imgb;
                    }

                    return tiffImage;
                }
                catch (Exception ex)
                {
                    if (recoverFromImageError)
                    {
                        Image tiffImage2 = TiffImage.GetTiffImage(randomAccessFileOrArray, recoverFromImageError, 1);
                        if (tiffImage2.OriginalData == null)
                        {
                            tiffImage2.OriginalData = imgb;
                        }

                        return tiffImage2;
                    }

                    throw ex;
                }
                finally
                {
                    randomAccessFileOrArray?.Close();
                }
            }

            if (num == 151 && num2 == 74 && num3 == 66 && num4 == 50)
            {
                byte num5 = imgb[4];
                int num6 = imgb[5];
                int num7 = imgb[6];
                int num8 = imgb[7];
                if (num5 == 13 && num6 == 10 && num7 == 26 && num8 == 10)
                {
                    RandomAccessFileOrArray randomAccessFileOrArray2 = null;
                    try
                    {
                        randomAccessFileOrArray2 = new RandomAccessFileOrArray(randomAccessSourceFactory.CreateSource(imgb));
                        Image jbig2Image = JBIG2Image.GetJbig2Image(randomAccessFileOrArray2, 1);
                        if (jbig2Image.OriginalData == null)
                        {
                            jbig2Image.OriginalData = imgb;
                        }

                        return jbig2Image;
                    }
                    finally
                    {
                        randomAccessFileOrArray2?.Close();
                    }
                }
            }

            throw new IOException(MessageLocalization.GetComposedMessage("the.byte.array.is.not.a.recognized.imageformat"));
        }

        public static Image GetInstance(System.Drawing.Image image, BaseColor color, bool forceBW)
        {
            Bitmap bitmap = (Bitmap)image;
            int width = bitmap.Width;
            int height = bitmap.Height;
            int num = 0;
            if (forceBW)
            {
                byte[] array = new byte[(width / 8 + ((((uint)width & 7u) != 0) ? 1 : 0)) * height];
                int num2 = 0;
                int num3 = 1;
                if (color != null)
                {
                    num3 = ((color.R + color.G + color.B >= 384) ? 1 : 0);
                }

                int[] array2 = null;
                int num4 = 128;
                int num5 = 0;
                int num6 = 0;
                if (color != null)
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            if (bitmap.GetPixel(j, i).A < 250)
                            {
                                if (num3 == 1)
                                {
                                    num6 |= num4;
                                }
                            }
                            else if (((uint)bitmap.GetPixel(j, i).ToArgb() & 0x888u) != 0)
                            {
                                num6 |= num4;
                            }

                            num4 >>= 1;
                            if (num4 == 0 || num5 + 1 >= width)
                            {
                                array[num2++] = (byte)num6;
                                num4 = 128;
                                num6 = 0;
                            }

                            num5++;
                            if (num5 >= width)
                            {
                                num5 = 0;
                            }
                        }
                    }
                }
                else
                {
                    for (int k = 0; k < height; k++)
                    {
                        for (int l = 0; l < width; l++)
                        {
                            if (array2 == null && bitmap.GetPixel(l, k).A == 0)
                            {
                                array2 = new int[2];
                                array2[0] = (array2[1] = ((((uint)bitmap.GetPixel(l, k).ToArgb() & 0x888u) != 0) ? 1 : 0));
                            }

                            if (((uint)bitmap.GetPixel(l, k).ToArgb() & 0x888u) != 0)
                            {
                                num6 |= num4;
                            }

                            num4 >>= 1;
                            if (num4 == 0 || num5 + 1 >= width)
                            {
                                array[num2++] = (byte)num6;
                                num4 = 128;
                                num6 = 0;
                            }

                            num5++;
                            if (num5 >= width)
                            {
                                num5 = 0;
                            }
                        }
                    }
                }

                return GetInstance(width, height, 1, 1, array, array2);
            }

            byte[] array3 = new byte[width * height * 3];
            byte[] array4 = null;
            int num7 = 0;
            int num8 = 255;
            int num9 = 255;
            int num10 = 255;
            if (color != null)
            {
                num8 = color.R;
                num9 = color.G;
                num10 = color.B;
            }

            int[] array5 = null;
            if (color != null)
            {
                for (int m = 0; m < height; m++)
                {
                    for (int n = 0; n < width; n++)
                    {
                        if (((bitmap.GetPixel(n, m).ToArgb() >> 24) & 0xFF) < 250)
                        {
                            array3[num7++] = (byte)num8;
                            array3[num7++] = (byte)num9;
                            array3[num7++] = (byte)num10;
                        }
                        else
                        {
                            num = bitmap.GetPixel(n, m).ToArgb();
                            array3[num7++] = (byte)((uint)(num >> 16) & 0xFFu);
                            array3[num7++] = (byte)((uint)(num >> 8) & 0xFFu);
                            array3[num7++] = (byte)((uint)num & 0xFFu);
                        }
                    }
                }
            }
            else
            {
                int num11 = 0;
                array4 = new byte[width * height];
                bool flag = false;
                int num12 = 0;
                for (int num13 = 0; num13 < height; num13++)
                {
                    for (int num14 = 0; num14 < width; num14++)
                    {
                        num = bitmap.GetPixel(num14, num13).ToArgb();
                        byte b;
                        array4[num12++] = (b = (byte)((uint)(num >> 24) & 0xFFu));
                        byte b2 = b;
                        if (!flag)
                        {
                            if (b2 != 0 && b2 != byte.MaxValue)
                            {
                                flag = true;
                            }
                            else if (array5 == null)
                            {
                                if (b2 == 0)
                                {
                                    num11 = num & 0xFFFFFF;
                                    array5 = new int[6];
                                    array5[0] = (array5[1] = (num11 >> 16) & 0xFF);
                                    array5[2] = (array5[3] = (num11 >> 8) & 0xFF);
                                    array5[4] = (array5[5] = num11 & 0xFF);
                                }
                            }
                            else if ((num & 0xFFFFFF) != num11)
                            {
                                flag = true;
                            }
                        }

                        array3[num7++] = (byte)((uint)(num >> 16) & 0xFFu);
                        array3[num7++] = (byte)((uint)(num >> 8) & 0xFFu);
                        array3[num7++] = (byte)((uint)num & 0xFFu);
                    }
                }

                if (flag)
                {
                    array5 = null;
                }
                else
                {
                    array4 = null;
                }
            }

            Image instance = GetInstance(width, height, 3, 8, array3, array5);
            if (array4 != null)
            {
                Image instance2 = GetInstance(width, height, 1, 8, array4);
                instance2.MakeMask();
                instance.ImageMask = instance2;
            }

            return instance;
        }

        public static Image GetInstance(System.Drawing.Image image, ImageFormat format)
        {
            MemoryStream memoryStream = new MemoryStream();
            image.Save(memoryStream, format);
            return GetInstance(memoryStream.ToArray());
        }

        public static Image GetInstance(System.Drawing.Image image, BaseColor color)
        {
            return GetInstance(image, color, forceBW: false);
        }

        public static Image GetInstance(string filename)
        {
            return GetInstance(Utilities.ToURL(filename));
        }

        public static Image GetInstance(int width, int height, int components, int bpc, byte[] data)
        {
            return GetInstance(width, height, components, bpc, data, null);
        }

        public static Image GetInstance(int width, int height, byte[] data, byte[] globals)
        {
            return new ImgJBIG2(width, height, data, globals);
        }

        public static Image GetInstance(PRIndirectReference iref)
        {
            PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(iref);
            int intValue = ((PdfNumber)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.WIDTH))).IntValue;
            int intValue2 = ((PdfNumber)PdfReader.GetPdfObjectRelease(pdfDictionary.Get(PdfName.HEIGHT))).IntValue;
            Image image = null;
            PdfObject pdfObject = pdfDictionary.Get(PdfName.SMASK);
            if (pdfObject != null && pdfObject.IsIndirect())
            {
                image = GetInstance((PRIndirectReference)pdfObject);
            }
            else
            {
                pdfObject = pdfDictionary.Get(PdfName.MASK);
                if (pdfObject != null && pdfObject.IsIndirect() && PdfReader.GetPdfObjectRelease(pdfObject) is PdfDictionary)
                {
                    image = GetInstance((PRIndirectReference)pdfObject);
                }
            }

            return new ImgRaw(intValue, intValue2, 1, 1, null)
            {
                imageMask = image,
                directReference = iref
            };
        }

        public static Image GetInstance(PdfTemplate template)
        {
            return new ImgTemplate(template);
        }

        public static Image GetInstance(int width, int height, bool reverseBits, int typeCCITT, int parameters, byte[] data)
        {
            return GetInstance(width, height, reverseBits, typeCCITT, parameters, data, null);
        }

        public static Image GetInstance(int width, int height, bool reverseBits, int typeCCITT, int parameters, byte[] data, int[] transparency)
        {
            if (transparency != null && transparency.Length != 2)
            {
                throw new BadElementException(MessageLocalization.GetComposedMessage("transparency.length.must.be.equal.to.2.with.ccitt.images"));
            }

            return new ImgCCITT(width, height, reverseBits, typeCCITT, parameters, data)
            {
                transparency = transparency
            };
        }

        public static Image GetInstance(int width, int height, int components, int bpc, byte[] data, int[] transparency)
        {
            if (transparency != null && transparency.Length != components * 2)
            {
                throw new BadElementException(MessageLocalization.GetComposedMessage("transparency.length.must.be.equal.to.componentes.2"));
            }

            if (components == 1 && bpc == 1)
            {
                byte[] data2 = CCITTG4Encoder.Compress(data, width, height);
                return GetInstance(width, height, reverseBits: false, 256, 1, data2, transparency);
            }

            return new ImgRaw(width, height, components, bpc, data)
            {
                transparency = transparency
            };
        }

        public virtual void SetAbsolutePosition(float absoluteX, float absoluteY)
        {
            this.absoluteX = absoluteX;
            this.absoluteY = absoluteY;
        }

        public virtual void ScaleAbsolute(Rectangle rectangle)
        {
            ScaleAbsolute(rectangle.Width, rectangle.Height);
        }

        public virtual void ScaleAbsolute(float newWidth, float newHeight)
        {
            plainWidth = newWidth;
            plainHeight = newHeight;
            float[] matrix = GetMatrix();
            scaledWidth = matrix[6] - matrix[4];
            scaledHeight = matrix[7] - matrix[5];
            WidthPercentage = 0f;
        }

        public virtual void ScaleAbsoluteWidth(float newWidth)
        {
            plainWidth = newWidth;
            float[] matrix = GetMatrix();
            scaledWidth = matrix[6] - matrix[4];
            scaledHeight = matrix[7] - matrix[5];
            WidthPercentage = 0f;
        }

        public virtual void ScaleAbsoluteHeight(float newHeight)
        {
            plainHeight = newHeight;
            float[] matrix = GetMatrix();
            scaledWidth = matrix[6] - matrix[4];
            scaledHeight = matrix[7] - matrix[5];
            WidthPercentage = 0f;
        }

        public virtual void ScalePercent(float percent)
        {
            ScalePercent(percent, percent);
        }

        public virtual void ScalePercent(float percentX, float percentY)
        {
            plainWidth = Width * percentX / 100f;
            plainHeight = Height * percentY / 100f;
            float[] matrix = GetMatrix();
            scaledWidth = matrix[6] - matrix[4];
            scaledHeight = matrix[7] - matrix[5];
            WidthPercentage = 0f;
        }

        public virtual void ScaleToFit(Rectangle rectangle)
        {
            ScaleToFit(rectangle.Width, rectangle.Height);
        }

        public virtual void ScaleToFit(float fitWidth, float fitHeight)
        {
            ScalePercent(100f);
            float num = fitWidth * 100f / ScaledWidth;
            float num2 = fitHeight * 100f / ScaledHeight;
            ScalePercent((num < num2) ? num : num2);
            WidthPercentage = 0f;
        }

        public virtual float GetImageRotation()
        {
            float num = (float)((double)(rotationRadians - initialRotation) % (Math.PI * 2.0));
            if (num < 0f)
            {
                num += (float)Math.PI * 2f;
            }

            return num;
        }

        public virtual bool HasAbsolutePosition()
        {
            return !float.IsNaN(absoluteY);
        }

        public virtual bool HasAbsoluteX()
        {
            return !float.IsNaN(absoluteX);
        }

        public override bool IsNestable()
        {
            return true;
        }

        public virtual bool IsJpeg()
        {
            return type == 32;
        }

        public virtual bool IsImgRaw()
        {
            return type == 34;
        }

        public virtual bool IsImgTemplate()
        {
            return type == 35;
        }

        public virtual float[] GetMatrix()
        {
            return GetMatrix(1f);
        }

        public virtual float[] GetMatrix(float scalePercentage)
        {
            float[] array = new float[8];
            float num = (float)Math.Cos(rotationRadians);
            float num2 = (float)Math.Sin(rotationRadians);
            array[0] = plainWidth * num * scalePercentage;
            array[1] = plainWidth * num2 * scalePercentage;
            array[2] = (0f - plainHeight) * num2 * scalePercentage;
            array[3] = plainHeight * num * scalePercentage;
            if ((double)rotationRadians < Math.PI / 2.0)
            {
                array[4] = array[2];
                array[5] = 0f;
                array[6] = array[0];
                array[7] = array[1] + array[3];
            }
            else if ((double)rotationRadians < Math.PI)
            {
                array[4] = array[0] + array[2];
                array[5] = array[3];
                array[6] = 0f;
                array[7] = array[1];
            }
            else if ((double)rotationRadians < 4.71238898038469)
            {
                array[4] = array[0];
                array[5] = array[1] + array[3];
                array[6] = array[2];
                array[7] = 0f;
            }
            else
            {
                array[4] = 0f;
                array[5] = array[1];
                array[6] = array[0] + array[2];
                array[7] = array[3];
            }

            return array;
        }

        protected static long GetSerialId()
        {
            lock (serialId)
            {
                serialId = (long)serialId + 1;
                return (long)serialId;
            }
        }

        public virtual void SetDpi(int dpiX, int dpiY)
        {
            this.dpiX = dpiX;
            this.dpiY = dpiY;
        }

        public virtual bool IsMaskCandidate()
        {
            if (type == 34 && bpc > 255)
            {
                return true;
            }

            return colorspace == 1;
        }

        public virtual void MakeMask()
        {
            if (!IsMaskCandidate())
            {
                throw new DocumentException(MessageLocalization.GetComposedMessage("this.image.can.not.be.an.image.mask"));
            }

            mask = true;
        }

        public virtual bool IsMask()
        {
            return mask;
        }

        public virtual bool HasICCProfile()
        {
            return profile != null;
        }

        private PdfObject SimplifyColorspace(PdfArray obj)
        {
            if (obj == null)
            {
                return null;
            }

            PdfObject asName = obj.GetAsName(0);
            if (PdfName.CALGRAY.Equals(asName))
            {
                return PdfName.DEVICEGRAY;
            }

            if (PdfName.CALRGB.Equals(asName))
            {
                return PdfName.DEVICERGB;
            }

            return obj;
        }

        public virtual void SimplifyColorspace()
        {
            if (additional == null)
            {
                return;
            }

            PdfArray asArray = additional.GetAsArray(PdfName.COLORSPACE);
            if (asArray == null)
            {
                return;
            }

            PdfObject pdfObject = SimplifyColorspace(asArray);
            PdfObject value;
            if (pdfObject.IsName())
            {
                value = pdfObject;
            }
            else
            {
                value = asArray;
                PdfName asName = asArray.GetAsName(0);
                if (PdfName.INDEXED.Equals(asName) && asArray.Size >= 2)
                {
                    PdfArray asArray2 = asArray.GetAsArray(1);
                    if (asArray2 != null)
                    {
                        asArray[1] = SimplifyColorspace(asArray2);
                    }
                }
            }

            additional.Put(PdfName.COLORSPACE, value);
        }

        public virtual PdfObject GetAccessibleAttribute(PdfName key)
        {
            if (accessibleAttributes != null)
            {
                accessibleAttributes.TryGetValue(key, out var value);
                return value;
            }

            return null;
        }

        public virtual void SetAccessibleAttribute(PdfName key, PdfObject value)
        {
            if (accessibleAttributes == null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            }

            accessibleAttributes[key] = value;
        }

        public virtual Dictionary<PdfName, PdfObject> GetAccessibleAttributes()
        {
            return accessibleAttributes;
        }
    }
}
