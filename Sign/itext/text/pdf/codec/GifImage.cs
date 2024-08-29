using Sign.itext.error_messages;
using Sign.itext.pdf;
using System.Net;
using System.Text;

namespace Sign.itext.text.pdf.codec
{
    public class GifImage
    {
        internal class GifFrame
        {
            internal Image image;

            internal int ix;

            internal int iy;
        }

        protected Stream inp;

        protected int width;

        protected int height;

        protected bool gctFlag;

        protected int bgIndex;

        protected int bgColor;

        protected int pixelAspect;

        protected bool lctFlag;

        protected bool interlace;

        protected int lctSize;

        protected int ix;

        protected int iy;

        protected int iw;

        protected int ih;

        protected byte[] block = new byte[256];

        protected int blockSize;

        protected int dispose;

        protected bool transparency;

        protected int delay;

        protected int transIndex;

        protected const int MaxStackSize = 4096;

        protected short[] prefix;

        protected byte[] suffix;

        protected byte[] pixelStack;

        protected byte[] pixels;

        protected byte[] m_out;

        protected int m_bpc;

        protected int m_gbpc;

        protected byte[] m_global_table;

        protected byte[] m_local_table;

        protected byte[] m_curr_table;

        protected int m_line_stride;

        protected byte[] fromData;

        protected Uri fromUrl;

        internal List<GifFrame> frames = new List<GifFrame>();

        public GifImage(Uri url)
        {
            fromUrl = url;
            Stream stream = null;
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    using Stream stream2 = webResponse.GetResponseStream();
                    stream = new MemoryStream();
                    byte[] array = new byte[1024];
                    int count;
                    while ((count = stream2.Read(array, 0, array.Length)) > 0)
                    {
                        stream.Write(array, 0, count);
                    }

                    stream.Position = 0L;
                }

                Process(stream);
            }
            finally
            {
                stream?.Close();
            }
        }

        public GifImage(string file)
            : this(Utilities.ToURL(file))
        {
        }

        public GifImage(byte[] data)
        {
            fromData = data;
            Stream stream = null;
            try
            {
                stream = new MemoryStream(data);
                Process(stream);
            }
            finally
            {
                stream?.Close();
            }
        }

        public GifImage(Stream isp)
        {
            Process(isp);
        }

        public virtual int GetFrameCount()
        {
            return frames.Count;
        }

        public virtual Image GetImage(int frame)
        {
            return frames[frame - 1].image;
        }

        public virtual int[] GetFramePosition(int frame)
        {
            GifFrame gifFrame = frames[frame - 1];
            return new int[2] { gifFrame.ix, gifFrame.iy };
        }

        public virtual int[] GetLogicalScreen()
        {
            return new int[2] { width, height };
        }

        internal void Process(Stream isp)
        {
            inp = new BufferedStream(isp);
            ReadHeader();
            ReadContents();
            if (frames.Count == 0)
            {
                throw new IOException(MessageLocalization.GetComposedMessage("the.file.does.not.contain.any.valid.image"));
            }
        }

        protected virtual void ReadHeader()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                stringBuilder.Append((char)inp.ReadByte());
            }

            if (!stringBuilder.ToString().StartsWith("GIF8"))
            {
                throw new IOException(MessageLocalization.GetComposedMessage("gif.signature.nor.found"));
            }

            ReadLSD();
            if (gctFlag)
            {
                m_global_table = ReadColorTable(m_gbpc);
            }
        }

        protected virtual void ReadLSD()
        {
            width = ReadShort();
            height = ReadShort();
            int num = inp.ReadByte();
            gctFlag = (num & 0x80) != 0;
            m_gbpc = (num & 7) + 1;
            bgIndex = inp.ReadByte();
            pixelAspect = inp.ReadByte();
        }

        protected virtual int ReadShort()
        {
            return inp.ReadByte() | (inp.ReadByte() << 8);
        }

        protected virtual int ReadBlock()
        {
            blockSize = inp.ReadByte();
            if (blockSize <= 0)
            {
                return blockSize = 0;
            }

            blockSize = inp.Read(block, 0, blockSize);
            return blockSize;
        }

        protected virtual byte[] ReadColorTable(int bpc)
        {
            int num = 1 << bpc;
            int count = 3 * num;
            bpc = NewBpc(bpc);
            byte[] array = new byte[(1 << bpc) * 3];
            ReadFully(array, 0, count);
            return array;
        }

        protected static int NewBpc(int bpc)
        {
            switch (bpc)
            {
                case 3:
                    return 4;
                default:
                    return 8;
                case 1:
                case 2:
                case 4:
                    return bpc;
            }
        }

        protected virtual void ReadContents()
        {
            bool flag = false;
            while (!flag)
            {
                switch (inp.ReadByte())
                {
                    case 44:
                        ReadImage();
                        break;
                    case 33:
                        switch (inp.ReadByte())
                        {
                            case 249:
                                ReadGraphicControlExt();
                                break;
                            case 255:
                                ReadBlock();
                                Skip();
                                break;
                            default:
                                Skip();
                                break;
                        }

                        break;
                    default:
                        flag = true;
                        break;
                }
            }
        }

        protected virtual void ReadImage()
        {
            ix = ReadShort();
            iy = ReadShort();
            iw = ReadShort();
            ih = ReadShort();
            int num = inp.ReadByte();
            lctFlag = (num & 0x80) != 0;
            interlace = (num & 0x40) != 0;
            lctSize = 2 << (num & 7);
            m_bpc = NewBpc(m_gbpc);
            if (lctFlag)
            {
                m_curr_table = ReadColorTable((num & 7) + 1);
                m_bpc = NewBpc((num & 7) + 1);
            }
            else
            {
                m_curr_table = m_global_table;
            }

            if (transparency && transIndex >= m_curr_table.Length / 3)
            {
                transparency = false;
            }

            if (transparency && m_bpc == 1)
            {
                byte[] array = new byte[12];
                Array.Copy(m_curr_table, 0, array, 0, 6);
                m_curr_table = array;
                m_bpc = 2;
            }

            if (!DecodeImageData())
            {
                Skip();
            }

            Image image = null;
            image = new ImgRaw(iw, ih, 1, m_bpc, m_out);
            PdfArray pdfArray = new PdfArray();
            pdfArray.Add(PdfName.INDEXED);
            pdfArray.Add(PdfName.DEVICERGB);
            int num2 = m_curr_table.Length;
            pdfArray.Add(new PdfNumber(num2 / 3 - 1));
            pdfArray.Add(new PdfString(m_curr_table));
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.COLORSPACE, pdfArray);
            image.Additional = pdfDictionary;
            if (transparency)
            {
                image.Transparency = new int[2] { transIndex, transIndex };
            }

            image.OriginalType = 3;
            image.OriginalData = fromData;
            image.Url = fromUrl;
            GifFrame gifFrame = new GifFrame();
            gifFrame.image = image;
            gifFrame.ix = ix;
            gifFrame.iy = iy;
            frames.Add(gifFrame);
        }

        protected virtual bool DecodeImageData()
        {
            int num = -1;
            int num2 = iw * ih;
            bool result = false;
            if (prefix == null)
            {
                prefix = new short[4096];
            }

            if (suffix == null)
            {
                suffix = new byte[4096];
            }

            if (pixelStack == null)
            {
                pixelStack = new byte[4097];
            }

            m_line_stride = (iw * m_bpc + 7) / 8;
            m_out = new byte[m_line_stride * ih];
            int num3 = 1;
            int num4 = ((!interlace) ? 1 : 8);
            int num5 = 0;
            int num6 = 0;
            int num7 = inp.ReadByte();
            int num8 = 1 << num7;
            int num9 = num8 + 1;
            int num10 = num8 + 2;
            int num11 = num;
            int num12 = num7 + 1;
            int num13 = (1 << num12) - 1;
            for (int i = 0; i < num8; i++)
            {
                prefix[i] = 0;
                suffix[i] = (byte)i;
            }

            int num18;
            int num17;
            int num16;
            int num15;
            int num14;
            int num19 = (num18 = (num17 = (num16 = (num15 = (num14 = 0)))));
            int num20 = 0;
            while (num20 < num2)
            {
                if (num15 == 0)
                {
                    if (num18 < num12)
                    {
                        if (num17 == 0)
                        {
                            num17 = ReadBlock();
                            if (num17 <= 0)
                            {
                                result = true;
                                break;
                            }

                            num14 = 0;
                        }

                        num19 += (block[num14] & 0xFF) << num18;
                        num18 += 8;
                        num14++;
                        num17--;
                        continue;
                    }

                    int i = num19 & num13;
                    num19 >>= num12;
                    num18 -= num12;
                    if (i > num10 || i == num9)
                    {
                        break;
                    }

                    if (i == num8)
                    {
                        num12 = num7 + 1;
                        num13 = (1 << num12) - 1;
                        num10 = num8 + 2;
                        num11 = num;
                        continue;
                    }

                    if (num11 == num)
                    {
                        pixelStack[num15++] = suffix[i];
                        num11 = i;
                        num16 = i;
                        continue;
                    }

                    int num21 = i;
                    if (i == num10)
                    {
                        pixelStack[num15++] = (byte)num16;
                        i = num11;
                    }

                    while (i > num8)
                    {
                        pixelStack[num15++] = suffix[i];
                        i = prefix[i];
                    }

                    num16 = suffix[i] & 0xFF;
                    if (num10 >= 4096)
                    {
                        break;
                    }

                    pixelStack[num15++] = (byte)num16;
                    prefix[num10] = (short)num11;
                    suffix[num10] = (byte)num16;
                    num10++;
                    if ((num10 & num13) == 0 && num10 < 4096)
                    {
                        num12++;
                        num13 += num10;
                    }

                    num11 = num21;
                }

                num15--;
                num20++;
                SetPixel(num6, num5, pixelStack[num15]);
                num6++;
                if (num6 < iw)
                {
                    continue;
                }

                num6 = 0;
                num5 += num4;
                if (num5 < ih)
                {
                    continue;
                }

                if (interlace)
                {
                    do
                    {
                        num3++;
                        switch (num3)
                        {
                            case 2:
                                num5 = 4;
                                break;
                            case 3:
                                num5 = 2;
                                num4 = 4;
                                break;
                            case 4:
                                num5 = 1;
                                num4 = 2;
                                break;
                            default:
                                num5 = ih - 1;
                                num4 = 0;
                                break;
                        }
                    }
                    while (num5 >= ih);
                }
                else
                {
                    num5 = ih - 1;
                    num4 = 0;
                }
            }

            return result;
        }

        protected virtual void SetPixel(int x, int y, int v)
        {
            if (m_bpc == 8)
            {
                int num = x + iw * y;
                m_out[num] = (byte)v;
            }
            else
            {
                int num2 = m_line_stride * y + x / (8 / m_bpc);
                int num3 = v << 8 - m_bpc * (x % (8 / m_bpc)) - m_bpc;
                m_out[num2] |= (byte)num3;
            }
        }

        protected virtual void ResetFrame()
        {
        }

        protected virtual void ReadGraphicControlExt()
        {
            inp.ReadByte();
            int num = inp.ReadByte();
            dispose = (num & 0x1C) >> 2;
            if (dispose == 0)
            {
                dispose = 1;
            }

            transparency = (num & 1) != 0;
            delay = ReadShort() * 10;
            transIndex = inp.ReadByte();
            inp.ReadByte();
        }

        protected virtual void Skip()
        {
            do
            {
                ReadBlock();
            }
            while (blockSize > 0);
        }

        private void ReadFully(byte[] b, int offset, int count)
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
