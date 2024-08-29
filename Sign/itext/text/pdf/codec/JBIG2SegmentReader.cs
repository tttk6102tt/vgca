using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf.codec
{
    public class JBIG2SegmentReader
    {
        public class JBIG2Segment : IComparable<JBIG2Segment>
        {
            public int segmentNumber;

            public long dataLength = -1L;

            public int page = -1;

            public int[] referredToSegmentNumbers;

            public bool[] segmentRetentionFlags;

            public int type = -1;

            public bool deferredNonRetain;

            public int countOfReferredToSegments = -1;

            public byte[] data;

            public byte[] headerData;

            public bool page_association_size;

            public int page_association_offset = -1;

            public JBIG2Segment(int segment_number)
            {
                segmentNumber = segment_number;
            }

            public virtual int CompareTo(JBIG2Segment s)
            {
                return segmentNumber - s.segmentNumber;
            }
        }

        public class JBIG2Page
        {
            public int page;

            private JBIG2SegmentReader sr;

            private SortedDictionary<int, JBIG2Segment> segs = new SortedDictionary<int, JBIG2Segment>();

            public int pageBitmapWidth = -1;

            public int pageBitmapHeight = -1;

            public JBIG2Page(int page, JBIG2SegmentReader sr)
            {
                this.page = page;
                this.sr = sr;
            }

            public virtual byte[] GetData(bool for_embedding)
            {
                MemoryStream memoryStream = new MemoryStream();
                foreach (int key in segs.Keys)
                {
                    JBIG2Segment jBIG2Segment = segs[key];
                    if (for_embedding && (jBIG2Segment.type == 51 || jBIG2Segment.type == 49))
                    {
                        continue;
                    }

                    if (for_embedding)
                    {
                        byte[] array = CopyByteArray(jBIG2Segment.headerData);
                        if (jBIG2Segment.page_association_size)
                        {
                            array[jBIG2Segment.page_association_offset] = 0;
                            array[jBIG2Segment.page_association_offset + 1] = 0;
                            array[jBIG2Segment.page_association_offset + 2] = 0;
                            array[jBIG2Segment.page_association_offset + 3] = 1;
                        }
                        else
                        {
                            array[jBIG2Segment.page_association_offset] = 1;
                        }

                        memoryStream.Write(array, 0, array.Length);
                    }
                    else
                    {
                        memoryStream.Write(jBIG2Segment.headerData, 0, jBIG2Segment.headerData.Length);
                    }

                    memoryStream.Write(jBIG2Segment.data, 0, jBIG2Segment.data.Length);
                }

                memoryStream.Close();
                return memoryStream.ToArray();
            }

            public virtual void AddSegment(JBIG2Segment s)
            {
                segs[s.segmentNumber] = s;
            }
        }

        public const int SYMBOL_DICTIONARY = 0;

        public const int INTERMEDIATE_TEXT_REGION = 4;

        public const int IMMEDIATE_TEXT_REGION = 6;

        public const int IMMEDIATE_LOSSLESS_TEXT_REGION = 7;

        public const int PATTERN_DICTIONARY = 16;

        public const int INTERMEDIATE_HALFTONE_REGION = 20;

        public const int IMMEDIATE_HALFTONE_REGION = 22;

        public const int IMMEDIATE_LOSSLESS_HALFTONE_REGION = 23;

        public const int INTERMEDIATE_GENERIC_REGION = 36;

        public const int IMMEDIATE_GENERIC_REGION = 38;

        public const int IMMEDIATE_LOSSLESS_GENERIC_REGION = 39;

        public const int INTERMEDIATE_GENERIC_REFINEMENT_REGION = 40;

        public const int IMMEDIATE_GENERIC_REFINEMENT_REGION = 42;

        public const int IMMEDIATE_LOSSLESS_GENERIC_REFINEMENT_REGION = 43;

        public const int PAGE_INFORMATION = 48;

        public const int END_OF_PAGE = 49;

        public const int END_OF_STRIPE = 50;

        public const int END_OF_FILE = 51;

        public const int PROFILES = 52;

        public const int TABLES = 53;

        public const int EXTENSION = 62;

        private SortedDictionary<int, JBIG2Segment> segments = new SortedDictionary<int, JBIG2Segment>();

        private SortedDictionary<int, JBIG2Page> pages = new SortedDictionary<int, JBIG2Page>();

        private SortedDictionary<JBIG2Segment, object> globals = new SortedDictionary<JBIG2Segment, object>();

        private RandomAccessFileOrArray ra;

        private bool sequential;

        private bool number_of_pages_known;

        private int number_of_pages = -1;

        private bool read;

        public JBIG2SegmentReader(RandomAccessFileOrArray ra)
        {
            this.ra = ra;
        }

        public static byte[] CopyByteArray(byte[] b)
        {
            byte[] array = new byte[b.Length];
            Array.Copy(b, 0, array, 0, b.Length);
            return array;
        }

        public virtual void Read()
        {
            if (read)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("already.attempted.a.read.on.this.jbig2.file"));
            }

            read = true;
            ReadFileHeader();
            if (sequential)
            {
                do
                {
                    JBIG2Segment jBIG2Segment = ReadHeader();
                    ReadSegment(jBIG2Segment);
                    segments[jBIG2Segment.segmentNumber] = jBIG2Segment;
                }
                while (ra.FilePointer < ra.Length);
                return;
            }

            JBIG2Segment jBIG2Segment2;
            do
            {
                jBIG2Segment2 = ReadHeader();
                segments[jBIG2Segment2.segmentNumber] = jBIG2Segment2;
            }
            while (jBIG2Segment2.type != 51);
            foreach (int key in segments.Keys)
            {
                ReadSegment(segments[key]);
            }
        }

        private void ReadSegment(JBIG2Segment s)
        {
            int pos = (int)ra.FilePointer;
            if (s.dataLength == uint.MaxValue)
            {
                return;
            }

            byte[] array = new byte[(int)s.dataLength];
            ra.Read(array);
            s.data = array;
            if (s.type == 48)
            {
                int pos2 = (int)ra.FilePointer;
                ra.Seek(pos);
                int pageBitmapWidth = ra.ReadInt();
                int pageBitmapHeight = ra.ReadInt();
                ra.Seek(pos2);
                JBIG2Page jBIG2Page = pages[s.page];
                if (jBIG2Page == null)
                {
                    throw new InvalidOperationException(MessageLocalization.GetComposedMessage("referring.to.widht.height.of.page.we.havent.seen.yet.1", s.page));
                }

                jBIG2Page.pageBitmapWidth = pageBitmapWidth;
                jBIG2Page.pageBitmapHeight = pageBitmapHeight;
            }
        }

        private JBIG2Segment ReadHeader()
        {
            int num = (int)ra.FilePointer;
            int num2 = ra.ReadInt();
            JBIG2Segment jBIG2Segment = new JBIG2Segment(num2);
            int num3 = ra.Read();
            bool flag = (jBIG2Segment.deferredNonRetain = (num3 & 0x80) == 128);
            bool flag2 = (num3 & 0x40) == 64;
            int num4 = (jBIG2Segment.type = num3 & 0x3F);
            int num5 = ra.Read();
            int num6 = (num5 & 0xE0) >> 5;
            int[] array = null;
            bool[] array2 = null;
            if (num6 == 7)
            {
                ra.Seek(ra.FilePointer - 1);
                num6 = ra.ReadInt() & 0x1FFFFFFF;
                array2 = new bool[num6 + 1];
                int num7 = 0;
                int num8 = 0;
                do
                {
                    int num9 = num7 % 8;
                    if (num9 == 0)
                    {
                        num8 = ra.Read();
                    }

                    array2[num7] = ((1 << num9) & num8) >> num9 == 1;
                    num7++;
                }
                while (num7 <= num6);
            }
            else if (num6 <= 4)
            {
                array2 = new bool[num6 + 1];
                num5 &= 0x1F;
                for (int i = 0; i <= num6; i++)
                {
                    array2[i] = ((1 << i) & num5) >> i == 1;
                }
            }
            else if (num6 == 5 || num6 == 6)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("count.of.referred.to.segments.had.bad.value.in.header.for.segment.1.starting.at.2", num2, num));
            }

            jBIG2Segment.segmentRetentionFlags = array2;
            jBIG2Segment.countOfReferredToSegments = num6;
            array = new int[num6 + 1];
            for (int j = 1; j <= num6; j++)
            {
                if (num2 <= 256)
                {
                    array[j] = ra.Read();
                }
                else if (num2 <= 65536)
                {
                    array[j] = ra.ReadUnsignedShort();
                }
                else
                {
                    array[j] = (int)ra.ReadUnsignedInt();
                }
            }

            jBIG2Segment.referredToSegmentNumbers = array;
            int page_association_offset = (int)ra.FilePointer - num;
            int num10 = ((!flag2) ? ra.Read() : ra.ReadInt());
            if (num10 < 0)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("page.1.invalid.for.segment.2.starting.at.3", num10, num2, num));
            }

            jBIG2Segment.page = num10;
            jBIG2Segment.page_association_size = flag2;
            jBIG2Segment.page_association_offset = page_association_offset;
            if (num10 > 0 && !pages.ContainsKey(num10))
            {
                pages[num10] = new JBIG2Page(num10, this);
            }

            if (num10 > 0)
            {
                pages[num10].AddSegment(jBIG2Segment);
            }
            else
            {
                globals[jBIG2Segment] = null;
            }

            long num11 = (jBIG2Segment.dataLength = ra.ReadUnsignedInt());
            int num12 = (int)ra.FilePointer;
            ra.Seek(num);
            byte[] array3 = new byte[num12 - num];
            ra.Read(array3);
            jBIG2Segment.headerData = array3;
            return jBIG2Segment;
        }

        private void ReadFileHeader()
        {
            ra.Seek(0);
            byte[] array = new byte[8];
            ra.Read(array);
            byte[] array2 = new byte[8] { 151, 74, 66, 50, 13, 10, 26, 10 };
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != array2[i])
                {
                    throw new InvalidOperationException(MessageLocalization.GetComposedMessage("file.header.idstring.not.good.at.byte.1", i));
                }
            }

            int num = ra.Read();
            sequential = (num & 1) == 1;
            number_of_pages_known = (num & 2) == 0;
            if (((uint)num & 0xFCu) != 0)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("file.header.flags.bits.2.7.not.0"));
            }

            if (number_of_pages_known)
            {
                number_of_pages = ra.ReadInt();
            }
        }

        public virtual int NumberOfPages()
        {
            return pages.Count;
        }

        public virtual int GetPageHeight(int i)
        {
            return pages[i].pageBitmapHeight;
        }

        public virtual int GetPageWidth(int i)
        {
            return pages[i].pageBitmapWidth;
        }

        public virtual JBIG2Page GetPage(int page)
        {
            return pages[page];
        }

        public virtual byte[] GetGlobal(bool for_embedding)
        {
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                foreach (JBIG2Segment key in globals.Keys)
                {
                    if (!for_embedding || (key.type != 51 && key.type != 49))
                    {
                        memoryStream.Write(key.headerData, 0, key.headerData.Length);
                        memoryStream.Write(key.data, 0, key.data.Length);
                    }
                }
            }
            catch
            {
            }

            if (memoryStream.Length <= 0)
            {
                return null;
            }

            return memoryStream.ToArray();
        }

        public override string ToString()
        {
            if (read)
            {
                return "Jbig2SegmentReader: number of pages: " + NumberOfPages();
            }

            return "Jbig2SegmentReader in indeterminate state.";
        }
    }
}
