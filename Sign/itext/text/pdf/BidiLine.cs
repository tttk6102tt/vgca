using Sign.itext.pdf;
using Sign.itext.pdf.draw;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class BidiLine
    {
        private const int pieceSizeStart = 256;

        protected int runDirection;

        protected int pieceSize = 256;

        protected char[] text = new char[256];

        protected PdfChunk[] detailChunks = new PdfChunk[256];

        protected int totalTextLength;

        protected byte[] orderLevels = new byte[256];

        protected int[] indexChars = new int[256];

        protected internal List<PdfChunk> chunks = new List<PdfChunk>();

        protected int indexChunk;

        protected int indexChunkChar;

        protected int currentChar;

        protected int storedRunDirection;

        protected char[] storedText = new char[0];

        protected PdfChunk[] storedDetailChunks = new PdfChunk[0];

        protected int storedTotalTextLength;

        protected byte[] storedOrderLevels = new byte[0];

        protected int[] storedIndexChars = new int[0];

        protected int storedIndexChunk;

        protected int storedIndexChunkChar;

        protected int storedCurrentChar;

        protected bool isWordSplit;

        protected bool shortStore;

        protected static IntHashtable mirrorChars;

        protected int arabicOptions;

        public BidiLine()
        {
        }

        public BidiLine(BidiLine org)
        {
            runDirection = org.runDirection;
            pieceSize = org.pieceSize;
            text = (char[])org.text.Clone();
            detailChunks = (PdfChunk[])org.detailChunks.Clone();
            totalTextLength = org.totalTextLength;
            orderLevels = (byte[])org.orderLevels.Clone();
            indexChars = (int[])org.indexChars.Clone();
            chunks = new List<PdfChunk>(org.chunks);
            indexChunk = org.indexChunk;
            indexChunkChar = org.indexChunkChar;
            currentChar = org.currentChar;
            storedRunDirection = org.storedRunDirection;
            storedText = (char[])org.storedText.Clone();
            storedDetailChunks = (PdfChunk[])org.storedDetailChunks.Clone();
            storedTotalTextLength = org.storedTotalTextLength;
            storedOrderLevels = (byte[])org.storedOrderLevels.Clone();
            storedIndexChars = (int[])org.storedIndexChars.Clone();
            storedIndexChunk = org.storedIndexChunk;
            storedIndexChunkChar = org.storedIndexChunkChar;
            storedCurrentChar = org.storedCurrentChar;
            shortStore = org.shortStore;
            arabicOptions = org.arabicOptions;
        }

        public virtual bool IsEmpty()
        {
            if (currentChar >= totalTextLength)
            {
                return indexChunk >= chunks.Count;
            }

            return false;
        }

        public virtual void ClearChunks()
        {
            chunks.Clear();
            totalTextLength = 0;
            currentChar = 0;
        }

        public virtual bool GetParagraph(int runDirection)
        {
            this.runDirection = runDirection;
            currentChar = 0;
            totalTextLength = 0;
            bool flag = false;
            while (indexChunk < chunks.Count)
            {
                PdfChunk pdfChunk = chunks[indexChunk];
                BaseFont font = pdfChunk.Font.Font;
                string text = pdfChunk.ToString();
                int length = text.Length;
                while (indexChunkChar < length)
                {
                    char c = text[indexChunkChar];
                    char c2 = (char)font.GetUnicodeEquivalent(c);
                    if (c2 == '\r' || c2 == '\n')
                    {
                        if (c2 == '\r' && indexChunkChar + 1 < length && text[indexChunkChar + 1] == '\n')
                        {
                            indexChunkChar++;
                        }

                        indexChunkChar++;
                        if (indexChunkChar >= length)
                        {
                            indexChunkChar = 0;
                            indexChunk++;
                        }

                        flag = true;
                        if (totalTextLength == 0)
                        {
                            detailChunks[0] = pdfChunk;
                        }

                        break;
                    }

                    AddPiece(c, pdfChunk);
                    indexChunkChar++;
                }

                if (flag)
                {
                    break;
                }

                indexChunkChar = 0;
                indexChunk++;
            }

            if (totalTextLength == 0)
            {
                return flag;
            }

            totalTextLength = TrimRight(0, totalTextLength - 1) + 1;
            if (totalTextLength == 0)
            {
                return true;
            }

            if (runDirection == 2 || runDirection == 3)
            {
                if (orderLevels.Length < totalTextLength)
                {
                    orderLevels = new byte[pieceSize];
                    indexChars = new int[pieceSize];
                }

                ArabicLigaturizer.ProcessNumbers(this.text, 0, totalTextLength, arabicOptions);
                byte[] levels = new BidiOrder(this.text, 0, totalTextLength, (sbyte)((runDirection == 3) ? 1 : 0)).GetLevels();
                for (int i = 0; i < totalTextLength; i++)
                {
                    orderLevels[i] = levels[i];
                    indexChars[i] = i;
                }

                DoArabicShapping();
                MirrorGlyphs();
            }

            totalTextLength = TrimRightEx(0, totalTextLength - 1) + 1;
            return true;
        }

        public virtual void AddChunk(PdfChunk chunk)
        {
            chunks.Add(chunk);
        }

        public virtual void AddChunks(List<PdfChunk> chunks)
        {
            this.chunks.AddRange(chunks);
        }

        public virtual void AddPiece(char c, PdfChunk chunk)
        {
            if (totalTextLength >= pieceSize)
            {
                char[] sourceArray = text;
                PdfChunk[] sourceArray2 = detailChunks;
                pieceSize *= 2;
                text = new char[pieceSize];
                detailChunks = new PdfChunk[pieceSize];
                Array.Copy(sourceArray, 0, text, 0, totalTextLength);
                Array.Copy(sourceArray2, 0, detailChunks, 0, totalTextLength);
            }

            text[totalTextLength] = c;
            detailChunks[totalTextLength++] = chunk;
        }

        public virtual void Save()
        {
            if (indexChunk > 0)
            {
                if (indexChunk >= chunks.Count)
                {
                    chunks.Clear();
                }
                else
                {
                    indexChunk--;
                    while (indexChunk >= 0)
                    {
                        chunks.RemoveAt(indexChunk);
                        indexChunk--;
                    }
                }

                indexChunk = 0;
            }

            storedRunDirection = runDirection;
            storedTotalTextLength = totalTextLength;
            storedIndexChunk = indexChunk;
            storedIndexChunkChar = indexChunkChar;
            storedCurrentChar = currentChar;
            shortStore = currentChar < totalTextLength;
            if (!shortStore)
            {
                if (storedText.Length < totalTextLength)
                {
                    storedText = new char[totalTextLength];
                    storedDetailChunks = new PdfChunk[totalTextLength];
                }

                Array.Copy(text, 0, storedText, 0, totalTextLength);
                Array.Copy(detailChunks, 0, storedDetailChunks, 0, totalTextLength);
            }

            if (runDirection == 2 || runDirection == 3)
            {
                if (storedOrderLevels.Length < totalTextLength)
                {
                    storedOrderLevels = new byte[totalTextLength];
                    storedIndexChars = new int[totalTextLength];
                }

                Array.Copy(orderLevels, currentChar, storedOrderLevels, currentChar, totalTextLength - currentChar);
                Array.Copy(indexChars, currentChar, storedIndexChars, currentChar, totalTextLength - currentChar);
            }
        }

        public virtual void Restore()
        {
            runDirection = storedRunDirection;
            totalTextLength = storedTotalTextLength;
            indexChunk = storedIndexChunk;
            indexChunkChar = storedIndexChunkChar;
            currentChar = storedCurrentChar;
            if (!shortStore)
            {
                Array.Copy(storedText, 0, text, 0, totalTextLength);
                Array.Copy(storedDetailChunks, 0, detailChunks, 0, totalTextLength);
            }

            if (runDirection == 2 || runDirection == 3)
            {
                Array.Copy(storedOrderLevels, currentChar, orderLevels, currentChar, totalTextLength - currentChar);
                Array.Copy(storedIndexChars, currentChar, indexChars, currentChar, totalTextLength - currentChar);
            }
        }

        public virtual void MirrorGlyphs()
        {
            for (int i = 0; i < totalTextLength; i++)
            {
                if ((orderLevels[i] & 1) == 1)
                {
                    int num = mirrorChars[text[i]];
                    if (num != 0)
                    {
                        text[i] = (char)num;
                    }
                }
            }
        }

        public virtual void DoArabicShapping()
        {
            int i = 0;
            int num = 0;
            while (true)
            {
                if (i < totalTextLength)
                {
                    char c = text[i];
                    if (c < '\u0600' || c > 'ۿ')
                    {
                        if (i != num)
                        {
                            text[num] = text[i];
                            detailChunks[num] = detailChunks[i];
                            orderLevels[num] = orderLevels[i];
                        }

                        i++;
                        num++;
                        continue;
                    }
                }

                if (i >= totalTextLength)
                {
                    break;
                }

                int num2 = i;
                for (i++; i < totalTextLength; i++)
                {
                    char c2 = text[i];
                    if (c2 < '\u0600' || c2 > 'ۿ')
                    {
                        break;
                    }
                }

                int num3 = i - num2;
                int num4 = ArabicLigaturizer.Arabic_shape(text, num2, num3, text, num, num3, arabicOptions);
                if (num2 != num)
                {
                    for (int j = 0; j < num4; j++)
                    {
                        detailChunks[num] = detailChunks[num2];
                        orderLevels[num++] = orderLevels[num2++];
                    }
                }
                else
                {
                    num += num4;
                }
            }

            totalTextLength = num;
        }

        public virtual PdfLine ProcessLine(float leftX, float width, int alignment, int runDirection, int arabicOptions, float minY, float yLine, float descender)
        {
            isWordSplit = false;
            this.arabicOptions = arabicOptions;
            Save();
            bool isRTL = runDirection == 3;
            if (currentChar >= totalTextLength)
            {
                if (!GetParagraph(runDirection))
                {
                    return null;
                }

                if (totalTextLength == 0)
                {
                    List<PdfChunk> list = new List<PdfChunk>();
                    PdfChunk item = new PdfChunk("", detailChunks[0]);
                    list.Add(item);
                    return new PdfLine(0f, 0f, width, alignment, newlineSplit: true, list, isRTL);
                }
            }

            float num = width;
            int num2 = -1;
            if (currentChar != 0)
            {
                currentChar = TrimLeftEx(currentChar, totalTextLength - 1);
            }

            int num3 = currentChar;
            int num4 = 0;
            PdfChunk pdfChunk = null;
            float num5 = 0f;
            PdfChunk pdfChunk2 = null;
            TabStop tabStop = null;
            float num6 = float.NaN;
            float num7 = float.NaN;
            bool flag = false;
            while (currentChar < totalTextLength)
            {
                pdfChunk = detailChunks[currentChar];
                if (pdfChunk.IsImage() && minY < yLine)
                {
                    Image image = pdfChunk.Image;
                    if (image.ScaleToFitHeight && yLine + 2f * descender - image.ScaledHeight - pdfChunk.ImageOffsetY - image.SpacingBefore < minY)
                    {
                        float num9 = (pdfChunk.ImageScalePercentage = (yLine + 2f * descender - pdfChunk.ImageOffsetY - image.SpacingBefore - minY) / image.ScaledHeight);
                    }
                }

                flag = Utilities.IsSurrogatePair(text, currentChar);
                num4 = ((!flag) ? pdfChunk.GetUnicodeEquivalent(text[currentChar]) : pdfChunk.GetUnicodeEquivalent(Utilities.ConvertToUtf32(text, currentChar)));
                if (!PdfChunk.NoPrint(num4))
                {
                    num5 = (flag ? pdfChunk.GetCharWidth(num4) : ((!pdfChunk.IsImage()) ? pdfChunk.GetCharWidth(text[currentChar]) : pdfChunk.ImageWidth));
                    if (width - num5 < 0f && pdfChunk2 == null && pdfChunk.IsImage())
                    {
                        Image image2 = pdfChunk.Image;
                        if (image2.ScaleToFitLineWhenOverflow)
                        {
                            float num11 = (pdfChunk.ImageScalePercentage = width / image2.Width);
                            num5 = width;
                        }
                    }

                    if (pdfChunk.IsTab())
                    {
                        if (pdfChunk.IsAttribute("TABSETTINGS"))
                        {
                            num2 = currentChar;
                            if (tabStop != null)
                            {
                                float num12 = tabStop.GetPosition(num7, num - width, num6);
                                width = num - (num12 + (num - width - num7));
                                if (width < 0f)
                                {
                                    num12 += width;
                                    width = 0f;
                                }

                                tabStop.Position = num12;
                            }

                            tabStop = PdfChunk.GetTabStop(pdfChunk, num - width);
                            if (tabStop.Position > num)
                            {
                                tabStop = null;
                                break;
                            }

                            pdfChunk.TabStop = tabStop;
                            if (tabStop.Align == TabStop.Alignment.LEFT)
                            {
                                width = num - tabStop.Position;
                                tabStop = null;
                                num7 = float.NaN;
                                num6 = float.NaN;
                            }
                            else
                            {
                                num7 = num - width;
                                num6 = float.NaN;
                            }
                        }
                        else
                        {
                            object[] obj = (object[])pdfChunk.GetAttribute("TAB");
                            float num13 = (float)obj[1];
                            if ((bool)obj[2] && num13 < num - width)
                            {
                                return new PdfLine(0f, num, width, alignment, newlineSplit: true, CreateArrayOfPdfChunks(num3, currentChar - 1), isRTL);
                            }

                            detailChunks[currentChar].AdjustLeft(leftX);
                            width = num - num13;
                        }
                    }
                    else if (pdfChunk.IsSeparator())
                    {
                        object[] obj2 = (object[])pdfChunk.GetAttribute("SEPARATOR");
                        IDrawInterface drawInterface = (IDrawInterface)obj2[0];
                        if ((bool)obj2[1] && drawInterface is LineSeparator)
                        {
                            float num14 = num * ((LineSeparator)drawInterface).Percentage / 100f;
                            width -= num14;
                            if (width < 0f)
                            {
                                width = 0f;
                            }
                        }
                    }
                    else
                    {
                        bool flag2 = pdfChunk.IsExtSplitCharacter(num3, currentChar, totalTextLength, text, detailChunks);
                        if (flag2 && char.IsWhiteSpace((char)num4))
                        {
                            num2 = currentChar;
                        }

                        if (width - num5 < 0f)
                        {
                            break;
                        }

                        if (tabStop != null && tabStop.Align == TabStop.Alignment.ANCHOR && float.IsNaN(num6) && tabStop.AnchorChar == (ushort)num4)
                        {
                            num6 = num - width;
                        }

                        width -= num5;
                        if (flag2)
                        {
                            num2 = currentChar;
                        }
                    }

                    pdfChunk2 = pdfChunk;
                    if (flag)
                    {
                        currentChar++;
                    }
                }

                currentChar++;
            }

            if (pdfChunk2 == null)
            {
                currentChar++;
                if (flag)
                {
                    currentChar++;
                }

                return new PdfLine(0f, num, 0f, alignment, newlineSplit: false, CreateArrayOfPdfChunks(currentChar - 1, currentChar - 1), isRTL);
            }

            if (tabStop != null)
            {
                float num15 = tabStop.GetPosition(num7, num - width, num6);
                width = num - (num15 + (num - width - num7));
                if (width < 0f)
                {
                    num15 += width;
                    width = 0f;
                }

                tabStop.Position = num15;
            }

            if (currentChar >= totalTextLength)
            {
                return new PdfLine(0f, num, width, alignment, newlineSplit: true, CreateArrayOfPdfChunks(num3, totalTextLength - 1), isRTL);
            }

            int num16 = TrimRightEx(num3, currentChar - 1);
            if (num16 < num3)
            {
                return new PdfLine(0f, num, width, alignment, newlineSplit: false, CreateArrayOfPdfChunks(num3, currentChar - 1), isRTL);
            }

            if (num16 == currentChar - 1)
            {
                IHyphenationEvent hyphenationEvent = (IHyphenationEvent)pdfChunk2.GetAttribute("HYPHENATION");
                if (hyphenationEvent != null)
                {
                    int[] word = GetWord(num3, num16);
                    if (word != null)
                    {
                        float num17 = width + GetWidth(word[0], currentChar - 1);
                        string hyphenatedWordPre = hyphenationEvent.GetHyphenatedWordPre(new string(text, word[0], word[1] - word[0]), pdfChunk2.Font.Font, pdfChunk2.Font.Size, num17);
                        string hyphenatedWordPost = hyphenationEvent.HyphenatedWordPost;
                        if (hyphenatedWordPre.Length > 0)
                        {
                            PdfChunk extraPdfChunk = new PdfChunk(hyphenatedWordPre, pdfChunk2);
                            currentChar = word[1] - hyphenatedWordPost.Length;
                            return new PdfLine(0f, num, num17 - pdfChunk2.Width(hyphenatedWordPre), alignment, newlineSplit: false, CreateArrayOfPdfChunks(num3, word[0] - 1, extraPdfChunk), isRTL);
                        }
                    }
                }
            }

            if (num2 == -1)
            {
                isWordSplit = true;
            }

            if (num2 == -1 || num2 >= num16)
            {
                return new PdfLine(0f, num, width + GetWidth(num16 + 1, currentChar - 1), alignment, newlineSplit: false, CreateArrayOfPdfChunks(num3, num16), isRTL);
            }

            currentChar = num2 + 1;
            num16 = TrimRightEx(num3, num2);
            if (num16 < num3)
            {
                num16 = currentChar - 1;
            }

            return new PdfLine(0f, num, num - GetWidth(num3, num16), alignment, newlineSplit: false, CreateArrayOfPdfChunks(num3, num16), isRTL);
        }

        public virtual bool IsWordSplit()
        {
            return isWordSplit;
        }

        public virtual float GetWidth(int startIdx, int lastIdx)
        {
            char c = '\0';
            PdfChunk pdfChunk = null;
            float num = 0f;
            TabStop tabStop = null;
            float num2 = float.NaN;
            float num3 = float.NaN;
            while (startIdx <= lastIdx)
            {
                bool flag = Utilities.IsSurrogatePair(text, startIdx);
                if (detailChunks[startIdx].IsTab() && detailChunks[startIdx].IsAttribute("TABSETTINGS"))
                {
                    if (tabStop != null)
                    {
                        float position = tabStop.GetPosition(num3, num, num2);
                        num = position + (num - num3);
                        tabStop.Position = position;
                    }

                    tabStop = detailChunks[startIdx].TabStop;
                    if (tabStop == null)
                    {
                        tabStop = PdfChunk.GetTabStop(detailChunks[startIdx], num);
                        num3 = num;
                        num2 = float.NaN;
                    }
                    else
                    {
                        num = tabStop.Position;
                        tabStop = null;
                        num3 = float.NaN;
                        num2 = float.NaN;
                    }
                }
                else if (flag)
                {
                    num += detailChunks[startIdx].GetCharWidth(Utilities.ConvertToUtf32(text, startIdx));
                    startIdx++;
                }
                else
                {
                    c = text[startIdx];
                    pdfChunk = detailChunks[startIdx];
                    if (!PdfChunk.NoPrint(pdfChunk.GetUnicodeEquivalent(c)))
                    {
                        if (tabStop != null && tabStop.Align != TabStop.Alignment.ANCHOR && float.IsNaN(num2) && tabStop.AnchorChar == (ushort)pdfChunk.GetUnicodeEquivalent(c))
                        {
                            num2 = num;
                        }

                        num += detailChunks[startIdx].GetCharWidth(c);
                    }
                }

                startIdx++;
            }

            if (tabStop != null)
            {
                float position2 = tabStop.GetPosition(num3, num, num2);
                num = position2 + (num - num3);
                tabStop.Position = position2;
            }

            return num;
        }

        public virtual List<PdfChunk> CreateArrayOfPdfChunks(int startIdx, int endIdx)
        {
            return CreateArrayOfPdfChunks(startIdx, endIdx, null);
        }

        public virtual List<PdfChunk> CreateArrayOfPdfChunks(int startIdx, int endIdx, PdfChunk extraPdfChunk)
        {
            bool flag = runDirection == 2 || runDirection == 3;
            if (flag)
            {
                Reorder(startIdx, endIdx);
            }

            List<PdfChunk> list = new List<PdfChunk>();
            PdfChunk pdfChunk = detailChunks[startIdx];
            PdfChunk pdfChunk2 = null;
            StringBuilder stringBuilder = new StringBuilder();
            int num = 0;
            while (startIdx <= endIdx)
            {
                num = (flag ? indexChars[startIdx] : startIdx);
                char c = text[num];
                pdfChunk2 = detailChunks[num];
                if (!PdfChunk.NoPrint(pdfChunk2.GetUnicodeEquivalent(c)))
                {
                    if (pdfChunk2.IsImage() || pdfChunk2.IsSeparator() || pdfChunk2.IsTab())
                    {
                        if (stringBuilder.Length > 0)
                        {
                            list.Add(new PdfChunk(stringBuilder.ToString(), pdfChunk));
                            stringBuilder = new StringBuilder();
                        }

                        list.Add(pdfChunk2);
                    }
                    else if (pdfChunk2 == pdfChunk)
                    {
                        stringBuilder.Append(c);
                    }
                    else
                    {
                        if (stringBuilder.Length > 0)
                        {
                            list.Add(new PdfChunk(stringBuilder.ToString(), pdfChunk));
                            stringBuilder = new StringBuilder();
                        }

                        if (!pdfChunk2.IsImage() && !pdfChunk2.IsSeparator() && !pdfChunk2.IsTab())
                        {
                            stringBuilder.Append(c);
                        }

                        pdfChunk = pdfChunk2;
                    }
                }

                startIdx++;
            }

            if (stringBuilder.Length > 0)
            {
                list.Add(new PdfChunk(stringBuilder.ToString(), pdfChunk));
            }

            if (extraPdfChunk != null)
            {
                list.Add(extraPdfChunk);
            }

            return list;
        }

        public virtual int[] GetWord(int startIdx, int idx)
        {
            int i = idx;
            int num = idx;
            for (; i < totalTextLength && (char.IsLetter(text[i]) || char.IsDigit(text[i])); i++)
            {
            }

            if (i == idx)
            {
                return null;
            }

            while (num >= startIdx && (char.IsLetter(text[num]) || char.IsDigit(text[num])))
            {
                num--;
            }

            num++;
            return new int[2] { num, i };
        }

        public virtual int TrimRight(int startIdx, int endIdx)
        {
            int num = endIdx;
            while (num >= startIdx && IsWS((char)detailChunks[num].GetUnicodeEquivalent(text[num])))
            {
                num--;
            }

            return num;
        }

        public virtual int TrimLeft(int startIdx, int endIdx)
        {
            int i;
            for (i = startIdx; i <= endIdx && IsWS((char)detailChunks[i].GetUnicodeEquivalent(text[i])); i++)
            {
            }

            return i;
        }

        public virtual int TrimRightEx(int startIdx, int endIdx)
        {
            int num = endIdx;
            char c = '\0';
            while (num >= startIdx)
            {
                c = (char)detailChunks[num].GetUnicodeEquivalent(text[num]);
                if (!IsWS(c) && !PdfChunk.NoPrint(c) && (!detailChunks[num].IsTab() || !detailChunks[num].IsAttribute("TABSETTINGS") || !(bool)((object[])detailChunks[num].GetAttribute("TAB"))[1]))
                {
                    break;
                }

                num--;
            }

            return num;
        }

        public virtual int TrimLeftEx(int startIdx, int endIdx)
        {
            int i = startIdx;
            char c = '\0';
            for (; i <= endIdx; i++)
            {
                c = (char)detailChunks[i].GetUnicodeEquivalent(text[i]);
                if (!IsWS(c) && !PdfChunk.NoPrint(c) && (!detailChunks[i].IsTab() || !detailChunks[i].IsAttribute("TABSETTINGS") || !(bool)((object[])detailChunks[i].GetAttribute("TAB"))[1]))
                {
                    break;
                }
            }

            return i;
        }

        public virtual void Reorder(int start, int end)
        {
            byte b = orderLevels[start];
            byte b2 = b;
            byte b3 = b;
            byte b4 = b;
            for (int i = start + 1; i <= end; i++)
            {
                byte b5 = orderLevels[i];
                if (b5 > b)
                {
                    b = b5;
                }
                else if (b5 < b2)
                {
                    b2 = b5;
                }

                b3 = (byte)(b3 & b5);
                b4 = (byte)(b4 | b5);
            }

            if ((b4 & 1) == 0)
            {
                return;
            }

            if ((b3 & 1) == 1)
            {
                Flip(start, end + 1);
                return;
            }

            b2 = (byte)(b2 | 1u);
            while (b >= b2)
            {
                int num = start;
                while (true)
                {
                    if (num <= end && orderLevels[num] < b)
                    {
                        num++;
                        continue;
                    }

                    if (num > end)
                    {
                        break;
                    }

                    int j;
                    for (j = num + 1; j <= end && orderLevels[j] >= b; j++)
                    {
                    }

                    Flip(num, j);
                    num = j + 1;
                }

                b = (byte)(b - 1);
            }
        }

        public virtual void Flip(int start, int end)
        {
            int num = (start + end) / 2;
            end--;
            while (start < num)
            {
                int num2 = indexChars[start];
                indexChars[start] = indexChars[end];
                indexChars[end] = num2;
                start++;
                end--;
            }
        }

        public static bool IsWS(char c)
        {
            return c <= ' ';
        }

        static BidiLine()
        {
            mirrorChars = new IntHashtable();
            mirrorChars[40] = 41;
            mirrorChars[41] = 40;
            mirrorChars[60] = 62;
            mirrorChars[62] = 60;
            mirrorChars[91] = 93;
            mirrorChars[93] = 91;
            mirrorChars[123] = 125;
            mirrorChars[125] = 123;
            mirrorChars[171] = 187;
            mirrorChars[187] = 171;
            mirrorChars[8249] = 8250;
            mirrorChars[8250] = 8249;
            mirrorChars[8261] = 8262;
            mirrorChars[8262] = 8261;
            mirrorChars[8317] = 8318;
            mirrorChars[8318] = 8317;
            mirrorChars[8333] = 8334;
            mirrorChars[8334] = 8333;
            mirrorChars[8712] = 8715;
            mirrorChars[8713] = 8716;
            mirrorChars[8714] = 8717;
            mirrorChars[8715] = 8712;
            mirrorChars[8716] = 8713;
            mirrorChars[8717] = 8714;
            mirrorChars[8725] = 10741;
            mirrorChars[8764] = 8765;
            mirrorChars[8765] = 8764;
            mirrorChars[8771] = 8909;
            mirrorChars[8786] = 8787;
            mirrorChars[8787] = 8786;
            mirrorChars[8788] = 8789;
            mirrorChars[8789] = 8788;
            mirrorChars[8804] = 8805;
            mirrorChars[8805] = 8804;
            mirrorChars[8806] = 8807;
            mirrorChars[8807] = 8806;
            mirrorChars[8808] = 8809;
            mirrorChars[8809] = 8808;
            mirrorChars[8810] = 8811;
            mirrorChars[8811] = 8810;
            mirrorChars[8814] = 8815;
            mirrorChars[8815] = 8814;
            mirrorChars[8816] = 8817;
            mirrorChars[8817] = 8816;
            mirrorChars[8818] = 8819;
            mirrorChars[8819] = 8818;
            mirrorChars[8820] = 8821;
            mirrorChars[8821] = 8820;
            mirrorChars[8822] = 8823;
            mirrorChars[8823] = 8822;
            mirrorChars[8824] = 8825;
            mirrorChars[8825] = 8824;
            mirrorChars[8826] = 8827;
            mirrorChars[8827] = 8826;
            mirrorChars[8828] = 8829;
            mirrorChars[8829] = 8828;
            mirrorChars[8830] = 8831;
            mirrorChars[8831] = 8830;
            mirrorChars[8832] = 8833;
            mirrorChars[8833] = 8832;
            mirrorChars[8834] = 8835;
            mirrorChars[8835] = 8834;
            mirrorChars[8836] = 8837;
            mirrorChars[8837] = 8836;
            mirrorChars[8838] = 8839;
            mirrorChars[8839] = 8838;
            mirrorChars[8840] = 8841;
            mirrorChars[8841] = 8840;
            mirrorChars[8842] = 8843;
            mirrorChars[8843] = 8842;
            mirrorChars[8847] = 8848;
            mirrorChars[8848] = 8847;
            mirrorChars[8849] = 8850;
            mirrorChars[8850] = 8849;
            mirrorChars[8856] = 10680;
            mirrorChars[8866] = 8867;
            mirrorChars[8867] = 8866;
            mirrorChars[8870] = 10974;
            mirrorChars[8872] = 10980;
            mirrorChars[8873] = 10979;
            mirrorChars[8875] = 10981;
            mirrorChars[8880] = 8881;
            mirrorChars[8881] = 8880;
            mirrorChars[8882] = 8883;
            mirrorChars[8883] = 8882;
            mirrorChars[8884] = 8885;
            mirrorChars[8885] = 8884;
            mirrorChars[8886] = 8887;
            mirrorChars[8887] = 8886;
            mirrorChars[8905] = 8906;
            mirrorChars[8906] = 8905;
            mirrorChars[8907] = 8908;
            mirrorChars[8908] = 8907;
            mirrorChars[8909] = 8771;
            mirrorChars[8912] = 8913;
            mirrorChars[8913] = 8912;
            mirrorChars[8918] = 8919;
            mirrorChars[8919] = 8918;
            mirrorChars[8920] = 8921;
            mirrorChars[8921] = 8920;
            mirrorChars[8922] = 8923;
            mirrorChars[8923] = 8922;
            mirrorChars[8924] = 8925;
            mirrorChars[8925] = 8924;
            mirrorChars[8926] = 8927;
            mirrorChars[8927] = 8926;
            mirrorChars[8928] = 8929;
            mirrorChars[8929] = 8928;
            mirrorChars[8930] = 8931;
            mirrorChars[8931] = 8930;
            mirrorChars[8932] = 8933;
            mirrorChars[8933] = 8932;
            mirrorChars[8934] = 8935;
            mirrorChars[8935] = 8934;
            mirrorChars[8936] = 8937;
            mirrorChars[8937] = 8936;
            mirrorChars[8938] = 8939;
            mirrorChars[8939] = 8938;
            mirrorChars[8940] = 8941;
            mirrorChars[8941] = 8940;
            mirrorChars[8944] = 8945;
            mirrorChars[8945] = 8944;
            mirrorChars[8946] = 8954;
            mirrorChars[8947] = 8955;
            mirrorChars[8948] = 8956;
            mirrorChars[8950] = 8957;
            mirrorChars[8951] = 8958;
            mirrorChars[8954] = 8946;
            mirrorChars[8955] = 8947;
            mirrorChars[8956] = 8948;
            mirrorChars[8957] = 8950;
            mirrorChars[8958] = 8951;
            mirrorChars[8968] = 8969;
            mirrorChars[8969] = 8968;
            mirrorChars[8970] = 8971;
            mirrorChars[8971] = 8970;
            mirrorChars[9001] = 9002;
            mirrorChars[9002] = 9001;
            mirrorChars[10088] = 10089;
            mirrorChars[10089] = 10088;
            mirrorChars[10090] = 10091;
            mirrorChars[10091] = 10090;
            mirrorChars[10092] = 10093;
            mirrorChars[10093] = 10092;
            mirrorChars[10094] = 10095;
            mirrorChars[10095] = 10094;
            mirrorChars[10096] = 10097;
            mirrorChars[10097] = 10096;
            mirrorChars[10098] = 10099;
            mirrorChars[10099] = 10098;
            mirrorChars[10100] = 10101;
            mirrorChars[10101] = 10100;
            mirrorChars[10197] = 10198;
            mirrorChars[10198] = 10197;
            mirrorChars[10205] = 10206;
            mirrorChars[10206] = 10205;
            mirrorChars[10210] = 10211;
            mirrorChars[10211] = 10210;
            mirrorChars[10212] = 10213;
            mirrorChars[10213] = 10212;
            mirrorChars[10214] = 10215;
            mirrorChars[10215] = 10214;
            mirrorChars[10216] = 10217;
            mirrorChars[10217] = 10216;
            mirrorChars[10218] = 10219;
            mirrorChars[10219] = 10218;
            mirrorChars[10627] = 10628;
            mirrorChars[10628] = 10627;
            mirrorChars[10629] = 10630;
            mirrorChars[10630] = 10629;
            mirrorChars[10631] = 10632;
            mirrorChars[10632] = 10631;
            mirrorChars[10633] = 10634;
            mirrorChars[10634] = 10633;
            mirrorChars[10635] = 10636;
            mirrorChars[10636] = 10635;
            mirrorChars[10637] = 10640;
            mirrorChars[10638] = 10639;
            mirrorChars[10639] = 10638;
            mirrorChars[10640] = 10637;
            mirrorChars[10641] = 10642;
            mirrorChars[10642] = 10641;
            mirrorChars[10643] = 10644;
            mirrorChars[10644] = 10643;
            mirrorChars[10645] = 10646;
            mirrorChars[10646] = 10645;
            mirrorChars[10647] = 10648;
            mirrorChars[10648] = 10647;
            mirrorChars[10680] = 8856;
            mirrorChars[10688] = 10689;
            mirrorChars[10689] = 10688;
            mirrorChars[10692] = 10693;
            mirrorChars[10693] = 10692;
            mirrorChars[10703] = 10704;
            mirrorChars[10704] = 10703;
            mirrorChars[10705] = 10706;
            mirrorChars[10706] = 10705;
            mirrorChars[10708] = 10709;
            mirrorChars[10709] = 10708;
            mirrorChars[10712] = 10713;
            mirrorChars[10713] = 10712;
            mirrorChars[10714] = 10715;
            mirrorChars[10715] = 10714;
            mirrorChars[10741] = 8725;
            mirrorChars[10744] = 10745;
            mirrorChars[10745] = 10744;
            mirrorChars[10748] = 10749;
            mirrorChars[10749] = 10748;
            mirrorChars[10795] = 10796;
            mirrorChars[10796] = 10795;
            mirrorChars[10797] = 10796;
            mirrorChars[10798] = 10797;
            mirrorChars[10804] = 10805;
            mirrorChars[10805] = 10804;
            mirrorChars[10812] = 10813;
            mirrorChars[10813] = 10812;
            mirrorChars[10852] = 10853;
            mirrorChars[10853] = 10852;
            mirrorChars[10873] = 10874;
            mirrorChars[10874] = 10873;
            mirrorChars[10877] = 10878;
            mirrorChars[10878] = 10877;
            mirrorChars[10879] = 10880;
            mirrorChars[10880] = 10879;
            mirrorChars[10881] = 10882;
            mirrorChars[10882] = 10881;
            mirrorChars[10883] = 10884;
            mirrorChars[10884] = 10883;
            mirrorChars[10891] = 10892;
            mirrorChars[10892] = 10891;
            mirrorChars[10897] = 10898;
            mirrorChars[10898] = 10897;
            mirrorChars[10899] = 10900;
            mirrorChars[10900] = 10899;
            mirrorChars[10901] = 10902;
            mirrorChars[10902] = 10901;
            mirrorChars[10903] = 10904;
            mirrorChars[10904] = 10903;
            mirrorChars[10905] = 10906;
            mirrorChars[10906] = 10905;
            mirrorChars[10907] = 10908;
            mirrorChars[10908] = 10907;
            mirrorChars[10913] = 10914;
            mirrorChars[10914] = 10913;
            mirrorChars[10918] = 10919;
            mirrorChars[10919] = 10918;
            mirrorChars[10920] = 10921;
            mirrorChars[10921] = 10920;
            mirrorChars[10922] = 10923;
            mirrorChars[10923] = 10922;
            mirrorChars[10924] = 10925;
            mirrorChars[10925] = 10924;
            mirrorChars[10927] = 10928;
            mirrorChars[10928] = 10927;
            mirrorChars[10931] = 10932;
            mirrorChars[10932] = 10931;
            mirrorChars[10939] = 10940;
            mirrorChars[10940] = 10939;
            mirrorChars[10941] = 10942;
            mirrorChars[10942] = 10941;
            mirrorChars[10943] = 10944;
            mirrorChars[10944] = 10943;
            mirrorChars[10945] = 10946;
            mirrorChars[10946] = 10945;
            mirrorChars[10947] = 10948;
            mirrorChars[10948] = 10947;
            mirrorChars[10949] = 10950;
            mirrorChars[10950] = 10949;
            mirrorChars[10957] = 10958;
            mirrorChars[10958] = 10957;
            mirrorChars[10959] = 10960;
            mirrorChars[10960] = 10959;
            mirrorChars[10961] = 10962;
            mirrorChars[10962] = 10961;
            mirrorChars[10963] = 10964;
            mirrorChars[10964] = 10963;
            mirrorChars[10965] = 10966;
            mirrorChars[10966] = 10965;
            mirrorChars[10974] = 8870;
            mirrorChars[10979] = 8873;
            mirrorChars[10980] = 8872;
            mirrorChars[10981] = 8875;
            mirrorChars[10988] = 10989;
            mirrorChars[10989] = 10988;
            mirrorChars[10999] = 11000;
            mirrorChars[11000] = 10999;
            mirrorChars[11001] = 11002;
            mirrorChars[11002] = 11001;
            mirrorChars[12296] = 12297;
            mirrorChars[12297] = 12296;
            mirrorChars[12298] = 12299;
            mirrorChars[12299] = 12298;
            mirrorChars[12300] = 12301;
            mirrorChars[12301] = 12300;
            mirrorChars[12302] = 12303;
            mirrorChars[12303] = 12302;
            mirrorChars[12304] = 12305;
            mirrorChars[12305] = 12304;
            mirrorChars[12308] = 12309;
            mirrorChars[12309] = 12308;
            mirrorChars[12310] = 12311;
            mirrorChars[12311] = 12310;
            mirrorChars[12312] = 12313;
            mirrorChars[12313] = 12312;
            mirrorChars[12314] = 12315;
            mirrorChars[12315] = 12314;
            mirrorChars[65288] = 65289;
            mirrorChars[65289] = 65288;
            mirrorChars[65308] = 65310;
            mirrorChars[65310] = 65308;
            mirrorChars[65339] = 65341;
            mirrorChars[65341] = 65339;
            mirrorChars[65371] = 65373;
            mirrorChars[65373] = 65371;
            mirrorChars[65375] = 65376;
            mirrorChars[65376] = 65375;
            mirrorChars[65378] = 65379;
            mirrorChars[65379] = 65378;
        }

        public static string ProcessLTR(string s, int runDirection, int arabicOptions)
        {
            BidiLine bidiLine = new BidiLine();
            bidiLine.AddChunk(new PdfChunk(new Chunk(s), null));
            bidiLine.arabicOptions = arabicOptions;
            bidiLine.GetParagraph(runDirection);
            List<PdfChunk> list = bidiLine.CreateArrayOfPdfChunks(0, bidiLine.totalTextLength - 1);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PdfChunk item in list)
            {
                stringBuilder.Append(item.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}
