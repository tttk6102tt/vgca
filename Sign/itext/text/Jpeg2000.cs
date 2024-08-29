using Sign.itext.error_messages;
using System.Net;
using System.Runtime.Serialization;

namespace Sign.itext.text
{
    public class Jpeg2000 : Image
    {
        public class ColorSpecBox : List<int?>
        {
            private byte[] colorProfile;

            public virtual int? GetMeth()
            {
                if (base.Count <= 0)
                {
                    return null;
                }

                return base[0];
            }

            public virtual int? GetPrec()
            {
                if (base.Count <= 1)
                {
                    return null;
                }

                return base[1];
            }

            public virtual int? GetApprox()
            {
                if (base.Count <= 2)
                {
                    return null;
                }

                return base[2];
            }

            public virtual int? GetEnumCs()
            {
                if (base.Count <= 3)
                {
                    return null;
                }

                return base[3];
            }

            public virtual byte[] GetColorProfile()
            {
                return colorProfile;
            }

            internal void SetColorProfile(byte[] colorProfile)
            {
                this.colorProfile = colorProfile;
            }
        }

        private class ZeroBoxSiteException : IOException
        {
            public ZeroBoxSiteException()
            {
            }

            public ZeroBoxSiteException(string message)
                : base(message)
            {
            }

            public ZeroBoxSiteException(string message, int hresult)
                : base(message, hresult)
            {
            }

            public ZeroBoxSiteException(string message, Exception innerException)
                : base(message, innerException)
            {
            }

            protected ZeroBoxSiteException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }

        public const int JP2_JP = 1783636000;

        public const int JP2_IHDR = 1768449138;

        public const int JPIP_JPIP = 1785751920;

        public const int JP2_FTYP = 1718909296;

        public const int JP2_JP2H = 1785737832;

        public const int JP2_COLR = 1668246642;

        public const int JP2_JP2C = 1785737827;

        public const int JP2_URL = 1970433056;

        public const int JP2_DBTL = 1685348972;

        public const int JP2_BPCC = 1651532643;

        public const int JP2_JP2 = 1785737760;

        private Stream inp;

        private int boxLength;

        private int boxType;

        private int numOfComps;

        private List<ColorSpecBox> colorSpecBoxes;

        private bool isJp2;

        private byte[] bpcBoxData;

        private const int ZERO_BOX_SIZE = 2003;

        public Jpeg2000(Image image)
            : base(image)
        {
            if (image is Jpeg2000)
            {
                Jpeg2000 jpeg = (Jpeg2000)image;
                numOfComps = jpeg.numOfComps;
                if (colorSpecBoxes != null)
                {
                    colorSpecBoxes = new List<ColorSpecBox>(jpeg.colorSpecBoxes);
                }

                isJp2 = jpeg.isJp2;
                if (bpcBoxData != null)
                {
                    bpcBoxData = (byte[])jpeg.bpcBoxData.Clone();
                }
            }
        }

        public Jpeg2000(Uri url)
            : base(url)
        {
            ProcessParameters();
        }

        public Jpeg2000(byte[] img)
            : base((Uri)null)
        {
            rawData = img;
            originalData = img;
            ProcessParameters();
        }

        public Jpeg2000(byte[] img, float width, float height)
            : this(img)
        {
            scaledWidth = width;
            scaledHeight = height;
        }

        private int Cio_read(int n)
        {
            int num = 0;
            for (int num2 = n - 1; num2 >= 0; num2--)
            {
                num += inp.ReadByte() << (num2 << 3);
            }

            return num;
        }

        public virtual void Jp2_read_boxhdr()
        {
            boxLength = Cio_read(4);
            boxType = Cio_read(4);
            if (boxLength == 1)
            {
                if (Cio_read(4) != 0)
                {
                    throw new IOException(MessageLocalization.GetComposedMessage("cannot.handle.box.sizes.higher.than.2.32"));
                }

                boxLength = Cio_read(4);
                if (boxLength == 0)
                {
                    throw new ZeroBoxSiteException(MessageLocalization.GetComposedMessage("unsupported.box.size.eq.eq.0"));
                }
            }
            else if (boxLength == 0)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("unsupported.box.size.eq.eq.0"));
            }
        }

        private void ProcessParameters()
        {
            type = 33;
            originalType = 8;
            inp = null;
            try
            {
                if (rawData == null)
                {
                    WebRequest webRequest = WebRequest.Create(url);
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    inp = webRequest.GetResponse().GetResponseStream();
                }
                else
                {
                    inp = new MemoryStream(rawData);
                }

                boxLength = Cio_read(4);
                if (boxLength == 12)
                {
                    isJp2 = true;
                    boxType = Cio_read(4);
                    if (1783636000 != boxType)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("expected.jp.marker"));
                    }

                    if (218793738 != Cio_read(4))
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("error.with.jp.marker"));
                    }

                    Jp2_read_boxhdr();
                    if (1718909296 != boxType)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("expected.ftyp.marker"));
                    }

                    Utilities.Skip(inp, boxLength - 8);
                    Jp2_read_boxhdr();
                    do
                    {
                        if (1785737832 != boxType)
                        {
                            if (boxType == 1785737827)
                            {
                                throw new IOException(MessageLocalization.GetComposedMessage("expected.jp2h.marker"));
                            }

                            Utilities.Skip(inp, boxLength - 8);
                            Jp2_read_boxhdr();
                        }
                    }
                    while (1785737832 != boxType);
                    Jp2_read_boxhdr();
                    if (1768449138 != boxType)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("expected.ihdr.marker"));
                    }

                    scaledHeight = Cio_read(4);
                    Top = scaledHeight;
                    scaledWidth = Cio_read(4);
                    Right = scaledWidth;
                    numOfComps = Cio_read(2);
                    bpc = -1;
                    bpc = Cio_read(1);
                    Utilities.Skip(inp, 3);
                    Jp2_read_boxhdr();
                    if (boxType == 1651532643)
                    {
                        bpcBoxData = new byte[boxLength - 8];
                        inp.Read(bpcBoxData, 0, boxLength - 8);
                    }
                    else if (boxType == 1668246642)
                    {
                        do
                        {
                            if (colorSpecBoxes == null)
                            {
                                colorSpecBoxes = new List<ColorSpecBox>();
                            }

                            colorSpecBoxes.Add(Jp2_read_colr());
                            try
                            {
                                Jp2_read_boxhdr();
                            }
                            catch (ZeroBoxSiteException)
                            {
                            }
                        }
                        while (1668246642 == boxType);
                    }
                }
                else
                {
                    if (boxLength != -11534511)
                    {
                        throw new IOException(MessageLocalization.GetComposedMessage("not.a.valid.jpeg2000.file"));
                    }

                    Utilities.Skip(inp, 4);
                    int num = Cio_read(4);
                    int num2 = Cio_read(4);
                    int num3 = Cio_read(4);
                    int num4 = Cio_read(4);
                    Utilities.Skip(inp, 16);
                    colorspace = Cio_read(2);
                    bpc = 8;
                    scaledHeight = num2 - num4;
                    Top = scaledHeight;
                    scaledWidth = num - num3;
                    Right = scaledWidth;
                }
            }
            finally
            {
                if (inp != null)
                {
                    try
                    {
                        inp.Close();
                    }
                    catch
                    {
                    }

                    inp = null;
                }
            }

            plainWidth = Width;
            plainHeight = Height;
        }

        private ColorSpecBox Jp2_read_colr()
        {
            int num = 8;
            ColorSpecBox colorSpecBox = new ColorSpecBox();
            for (int i = 0; i < 3; i++)
            {
                colorSpecBox.Add(Cio_read(1));
                num++;
            }

            if (colorSpecBox.GetMeth() == 1)
            {
                colorSpecBox.Add(Cio_read(4));
                num += 4;
            }
            else
            {
                colorSpecBox.Add(0);
            }

            if (boxLength - num > 0)
            {
                byte[] array = new byte[boxLength - num];
                inp.Read(array, 0, boxLength - num);
                colorSpecBox.SetColorProfile(array);
            }

            return colorSpecBox;
        }

        public virtual int GetNumOfComps()
        {
            return numOfComps;
        }

        public virtual byte[] GetBpcBoxData()
        {
            return bpcBoxData;
        }

        public virtual List<ColorSpecBox> GetColorSpecBoxes()
        {
            return colorSpecBoxes;
        }

        public virtual bool IsJp2()
        {
            return isJp2;
        }
    }
}
