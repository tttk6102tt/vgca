using System.Text;

namespace Sign.itext.text.pdf
{
    public class PdfLine
    {
        protected internal List<PdfChunk> line;

        protected internal float left;

        protected internal float width;

        protected internal int alignment;

        protected internal float height;

        protected internal bool newlineSplit;

        protected internal float originalWidth;

        protected internal bool isRTL;

        protected internal ListItem listItem;

        protected TabStop tabStop;

        protected float tabStopAnchorPosition = float.NaN;

        protected float tabPosition = float.NaN;

        public virtual int Size => line.Count;

        internal float Height => height;

        internal float IndentLeft
        {
            get
            {
                if (isRTL)
                {
                    return alignment switch
                    {
                        1 => left + width / 2f,
                        2 => left,
                        3 => left + (HasToBeJustified() ? 0f : width),
                        _ => left + width,
                    };
                }

                if (GetSeparatorCount() <= 0)
                {
                    switch (alignment)
                    {
                        case 2:
                            return left + width;
                        case 1:
                            return left + width / 2f;
                    }
                }

                return left;
            }
        }

        internal float WidthLeft => width;

        internal int NumberOfSpaces
        {
            get
            {
                int num = 0;
                foreach (PdfChunk item in line)
                {
                    string text = item.ToString();
                    int length = text.Length;
                    for (int i = 0; i < length; i++)
                    {
                        if (text[i] == ' ')
                        {
                            num++;
                        }
                    }
                }

                return num;
            }
        }

        public virtual ListItem ListItem
        {
            get
            {
                return listItem;
            }
            set
            {
                listItem = value;
            }
        }

        public virtual Chunk ListSymbol
        {
            get
            {
                if (listItem == null)
                {
                    return null;
                }

                return listItem.ListSymbol;
            }
        }

        public virtual float ListIndent
        {
            get
            {
                if (listItem == null)
                {
                    return 0f;
                }

                return listItem.IndentationLeft;
            }
        }

        public virtual bool NewlineSplit
        {
            get
            {
                if (newlineSplit)
                {
                    return alignment != 8;
                }

                return false;
            }
        }

        public virtual int LastStrokeChunk
        {
            get
            {
                int num = line.Count - 1;
                while (num >= 0 && !line[num].IsStroked())
                {
                    num--;
                }

                return num;
            }
        }

        public virtual float OriginalWidth => originalWidth;

        internal bool RTL => isRTL;

        public virtual float Ascender
        {
            get
            {
                float num = 0f;
                foreach (PdfChunk item in line)
                {
                    if (item.IsImage())
                    {
                        num = Math.Max(num, item.ImageHeight + item.ImageOffsetY);
                        continue;
                    }

                    PdfFont font = item.Font;
                    float textRise = item.TextRise;
                    num = Math.Max(num, ((textRise > 0f) ? textRise : 0f) + font.Font.GetFontDescriptor(1, font.Size));
                }

                return num;
            }
        }

        public virtual float Descender
        {
            get
            {
                float num = 0f;
                foreach (PdfChunk item in line)
                {
                    if (item.IsImage())
                    {
                        num = Math.Min(num, item.ImageOffsetY);
                        continue;
                    }

                    PdfFont font = item.Font;
                    float textRise = item.TextRise;
                    num = Math.Min(num, ((textRise < 0f) ? textRise : 0f) + font.Font.GetFontDescriptor(3, font.Size));
                }

                return num;
            }
        }

        internal PdfLine(float left, float right, int alignment, float height)
        {
            this.left = left;
            width = right - left;
            originalWidth = width;
            this.alignment = alignment;
            this.height = height;
            line = new List<PdfChunk>();
        }

        internal PdfLine(float left, float originalWidth, float remainingWidth, int alignment, bool newlineSplit, List<PdfChunk> line, bool isRTL)
        {
            this.left = left;
            this.originalWidth = originalWidth;
            width = remainingWidth;
            this.alignment = alignment;
            this.line = line;
            this.newlineSplit = newlineSplit;
            this.isRTL = isRTL;
        }

        internal PdfChunk Add(PdfChunk chunk)
        {
            if (chunk == null || chunk.ToString().Equals(""))
            {
                return null;
            }

            PdfChunk pdfChunk = chunk.Split(width);
            newlineSplit = chunk.IsNewlineSplit() || pdfChunk == null;
            if (chunk.IsTab())
            {
                object[] array = (object[])chunk.GetAttribute("TAB");
                if (chunk.IsAttribute("TABSETTINGS"))
                {
                    bool flag = (bool)array[1];
                    if (flag && line.Count <= 0)
                    {
                        return null;
                    }

                    Flush();
                    tabStopAnchorPosition = float.NaN;
                    tabStop = PdfChunk.GetTabStop(chunk, originalWidth - width);
                    if (tabStop.Position > originalWidth)
                    {
                        if (flag)
                        {
                            pdfChunk = null;
                        }
                        else if ((double)Math.Abs(originalWidth - width) < 0.001)
                        {
                            AddToLine(chunk);
                            pdfChunk = null;
                        }
                        else
                        {
                            pdfChunk = chunk;
                        }

                        width = 0f;
                    }
                    else
                    {
                        chunk.TabStop = tabStop;
                        if (tabStop.Align == TabStop.Alignment.LEFT)
                        {
                            width = originalWidth - tabStop.Position;
                            tabStop = null;
                            tabPosition = float.NaN;
                        }
                        else
                        {
                            tabPosition = originalWidth - width;
                        }

                        AddToLine(chunk);
                    }
                }
                else
                {
                    float num = (float)array[1];
                    if ((bool)array[2] && num < originalWidth - width)
                    {
                        return chunk;
                    }

                    chunk.AdjustLeft(left);
                    width = originalWidth - num;
                    AddToLine(chunk);
                }
            }
            else if (chunk.Length > 0 || chunk.IsImage())
            {
                if (pdfChunk != null)
                {
                    chunk.TrimLastSpace();
                }

                width -= chunk.Width();
                AddToLine(chunk);
            }
            else
            {
                if (line.Count < 1)
                {
                    chunk = pdfChunk;
                    pdfChunk = chunk.Truncate(width);
                    width -= chunk.Width();
                    if (chunk.Length > 0)
                    {
                        AddToLine(chunk);
                        return pdfChunk;
                    }

                    if (pdfChunk != null)
                    {
                        AddToLine(chunk);
                    }

                    return null;
                }

                width += line[line.Count - 1].TrimLastSpace();
            }

            return pdfChunk;
        }

        private void AddToLine(PdfChunk chunk)
        {
            if (chunk.ChangeLeading)
            {
                float num;
                if (chunk.IsImage())
                {
                    Image image = chunk.Image;
                    num = chunk.ImageHeight + chunk.ImageOffsetY + image.BorderWidthTop + image.SpacingBefore;
                }
                else
                {
                    num = chunk.Leading;
                }

                if (num > height)
                {
                    height = num;
                }
            }

            if (tabStop != null && tabStop.Align == TabStop.Alignment.ANCHOR && float.IsNaN(tabStopAnchorPosition))
            {
                string text = chunk.ToString();
                int num2 = text.IndexOf(tabStop.AnchorChar);
                if (num2 != -1)
                {
                    float num3 = chunk.Width(text.Substring(num2));
                    tabStopAnchorPosition = originalWidth - width - num3;
                }
            }

            line.Add(chunk);
        }

        public virtual IEnumerator<PdfChunk> GetEnumerator()
        {
            return line.GetEnumerator();
        }

        public virtual bool HasToBeJustified()
        {
            if ((alignment == 3 && !newlineSplit) || alignment == 8)
            {
                return width != 0f;
            }

            return false;
        }

        public virtual void ResetAlignment()
        {
            if (alignment == 3)
            {
                alignment = 0;
            }
        }

        internal void SetExtraIndent(float extra)
        {
            left += extra;
            width -= extra;
            originalWidth -= extra;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (PdfChunk item in line)
            {
                stringBuilder.Append(item.ToString());
            }

            return stringBuilder.ToString();
        }

        public virtual int GetLineLengthUtf32()
        {
            int num = 0;
            foreach (PdfChunk item in line)
            {
                num += item.LengthUtf32;
            }

            return num;
        }

        public virtual PdfChunk GetChunk(int idx)
        {
            if (idx < 0 || idx >= line.Count)
            {
                return null;
            }

            return line[idx];
        }

        internal float[] GetMaxSize(float fixedLeading, float multipliedLeading)
        {
            float num = 0f;
            float num2 = -10000f;
            for (int i = 0; i < line.Count; i++)
            {
                PdfChunk pdfChunk = line[i];
                if (pdfChunk.IsImage())
                {
                    Image image = pdfChunk.Image;
                    if (pdfChunk.ChangeLeading)
                    {
                        num2 = Math.Max(pdfChunk.ImageHeight + pdfChunk.ImageOffsetY + image.SpacingBefore, num2);
                    }
                }
                else
                {
                    num = ((!pdfChunk.ChangeLeading) ? Math.Max(fixedLeading + multipliedLeading * pdfChunk.Font.Size, num) : Math.Max(pdfChunk.Leading, num));
                }
            }

            return new float[2]
            {
                (num > 0f) ? num : fixedLeading,
                num2
            };
        }

        internal int GetSeparatorCount()
        {
            int num = 0;
            foreach (PdfChunk item in line)
            {
                if (item.IsTab())
                {
                    if (!item.IsAttribute("TABSETTINGS"))
                    {
                        return -1;
                    }
                }
                else if (item.IsHorizontalSeparator())
                {
                    num++;
                }
            }

            return num;
        }

        public virtual float GetWidthCorrected(float charSpacing, float wordSpacing)
        {
            float num = 0f;
            for (int i = 0; i < line.Count; i++)
            {
                PdfChunk pdfChunk = line[i];
                num += pdfChunk.GetWidthCorrected(charSpacing, wordSpacing);
            }

            return num;
        }

        public virtual void Flush()
        {
            if (tabStop != null)
            {
                float num = originalWidth - width - tabPosition;
                float num2 = tabStop.GetPosition(tabPosition, originalWidth - width, tabStopAnchorPosition);
                width = originalWidth - num2 - num;
                if (width < 0f)
                {
                    num2 += width;
                }

                tabStop.Position = num2;
                tabStop = null;
                tabPosition = float.NaN;
            }
        }
    }
}
