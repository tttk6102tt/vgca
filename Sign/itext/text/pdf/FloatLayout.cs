using Sign.itext.pdf;
using Sign.itext.text.api;

namespace Sign.itext.text.pdf
{
    public class FloatLayout
    {
        protected float maxY;

        protected float minY;

        protected float leftX;

        protected float rightX;

        protected float yLine;

        protected float floatLeftX;

        protected float floatRightX;

        protected float filledWidth;

        public ColumnText compositeColumn;

        public List<IElement> content;

        protected readonly bool useAscender;

        public virtual float YLine
        {
            get
            {
                return yLine;
            }
            set
            {
                yLine = value;
            }
        }

        public virtual float FilledWidth
        {
            get
            {
                return filledWidth;
            }
            set
            {
                filledWidth = value;
            }
        }

        public int RunDirection
        {
            get
            {
                return compositeColumn.RunDirection;
            }
            set
            {
                compositeColumn.RunDirection = value;
            }
        }

        public FloatLayout(List<IElement> elements, bool useAscender)
        {
            compositeColumn = new ColumnText(null);
            compositeColumn.UseAscender = useAscender;
            this.useAscender = useAscender;
            content = elements;
        }

        public virtual void SetSimpleColumn(float llx, float lly, float urx, float ury)
        {
            leftX = Math.Min(llx, urx);
            maxY = Math.Max(lly, ury);
            minY = Math.Min(lly, ury);
            rightX = Math.Max(llx, urx);
            floatLeftX = leftX;
            floatRightX = rightX;
            yLine = maxY;
            filledWidth = 0f;
        }

        public virtual int Layout(PdfContentByte canvas, bool simulate)
        {
            compositeColumn.Canvas = canvas;
            int num = 1;
            List<IElement> list = new List<IElement>();
            List<IElement> list2 = (simulate ? new List<IElement>(content) : content);
            while (list2.Count > 0)
            {
                if (list2[0] is PdfDiv)
                {
                    PdfDiv pdfDiv = (PdfDiv)list2[0];
                    if (pdfDiv.Float == PdfDiv.FloatType.LEFT || pdfDiv.Float == PdfDiv.FloatType.RIGHT)
                    {
                        list.Add(pdfDiv);
                        list2.RemoveAt(0);
                        continue;
                    }

                    if (list.Count > 0)
                    {
                        num = FloatingLayout(list, simulate);
                        if ((num & 1) == 0)
                        {
                            break;
                        }
                    }

                    list2.RemoveAt(0);
                    num = pdfDiv.Layout(canvas, useAscender, simulate: true, floatLeftX, minY, floatRightX, yLine);
                    if (!simulate)
                    {
                        canvas.OpenMCBlock(pdfDiv);
                        num = pdfDiv.Layout(canvas, useAscender, simulate, floatLeftX, minY, floatRightX, yLine);
                        canvas.CloseMCBlock(pdfDiv);
                    }

                    if (pdfDiv.getActualWidth() > filledWidth)
                    {
                        filledWidth = pdfDiv.getActualWidth();
                    }

                    if ((num & 1) == 0)
                    {
                        list2.Insert(0, pdfDiv);
                        yLine = pdfDiv.YLine;
                        break;
                    }

                    yLine -= pdfDiv.getActualHeight();
                }
                else
                {
                    list.Add(list2[0]);
                    list2.RemoveAt(0);
                }
            }

            if (((uint)num & (true ? 1u : 0u)) != 0 && list.Count > 0)
            {
                num = FloatingLayout(list, simulate);
            }

            list2.InsertRange(0, list);
            return num;
        }

        private int FloatingLayout(List<IElement> floatingElements, bool simulate)
        {
            int num = 1;
            float num2 = yLine;
            float num3 = 0f;
            float num4 = 0f;
            ColumnText columnText = compositeColumn;
            if (simulate)
            {
                columnText = ColumnText.Duplicate(compositeColumn);
            }

            while (floatingElements.Count > 0)
            {
                IElement element = floatingElements[0];
                floatingElements.RemoveAt(0);
                if (element is PdfDiv)
                {
                    PdfDiv pdfDiv = (PdfDiv)element;
                    num = pdfDiv.Layout(compositeColumn.Canvas, useAscender, simulate: true, floatLeftX, minY, floatRightX, yLine);
                    if ((num & 1) == 0)
                    {
                        yLine = num2;
                        floatLeftX = leftX;
                        floatRightX = rightX;
                        num = pdfDiv.Layout(compositeColumn.Canvas, useAscender, simulate: true, floatLeftX, minY, floatRightX, yLine);
                        if ((num & 1) == 0)
                        {
                            floatingElements.Insert(0, pdfDiv);
                            break;
                        }
                    }

                    if (pdfDiv.Float == PdfDiv.FloatType.LEFT)
                    {
                        num = pdfDiv.Layout(compositeColumn.Canvas, useAscender, simulate, floatLeftX, minY, floatRightX, yLine);
                        floatLeftX += pdfDiv.getActualWidth();
                        num3 += pdfDiv.getActualWidth();
                    }
                    else if (pdfDiv.Float == PdfDiv.FloatType.RIGHT)
                    {
                        num = pdfDiv.Layout(compositeColumn.Canvas, useAscender, simulate, floatRightX - pdfDiv.getActualWidth() - 0.01f, minY, floatRightX, yLine);
                        floatRightX -= pdfDiv.getActualWidth();
                        num4 += pdfDiv.getActualWidth();
                    }

                    num2 = Math.Min(num2, yLine - pdfDiv.getActualHeight());
                    continue;
                }

                if (minY > num2)
                {
                    num = 2;
                    floatingElements.Insert(0, element);
                    columnText?.SetText(null);
                    break;
                }

                if (element is ISpaceable)
                {
                    yLine -= ((ISpaceable)element).SpacingBefore;
                }

                if (simulate)
                {
                    if (element is PdfPTable)
                    {
                        columnText.AddElement(new PdfPTable((PdfPTable)element));
                    }
                    else
                    {
                        columnText.AddElement(element);
                    }
                }
                else
                {
                    columnText.AddElement(element);
                }

                if (yLine > num2)
                {
                    columnText.SetSimpleColumn(floatLeftX, yLine, floatRightX, num2);
                }
                else
                {
                    columnText.SetSimpleColumn(floatLeftX, yLine, floatRightX, minY);
                }

                columnText.FilledWidth = 0f;
                num = columnText.Go(simulate);
                if (yLine > num2 && (floatLeftX > leftX || floatRightX < rightX) && (num & 1) == 0)
                {
                    yLine = num2;
                    floatLeftX = leftX;
                    floatRightX = rightX;
                    if (num3 != 0f && num4 != 0f)
                    {
                        filledWidth = rightX - leftX;
                    }
                    else
                    {
                        if (num3 > filledWidth)
                        {
                            filledWidth = num3;
                        }

                        if (num4 > filledWidth)
                        {
                            filledWidth = num4;
                        }
                    }

                    num3 = 0f;
                    num4 = 0f;
                    if (simulate && element is PdfPTable)
                    {
                        columnText.AddElement(new PdfPTable((PdfPTable)element));
                    }

                    columnText.SetSimpleColumn(floatLeftX, yLine, floatRightX, minY);
                    num = columnText.Go(simulate);
                    num2 = (yLine = columnText.YLine + columnText.Descender);
                    if (columnText.FilledWidth > filledWidth)
                    {
                        filledWidth = columnText.FilledWidth;
                    }
                }
                else
                {
                    if (num4 > 0f)
                    {
                        num4 += columnText.FilledWidth;
                    }
                    else if (num3 > 0f)
                    {
                        num3 += columnText.FilledWidth;
                    }
                    else if (columnText.FilledWidth > filledWidth)
                    {
                        filledWidth = columnText.FilledWidth;
                    }

                    num2 = Math.Min(columnText.YLine + columnText.Descender, num2);
                    yLine = columnText.YLine + columnText.Descender;
                }

                if ((num & 1) == 0)
                {
                    if (!simulate)
                    {
                        floatingElements.InsertRange(0, columnText.CompositeElements);
                        columnText.CompositeElements.Clear();
                    }
                    else
                    {
                        floatingElements.Insert(0, element);
                        columnText.SetText(null);
                    }

                    break;
                }

                columnText.SetText(null);
            }

            if (num3 != 0f && num4 != 0f)
            {
                filledWidth = rightX - leftX;
            }
            else
            {
                if (num3 > filledWidth)
                {
                    filledWidth = num3;
                }

                if (num4 > filledWidth)
                {
                    filledWidth = num4;
                }
            }

            yLine = num2;
            floatLeftX = leftX;
            floatRightX = rightX;
            return num;
        }
    }
}
