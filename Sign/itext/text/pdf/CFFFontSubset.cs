using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class CFFFontSubset : CFFFont
    {
        internal static string[] SubrsFunctions = new string[32]
        {
            "RESERVED_0", "hstem", "RESERVED_2", "vstem", "vmoveto", "rlineto", "hlineto", "vlineto", "rrcurveto", "RESERVED_9",
            "callsubr", "return", "escape", "RESERVED_13", "endchar", "RESERVED_15", "RESERVED_16", "RESERVED_17", "hstemhm", "hintmask",
            "cntrmask", "rmoveto", "hmoveto", "vstemhm", "rcurveline", "rlinecurve", "vvcurveto", "hhcurveto", "shortint", "callgsubr",
            "vhcurveto", "hvcurveto"
        };

        internal static string[] SubrsEscapeFuncs = new string[39]
        {
            "RESERVED_0", "RESERVED_1", "RESERVED_2", "and", "or", "not", "RESERVED_6", "RESERVED_7", "RESERVED_8", "abs",
            "add", "sub", "div", "RESERVED_13", "neg", "eq", "RESERVED_16", "RESERVED_17", "drop", "RESERVED_19",
            "put", "get", "ifelse", "random", "mul", "RESERVED_25", "sqrt", "dup", "exch", "index",
            "roll", "RESERVED_31", "RESERVED_32", "RESERVED_33", "hflex", "flex", "hflex1", "flex1", "RESERVED_REST"
        };

        internal const byte ENDCHAR_OP = 14;

        internal const byte RETURN_OP = 11;

        internal Dictionary<int, int[]> GlyphsUsed;

        internal List<int> glyphsInList;

        internal Dictionary<int, object> FDArrayUsed = new Dictionary<int, object>();

        internal Dictionary<int, int[]>[] hSubrsUsed;

        internal List<int>[] lSubrsUsed;

        internal Dictionary<int, int[]> hGSubrsUsed = new Dictionary<int, int[]>();

        internal List<int> lGSubrsUsed = new List<int>();

        internal Dictionary<int, int[]> hSubrsUsedNonCID = new Dictionary<int, int[]>();

        internal List<int> lSubrsUsedNonCID = new List<int>();

        internal byte[][] NewLSubrsIndex;

        internal byte[] NewSubrsIndexNonCID;

        internal byte[] NewGSubrsIndex;

        internal byte[] NewCharStringsIndex;

        internal int GBias;

        internal List<Item> OutputList;

        internal int NumOfHints;

        public CFFFontSubset(RandomAccessFileOrArray rf, Dictionary<int, int[]> GlyphsUsed)
            : base(rf)
        {
            this.GlyphsUsed = GlyphsUsed;
            glyphsInList = new List<int>(GlyphsUsed.Keys);
            for (int i = 0; i < fonts.Length; i++)
            {
                Seek(fonts[i].charstringsOffset);
                fonts[i].nglyphs = GetCard16();
                Seek(stringIndexOffset);
                fonts[i].nstrings = GetCard16() + CFFFont.standardStrings.Length;
                fonts[i].charstringsOffsets = GetIndex(fonts[i].charstringsOffset);
                if (fonts[i].fdselectOffset >= 0)
                {
                    ReadFDSelect(i);
                    BuildFDArrayUsed(i);
                }

                if (fonts[i].isCID)
                {
                    ReadFDArray(i);
                }

                fonts[i].CharsetLength = CountCharset(fonts[i].charsetOffset, fonts[i].nglyphs);
            }
        }

        internal int CountCharset(int Offset, int NumofGlyphs)
        {
            int result = 0;
            Seek(Offset);
            switch (GetCard8())
            {
                case '\0':
                    result = 1 + 2 * NumofGlyphs;
                    break;
                case '\u0001':
                    result = 1 + 3 * CountRange(NumofGlyphs, 1);
                    break;
                case '\u0002':
                    result = 1 + 4 * CountRange(NumofGlyphs, 2);
                    break;
            }

            return result;
        }

        private int CountRange(int NumofGlyphs, int Type)
        {
            int num = 0;
            int num2;
            for (int i = 1; i < NumofGlyphs; i += num2 + 1)
            {
                num++;
                GetCard16();
                num2 = ((Type != 1) ? GetCard16() : GetCard8());
            }

            return num;
        }

        protected virtual void ReadFDSelect(int Font)
        {
            int nglyphs = fonts[Font].nglyphs;
            int[] array = new int[nglyphs];
            Seek(fonts[Font].fdselectOffset);
            fonts[Font].FDSelectFormat = GetCard8();
            switch (fonts[Font].FDSelectFormat)
            {
                case 0:
                    {
                        for (int k = 0; k < nglyphs; k++)
                        {
                            array[k] = GetCard8();
                        }

                        fonts[Font].FDSelectLength = fonts[Font].nglyphs + 1;
                        break;
                    }
                case 3:
                    {
                        int card = GetCard16();
                        int num = 0;
                        int num2 = GetCard16();
                        for (int i = 0; i < card; i++)
                        {
                            int card2 = GetCard8();
                            int card3 = GetCard16();
                            int num3 = card3 - num2;
                            for (int j = 0; j < num3; j++)
                            {
                                array[num] = card2;
                                num++;
                            }

                            num2 = card3;
                        }

                        fonts[Font].FDSelectLength = 3 + card * 3 + 2;
                        break;
                    }
            }

            fonts[Font].FDSelect = array;
        }

        protected virtual void BuildFDArrayUsed(int Font)
        {
            int[] fDSelect = fonts[Font].FDSelect;
            for (int i = 0; i < glyphsInList.Count; i++)
            {
                int num = glyphsInList[i];
                int num2 = fDSelect[num];
                FDArrayUsed[num2] = null;
            }
        }

        protected virtual void ReadFDArray(int Font)
        {
            Seek(fonts[Font].fdarrayOffset);
            fonts[Font].FDArrayCount = GetCard16();
            fonts[Font].FDArrayOffsize = GetCard8();
            if (fonts[Font].FDArrayOffsize < 4)
            {
                fonts[Font].FDArrayOffsize++;
            }

            fonts[Font].FDArrayOffsets = GetIndex(fonts[Font].fdarrayOffset);
        }

        public virtual byte[] Process(string fontName)
        {
            try
            {
                buf.ReOpen();
                int i;
                for (i = 0; i < fonts.Length && !fontName.Equals(fonts[i].name); i++)
                {
                }

                if (i == fonts.Length)
                {
                    return null;
                }

                if (gsubrIndexOffset >= 0)
                {
                    GBias = CalcBias(gsubrIndexOffset, i);
                }

                BuildNewCharString(i);
                BuildNewLGSubrs(i);
                return BuildNewFile(i);
            }
            finally
            {
                try
                {
                    buf.Close();
                }
                catch
                {
                }
            }
        }

        protected virtual int CalcBias(int Offset, int Font)
        {
            Seek(Offset);
            int card = GetCard16();
            if (fonts[Font].CharstringType == 1)
            {
                return 0;
            }

            if (card < 1240)
            {
                return 107;
            }

            if (card < 33900)
            {
                return 1131;
            }

            return 32768;
        }

        protected virtual void BuildNewCharString(int FontIndex)
        {
            NewCharStringsIndex = BuildNewIndex(fonts[FontIndex].charstringsOffsets, GlyphsUsed, 14);
        }

        protected virtual void BuildNewLGSubrs(int Font)
        {
            if (fonts[Font].isCID)
            {
                hSubrsUsed = new Dictionary<int, int[]>[fonts[Font].fdprivateOffsets.Length];
                lSubrsUsed = new List<int>[fonts[Font].fdprivateOffsets.Length];
                NewLSubrsIndex = new byte[fonts[Font].fdprivateOffsets.Length][];
                fonts[Font].PrivateSubrsOffset = new int[fonts[Font].fdprivateOffsets.Length];
                fonts[Font].PrivateSubrsOffsetsArray = new int[fonts[Font].fdprivateOffsets.Length][];
                List<int> list = new List<int>(FDArrayUsed.Keys);
                for (int i = 0; i < list.Count; i++)
                {
                    int num = list[i];
                    hSubrsUsed[num] = new Dictionary<int, int[]>();
                    lSubrsUsed[num] = new List<int>();
                    BuildFDSubrsOffsets(Font, num);
                    if (fonts[Font].PrivateSubrsOffset[num] >= 0)
                    {
                        BuildSubrUsed(Font, num, fonts[Font].PrivateSubrsOffset[num], fonts[Font].PrivateSubrsOffsetsArray[num], hSubrsUsed[num], lSubrsUsed[num]);
                        NewLSubrsIndex[num] = BuildNewIndex(fonts[Font].PrivateSubrsOffsetsArray[num], hSubrsUsed[num], 11);
                    }
                }
            }
            else if (fonts[Font].privateSubrs >= 0)
            {
                fonts[Font].SubrsOffsets = GetIndex(fonts[Font].privateSubrs);
                BuildSubrUsed(Font, -1, fonts[Font].privateSubrs, fonts[Font].SubrsOffsets, hSubrsUsedNonCID, lSubrsUsedNonCID);
            }

            BuildGSubrsUsed(Font);
            if (fonts[Font].privateSubrs >= 0)
            {
                NewSubrsIndexNonCID = BuildNewIndex(fonts[Font].SubrsOffsets, hSubrsUsedNonCID, 11);
            }

            NewGSubrsIndex = BuildNewIndex(gsubrOffsets, hGSubrsUsed, 11);
        }

        protected virtual void BuildFDSubrsOffsets(int Font, int FD)
        {
            fonts[Font].PrivateSubrsOffset[FD] = -1;
            Seek(fonts[Font].fdprivateOffsets[FD]);
            while (GetPosition() < fonts[Font].fdprivateOffsets[FD] + fonts[Font].fdprivateLengths[FD])
            {
                GetDictItem();
                if (key == "Subrs")
                {
                    fonts[Font].PrivateSubrsOffset[FD] = (int)args[0] + fonts[Font].fdprivateOffsets[FD];
                }
            }

            if (fonts[Font].PrivateSubrsOffset[FD] >= 0)
            {
                fonts[Font].PrivateSubrsOffsetsArray[FD] = GetIndex(fonts[Font].PrivateSubrsOffset[FD]);
            }
        }

        protected virtual void BuildSubrUsed(int Font, int FD, int SubrOffset, int[] SubrsOffsets, Dictionary<int, int[]> hSubr, List<int> lSubr)
        {
            int lBias = CalcBias(SubrOffset, Font);
            for (int i = 0; i < glyphsInList.Count; i++)
            {
                int num = glyphsInList[i];
                int begin = fonts[Font].charstringsOffsets[num];
                int end = fonts[Font].charstringsOffsets[num + 1];
                if (FD >= 0)
                {
                    EmptyStack();
                    NumOfHints = 0;
                    if (fonts[Font].FDSelect[num] == FD)
                    {
                        ReadASubr(begin, end, GBias, lBias, hSubr, lSubr, SubrsOffsets);
                    }
                }
                else
                {
                    ReadASubr(begin, end, GBias, lBias, hSubr, lSubr, SubrsOffsets);
                }
            }

            for (int j = 0; j < lSubr.Count; j++)
            {
                int num2 = lSubr[j];
                if (num2 < SubrsOffsets.Length - 1 && num2 >= 0)
                {
                    int begin2 = SubrsOffsets[num2];
                    int end2 = SubrsOffsets[num2 + 1];
                    ReadASubr(begin2, end2, GBias, lBias, hSubr, lSubr, SubrsOffsets);
                }
            }
        }

        protected virtual void BuildGSubrsUsed(int Font)
        {
            int lBias = 0;
            int num = 0;
            if (fonts[Font].privateSubrs >= 0)
            {
                lBias = CalcBias(fonts[Font].privateSubrs, Font);
                num = lSubrsUsedNonCID.Count;
            }

            for (int i = 0; i < lGSubrsUsed.Count; i++)
            {
                int num2 = lGSubrsUsed[i];
                if (num2 >= gsubrOffsets.Length - 1 || num2 < 0)
                {
                    continue;
                }

                int begin = gsubrOffsets[num2];
                int end = gsubrOffsets[num2 + 1];
                if (fonts[Font].isCID)
                {
                    ReadASubr(begin, end, GBias, 0, hGSubrsUsed, lGSubrsUsed, null);
                    continue;
                }

                ReadASubr(begin, end, GBias, lBias, hSubrsUsedNonCID, lSubrsUsedNonCID, fonts[Font].SubrsOffsets);
                if (num >= lSubrsUsedNonCID.Count)
                {
                    continue;
                }

                for (int j = num; j < lSubrsUsedNonCID.Count; j++)
                {
                    int num3 = lSubrsUsedNonCID[j];
                    if (num3 < fonts[Font].SubrsOffsets.Length - 1 && num3 >= 0)
                    {
                        int begin2 = fonts[Font].SubrsOffsets[num3];
                        int end2 = fonts[Font].SubrsOffsets[num3 + 1];
                        ReadASubr(begin2, end2, GBias, lBias, hSubrsUsedNonCID, lSubrsUsedNonCID, fonts[Font].SubrsOffsets);
                    }
                }

                num = lSubrsUsedNonCID.Count;
            }
        }

        protected virtual void ReadASubr(int begin, int end, int GBias, int LBias, Dictionary<int, int[]> hSubr, List<int> lSubr, int[] LSubrsOffsets)
        {
            EmptyStack();
            NumOfHints = 0;
            Seek(begin);
            while (GetPosition() < end)
            {
                ReadCommand();
                int position = GetPosition();
                object obj = null;
                if (arg_count > 0)
                {
                    obj = args[arg_count - 1];
                }

                int num = arg_count;
                HandelStack();
                if (key == "callsubr")
                {
                    if (num > 0)
                    {
                        int num2 = (int)obj + LBias;
                        if (!hSubr.ContainsKey(num2))
                        {
                            hSubr[num2] = null;
                            lSubr.Add(num2);
                        }

                        CalcHints(LSubrsOffsets[num2], LSubrsOffsets[num2 + 1], LBias, GBias, LSubrsOffsets);
                        Seek(position);
                    }
                }
                else if (key == "callgsubr")
                {
                    if (num > 0)
                    {
                        int num3 = (int)obj + GBias;
                        if (!hGSubrsUsed.ContainsKey(num3))
                        {
                            hGSubrsUsed[num3] = null;
                            lGSubrsUsed.Add(num3);
                        }

                        CalcHints(gsubrOffsets[num3], gsubrOffsets[num3 + 1], LBias, GBias, LSubrsOffsets);
                        Seek(position);
                    }
                }
                else if (key == "hstem" || key == "vstem" || key == "hstemhm" || key == "vstemhm")
                {
                    NumOfHints += num / 2;
                }
                else if (key == "hintmask" || key == "cntrmask")
                {
                    int num4 = NumOfHints / 8;
                    if (NumOfHints % 8 != 0 || num4 == 0)
                    {
                        num4++;
                    }

                    for (int i = 0; i < num4; i++)
                    {
                        GetCard8();
                    }
                }
            }
        }

        protected virtual void HandelStack()
        {
            int num = StackOpp();
            if (num < 2)
            {
                if (num == 1)
                {
                    PushStack();
                    return;
                }

                num *= -1;
                for (int i = 0; i < num; i++)
                {
                    PopStack();
                }
            }
            else
            {
                EmptyStack();
            }
        }

        protected virtual int StackOpp()
        {
            if (key == "ifelse")
            {
                return -3;
            }

            if (key == "roll" || key == "put")
            {
                return -2;
            }

            if (key == "callsubr" || key == "callgsubr" || key == "add" || key == "sub" || key == "div" || key == "mul" || key == "drop" || key == "and" || key == "or" || key == "eq")
            {
                return -1;
            }

            if (key == "abs" || key == "neg" || key == "sqrt" || key == "exch" || key == "index" || key == "get" || key == "not" || key == "return")
            {
                return 0;
            }

            if (key == "random" || key == "dup")
            {
                return 1;
            }

            return 2;
        }

        protected virtual void EmptyStack()
        {
            for (int i = 0; i < arg_count; i++)
            {
                args[i] = null;
            }

            arg_count = 0;
        }

        protected virtual void PopStack()
        {
            if (arg_count > 0)
            {
                args[arg_count - 1] = null;
                arg_count--;
            }
        }

        protected virtual void PushStack()
        {
            arg_count++;
        }

        protected virtual void ReadCommand()
        {
            key = null;
            bool flag = false;
            while (!flag)
            {
                char card = GetCard8();
                if (card == '\u001c')
                {
                    int card2 = GetCard8();
                    int card3 = GetCard8();
                    args[arg_count] = (card2 << 8) | card3;
                    arg_count++;
                }
                else if (card >= ' ' && card <= 'ö')
                {
                    args[arg_count] = card - 139;
                    arg_count++;
                }
                else if (card >= '÷' && card <= 'ú')
                {
                    int card4 = GetCard8();
                    args[arg_count] = (card - 247) * 256 + card4 + 108;
                    arg_count++;
                }
                else if (card >= 'û' && card <= 'þ')
                {
                    int card5 = GetCard8();
                    args[arg_count] = -(card - 251) * 256 - card5 - 108;
                    arg_count++;
                }
                else if (card == 'ÿ')
                {
                    int card6 = GetCard8();
                    int card7 = GetCard8();
                    int card8 = GetCard8();
                    int card9 = GetCard8();
                    args[arg_count] = (card6 << 24) | (card7 << 16) | (card8 << 8) | card9;
                    arg_count++;
                }
                else
                {
                    if (card > '\u001f' || card == '\u001c')
                    {
                        continue;
                    }

                    flag = true;
                    if (card == '\f')
                    {
                        int num = GetCard8();
                        if (num > SubrsEscapeFuncs.Length - 1)
                        {
                            num = SubrsEscapeFuncs.Length - 1;
                        }

                        key = SubrsEscapeFuncs[num];
                    }
                    else
                    {
                        key = SubrsFunctions[(uint)card];
                    }
                }
            }
        }

        protected virtual int CalcHints(int begin, int end, int LBias, int GBias, int[] LSubrsOffsets)
        {
            Seek(begin);
            while (GetPosition() < end)
            {
                ReadCommand();
                int position = GetPosition();
                object obj = null;
                if (arg_count > 0)
                {
                    obj = args[arg_count - 1];
                }

                int num = arg_count;
                HandelStack();
                if (key == "callsubr")
                {
                    if (num > 0)
                    {
                        int num2 = (int)obj + LBias;
                        CalcHints(LSubrsOffsets[num2], LSubrsOffsets[num2 + 1], LBias, GBias, LSubrsOffsets);
                        Seek(position);
                    }
                }
                else if (key == "callgsubr")
                {
                    if (num > 0)
                    {
                        int num3 = (int)obj + GBias;
                        CalcHints(gsubrOffsets[num3], gsubrOffsets[num3 + 1], LBias, GBias, LSubrsOffsets);
                        Seek(position);
                    }
                }
                else if (key == "hstem" || key == "vstem" || key == "hstemhm" || key == "vstemhm")
                {
                    NumOfHints += num / 2;
                }
                else if (key == "hintmask" || key == "cntrmask")
                {
                    int num4 = NumOfHints / 8;
                    if (NumOfHints % 8 != 0 || num4 == 0)
                    {
                        num4++;
                    }

                    for (int i = 0; i < num4; i++)
                    {
                        GetCard8();
                    }
                }
            }

            return NumOfHints;
        }

        protected virtual byte[] BuildNewIndex(int[] Offsets, Dictionary<int, int[]> Used, byte OperatorForUnusedEntries)
        {
            int num = 0;
            int num2 = 0;
            int[] array = new int[Offsets.Length];
            for (int i = 0; i < Offsets.Length; i++)
            {
                array[i] = num2;
                if (Used.ContainsKey(i))
                {
                    num2 += Offsets[i + 1] - Offsets[i];
                }
                else
                {
                    num++;
                }
            }

            byte[] array2 = new byte[num2 + num];
            int num3 = 0;
            for (int j = 0; j < Offsets.Length - 1; j++)
            {
                int num4 = array[j];
                int num5 = array[j + 1];
                array[j] = num4 + num3;
                if (num4 != num5)
                {
                    buf.Seek(Offsets[j]);
                    buf.ReadFully(array2, num4 + num3, num5 - num4);
                }
                else
                {
                    array2[num4 + num3] = OperatorForUnusedEntries;
                    num3++;
                }
            }

            array[Offsets.Length - 1] += num3;
            return AssembleIndex(array, array2);
        }

        protected virtual byte[] AssembleIndex(int[] NewOffsets, byte[] NewObjects)
        {
            char c = (char)(NewOffsets.Length - 1);
            int num = NewOffsets[^1];
            byte b = (byte)((num <= 255) ? 1 : ((num <= 65535) ? 2 : ((num > 16777215) ? 4 : 3)));
            byte[] array = new byte[3 + b * (c + 1) + NewObjects.Length];
            int num2 = 0;
            array[num2++] = (byte)((uint)((int)c >> 8) & 0xFFu);
            array[num2++] = (byte)(c & 0xFFu);
            array[num2++] = b;
            for (int i = 0; i < NewOffsets.Length; i++)
            {
                int num3 = NewOffsets[i] - NewOffsets[0] + 1;
                switch (b)
                {
                    case 4:
                        array[num2++] = (byte)((uint)(num3 >> 24) & 0xFFu);
                        goto case 3;
                    case 3:
                        array[num2++] = (byte)((uint)(num3 >> 16) & 0xFFu);
                        goto case 2;
                    case 2:
                        array[num2++] = (byte)((uint)(num3 >> 8) & 0xFFu);
                        break;
                    case 1:
                        break;
                    default:
                        continue;
                }

                array[num2++] = (byte)((uint)num3 & 0xFFu);
            }

            foreach (byte b2 in NewObjects)
            {
                array[num2++] = b2;
            }

            return array;
        }

        protected virtual byte[] BuildNewFile(int Font)
        {
            OutputList = new List<Item>();
            CopyHeader();
            BuildIndexHeader(1, 1, 1);
            OutputList.Add(new UInt8Item((char)(1 + fonts[Font].name.Length)));
            OutputList.Add(new StringItem(fonts[Font].name));
            BuildIndexHeader(1, 2, 1);
            OffsetItem offsetItem = new IndexOffsetItem(2);
            OutputList.Add(offsetItem);
            IndexBaseItem indexBaseItem = new IndexBaseItem();
            OutputList.Add(indexBaseItem);
            OffsetItem offsetItem2 = new DictOffsetItem();
            OffsetItem offsetItem3 = new DictOffsetItem();
            OffsetItem offsetItem4 = new DictOffsetItem();
            OffsetItem offsetItem5 = new DictOffsetItem();
            OffsetItem offsetItem6 = new DictOffsetItem();
            if (!fonts[Font].isCID)
            {
                OutputList.Add(new DictNumberItem(fonts[Font].nstrings));
                OutputList.Add(new DictNumberItem(fonts[Font].nstrings + 1));
                OutputList.Add(new DictNumberItem(0));
                OutputList.Add(new UInt8Item('\f'));
                OutputList.Add(new UInt8Item('\u001e'));
                OutputList.Add(new DictNumberItem(fonts[Font].nglyphs));
                OutputList.Add(new UInt8Item('\f'));
                OutputList.Add(new UInt8Item('"'));
            }

            Seek(topdictOffsets[Font]);
            while (GetPosition() < topdictOffsets[Font + 1])
            {
                int position = GetPosition();
                GetDictItem();
                int position2 = GetPosition();
                if (!(key == "Encoding") && !(key == "Private") && !(key == "FDSelect") && !(key == "FDArray") && !(key == "charset") && !(key == "CharStrings"))
                {
                    OutputList.Add(new RangeItem(buf, position, position2 - position));
                }
            }

            CreateKeys(offsetItem4, offsetItem5, offsetItem2, offsetItem3);
            OutputList.Add(new IndexMarkerItem(offsetItem, indexBaseItem));
            if (fonts[Font].isCID)
            {
                OutputList.Add(GetEntireIndexRange(stringIndexOffset));
            }
            else
            {
                CreateNewStringIndex(Font);
            }

            OutputList.Add(new RangeItem(new RandomAccessFileOrArray(NewGSubrsIndex), 0, NewGSubrsIndex.Length));
            if (fonts[Font].isCID)
            {
                OutputList.Add(new MarkerItem(offsetItem5));
                if (fonts[Font].fdselectOffset >= 0)
                {
                    OutputList.Add(new RangeItem(buf, fonts[Font].fdselectOffset, fonts[Font].FDSelectLength));
                }
                else
                {
                    CreateFDSelect(offsetItem5, fonts[Font].nglyphs);
                }

                OutputList.Add(new MarkerItem(offsetItem2));
                OutputList.Add(new RangeItem(buf, fonts[Font].charsetOffset, fonts[Font].CharsetLength));
                if (fonts[Font].fdarrayOffset >= 0)
                {
                    OutputList.Add(new MarkerItem(offsetItem4));
                    Reconstruct(Font);
                }
                else
                {
                    CreateFDArray(offsetItem4, offsetItem6, Font);
                }
            }
            else
            {
                CreateFDSelect(offsetItem5, fonts[Font].nglyphs);
                CreateCharset(offsetItem2, fonts[Font].nglyphs);
                CreateFDArray(offsetItem4, offsetItem6, Font);
            }

            if (fonts[Font].privateOffset >= 0)
            {
                IndexBaseItem indexBaseItem2 = new IndexBaseItem();
                OutputList.Add(indexBaseItem2);
                OutputList.Add(new MarkerItem(offsetItem6));
                OffsetItem offsetItem7 = new DictOffsetItem();
                CreateNonCIDPrivate(Font, offsetItem7);
                CreateNonCIDSubrs(Font, indexBaseItem2, offsetItem7);
            }

            OutputList.Add(new MarkerItem(offsetItem3));
            OutputList.Add(new RangeItem(new RandomAccessFileOrArray(NewCharStringsIndex), 0, NewCharStringsIndex.Length));
            int[] array = new int[1] { 0 };
            foreach (Item output in OutputList)
            {
                output.Increment(array);
            }

            foreach (Item output2 in OutputList)
            {
                output2.Xref();
            }

            byte[] array2 = new byte[array[0]];
            foreach (Item output3 in OutputList)
            {
                output3.Emit(array2);
            }

            return array2;
        }

        protected virtual void CopyHeader()
        {
            Seek(0);
            GetCard8();
            GetCard8();
            int card = GetCard8();
            GetCard8();
            nextIndexOffset = card;
            OutputList.Add(new RangeItem(buf, 0, card));
        }

        protected virtual void BuildIndexHeader(int Count, int Offsize, int First)
        {
            OutputList.Add(new UInt16Item((char)Count));
            OutputList.Add(new UInt8Item((char)Offsize));
            switch (Offsize)
            {
                case 1:
                    OutputList.Add(new UInt8Item((char)First));
                    break;
                case 2:
                    OutputList.Add(new UInt16Item((char)First));
                    break;
                case 3:
                    OutputList.Add(new UInt24Item((ushort)First));
                    break;
                case 4:
                    OutputList.Add(new UInt32Item((ushort)First));
                    break;
            }
        }

        protected virtual void CreateKeys(OffsetItem fdarrayRef, OffsetItem fdselectRef, OffsetItem charsetRef, OffsetItem charstringsRef)
        {
            OutputList.Add(fdarrayRef);
            OutputList.Add(new UInt8Item('\f'));
            OutputList.Add(new UInt8Item('$'));
            OutputList.Add(fdselectRef);
            OutputList.Add(new UInt8Item('\f'));
            OutputList.Add(new UInt8Item('%'));
            OutputList.Add(charsetRef);
            OutputList.Add(new UInt8Item('\u000f'));
            OutputList.Add(charstringsRef);
            OutputList.Add(new UInt8Item('\u0011'));
        }

        protected virtual void CreateNewStringIndex(int Font)
        {
            string text = fonts[Font].name + "-OneRange";
            if (text.Length > 127)
            {
                text = text.Substring(0, 127);
            }

            string text2 = "AdobeIdentity" + text;
            int num = stringOffsets[stringOffsets.Length - 1] - stringOffsets[0];
            int num2 = stringOffsets[0] - 1;
            byte b = (byte)((num + text2.Length <= 255) ? 1 : ((num + text2.Length <= 65535) ? 2 : ((num + text2.Length > 16777215) ? 4 : 3)));
            OutputList.Add(new UInt16Item((char)(stringOffsets.Length - 1 + 3)));
            OutputList.Add(new UInt8Item((char)b));
            int[] array = stringOffsets;
            foreach (int num3 in array)
            {
                OutputList.Add(new IndexOffsetItem(b, num3 - num2));
            }

            int num4 = stringOffsets[stringOffsets.Length - 1] - num2;
            num4 += "Adobe".Length;
            OutputList.Add(new IndexOffsetItem(b, num4));
            num4 += "Identity".Length;
            OutputList.Add(new IndexOffsetItem(b, num4));
            num4 += text.Length;
            OutputList.Add(new IndexOffsetItem(b, num4));
            OutputList.Add(new RangeItem(buf, stringOffsets[0], num));
            OutputList.Add(new StringItem(text2));
        }

        protected virtual void CreateFDSelect(OffsetItem fdselectRef, int nglyphs)
        {
            OutputList.Add(new MarkerItem(fdselectRef));
            OutputList.Add(new UInt8Item('\u0003'));
            OutputList.Add(new UInt16Item('\u0001'));
            OutputList.Add(new UInt16Item('\0'));
            OutputList.Add(new UInt8Item('\0'));
            OutputList.Add(new UInt16Item((char)nglyphs));
        }

        protected virtual void CreateCharset(OffsetItem charsetRef, int nglyphs)
        {
            OutputList.Add(new MarkerItem(charsetRef));
            OutputList.Add(new UInt8Item('\u0002'));
            OutputList.Add(new UInt16Item('\u0001'));
            OutputList.Add(new UInt16Item((char)(nglyphs - 1)));
        }

        protected virtual void CreateFDArray(OffsetItem fdarrayRef, OffsetItem privateRef, int Font)
        {
            OutputList.Add(new MarkerItem(fdarrayRef));
            BuildIndexHeader(1, 1, 1);
            OffsetItem offsetItem = new IndexOffsetItem(1);
            OutputList.Add(offsetItem);
            IndexBaseItem indexBaseItem = new IndexBaseItem();
            OutputList.Add(indexBaseItem);
            int num = fonts[Font].privateLength;
            int num2 = CalcSubrOffsetSize(fonts[Font].privateOffset, fonts[Font].privateLength);
            if (num2 != 0)
            {
                num += 5 - num2;
            }

            OutputList.Add(new DictNumberItem(num));
            OutputList.Add(privateRef);
            OutputList.Add(new UInt8Item('\u0012'));
            OutputList.Add(new IndexMarkerItem(offsetItem, indexBaseItem));
        }

        private void Reconstruct(int Font)
        {
            OffsetItem[] fdPrivate = new DictOffsetItem[fonts[Font].FDArrayOffsets.Length - 1];
            IndexBaseItem[] fdPrivateBase = new IndexBaseItem[fonts[Font].fdprivateOffsets.Length];
            OffsetItem[] fdSubrs = new DictOffsetItem[fonts[Font].fdprivateOffsets.Length];
            ReconstructFDArray(Font, fdPrivate);
            ReconstructPrivateDict(Font, fdPrivate, fdPrivateBase, fdSubrs);
            ReconstructPrivateSubrs(Font, fdPrivateBase, fdSubrs);
        }

        private void ReconstructFDArray(int Font, OffsetItem[] fdPrivate)
        {
            BuildIndexHeader(fonts[Font].FDArrayCount, fonts[Font].FDArrayOffsize, 1);
            OffsetItem[] array = new IndexOffsetItem[fonts[Font].FDArrayOffsets.Length - 1];
            for (int i = 0; i < fonts[Font].FDArrayOffsets.Length - 1; i++)
            {
                array[i] = new IndexOffsetItem(fonts[Font].FDArrayOffsize);
                OutputList.Add(array[i]);
            }

            IndexBaseItem indexBaseItem = new IndexBaseItem();
            OutputList.Add(indexBaseItem);
            for (int j = 0; j < fonts[Font].FDArrayOffsets.Length - 1; j++)
            {
                Seek(fonts[Font].FDArrayOffsets[j]);
                while (GetPosition() < fonts[Font].FDArrayOffsets[j + 1])
                {
                    int position = GetPosition();
                    GetDictItem();
                    int position2 = GetPosition();
                    if (key == "Private")
                    {
                        int num = (int)args[0];
                        int num2 = CalcSubrOffsetSize(fonts[Font].fdprivateOffsets[j], fonts[Font].fdprivateLengths[j]);
                        if (num2 != 0)
                        {
                            num += 5 - num2;
                        }

                        OutputList.Add(new DictNumberItem(num));
                        fdPrivate[j] = new DictOffsetItem();
                        OutputList.Add(fdPrivate[j]);
                        OutputList.Add(new UInt8Item('\u0012'));
                        Seek(position2);
                    }
                    else
                    {
                        OutputList.Add(new RangeItem(buf, position, position2 - position));
                    }
                }

                OutputList.Add(new IndexMarkerItem(array[j], indexBaseItem));
            }
        }

        internal void ReconstructPrivateDict(int Font, OffsetItem[] fdPrivate, IndexBaseItem[] fdPrivateBase, OffsetItem[] fdSubrs)
        {
            for (int i = 0; i < fonts[Font].fdprivateOffsets.Length; i++)
            {
                OutputList.Add(new MarkerItem(fdPrivate[i]));
                fdPrivateBase[i] = new IndexBaseItem();
                OutputList.Add(fdPrivateBase[i]);
                Seek(fonts[Font].fdprivateOffsets[i]);
                while (GetPosition() < fonts[Font].fdprivateOffsets[i] + fonts[Font].fdprivateLengths[i])
                {
                    int position = GetPosition();
                    GetDictItem();
                    int position2 = GetPosition();
                    if (key == "Subrs")
                    {
                        fdSubrs[i] = new DictOffsetItem();
                        OutputList.Add(fdSubrs[i]);
                        OutputList.Add(new UInt8Item('\u0013'));
                    }
                    else
                    {
                        OutputList.Add(new RangeItem(buf, position, position2 - position));
                    }
                }
            }
        }

        internal void ReconstructPrivateSubrs(int Font, IndexBaseItem[] fdPrivateBase, OffsetItem[] fdSubrs)
        {
            for (int i = 0; i < fonts[Font].fdprivateLengths.Length; i++)
            {
                if (fdSubrs[i] != null && fonts[Font].PrivateSubrsOffset[i] >= 0)
                {
                    OutputList.Add(new SubrMarkerItem(fdSubrs[i], fdPrivateBase[i]));
                    if (NewLSubrsIndex[i] != null)
                    {
                        OutputList.Add(new RangeItem(new RandomAccessFileOrArray(NewLSubrsIndex[i]), 0, NewLSubrsIndex[i].Length));
                    }
                }
            }
        }

        internal int CalcSubrOffsetSize(int Offset, int Size)
        {
            int result = 0;
            Seek(Offset);
            while (GetPosition() < Offset + Size)
            {
                int position = GetPosition();
                GetDictItem();
                int position2 = GetPosition();
                if (key == "Subrs")
                {
                    result = position2 - position - 1;
                }
            }

            return result;
        }

        protected virtual int CountEntireIndexRange(int indexOffset)
        {
            Seek(indexOffset);
            int card = GetCard16();
            if (card == 0)
            {
                return 2;
            }

            int card2 = GetCard8();
            Seek(indexOffset + 2 + 1 + card * card2);
            int num = GetOffset(card2) - 1;
            return 3 + (card + 1) * card2 + num;
        }

        internal void CreateNonCIDPrivate(int Font, OffsetItem Subr)
        {
            Seek(fonts[Font].privateOffset);
            while (GetPosition() < fonts[Font].privateOffset + fonts[Font].privateLength)
            {
                int position = GetPosition();
                GetDictItem();
                int position2 = GetPosition();
                if (key == "Subrs")
                {
                    OutputList.Add(Subr);
                    OutputList.Add(new UInt8Item('\u0013'));
                }
                else
                {
                    OutputList.Add(new RangeItem(buf, position, position2 - position));
                }
            }
        }

        internal void CreateNonCIDSubrs(int Font, IndexBaseItem PrivateBase, OffsetItem Subrs)
        {
            OutputList.Add(new SubrMarkerItem(Subrs, PrivateBase));
            if (NewSubrsIndexNonCID != null)
            {
                OutputList.Add(new RangeItem(new RandomAccessFileOrArray(NewSubrsIndexNonCID), 0, NewSubrsIndexNonCID.Length));
            }
        }
    }
}
