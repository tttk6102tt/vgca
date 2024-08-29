using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.itext.text.pdf.interfaces;

namespace Sign.itext.text.pdf
{
    public class PdfStructureElement : PdfDictionary, IPdfStructureElement
    {
        private PdfStructureElement parent;

        private PdfStructureTreeRoot top;

        private PdfIndirectReference reference;

        private PdfName structureType;

        public virtual PdfName StructureType => structureType;

        public virtual PdfDictionary Parent => GetParent(includeStructTreeRoot: false);

        public virtual PdfIndirectReference Reference => reference;

        public PdfStructureElement(PdfStructureElement parent, PdfName structureType)
        {
            top = parent.top;
            Init(parent, structureType);
            this.parent = parent;
            Put(PdfName.P, parent.reference);
            Put(PdfName.TYPE, PdfName.STRUCTELEM);
        }

        public PdfStructureElement(PdfStructureTreeRoot parent, PdfName structureType)
        {
            top = parent;
            Init(parent, structureType);
            Put(PdfName.P, parent.Reference);
            Put(PdfName.TYPE, PdfName.STRUCTELEM);
        }

        internal PdfStructureElement(PdfDictionary parent, PdfName structureType)
        {
            if (parent is PdfStructureElement)
            {
                top = ((PdfStructureElement)parent).top;
                Init(parent, structureType);
                this.parent = (PdfStructureElement)parent;
                Put(PdfName.P, ((PdfStructureElement)parent).reference);
                Put(PdfName.TYPE, PdfName.STRUCTELEM);
            }
            else if (parent is PdfStructureTreeRoot)
            {
                top = (PdfStructureTreeRoot)parent;
                Init(parent, structureType);
                Put(PdfName.P, ((PdfStructureTreeRoot)parent).Reference);
                Put(PdfName.TYPE, PdfName.STRUCTELEM);
            }
        }

        private void Init(PdfDictionary parent, PdfName structureType)
        {
            if (!top.Writer.GetStandardStructElems().Contains(structureType))
            {
                PdfDictionary asDict = top.GetAsDict(PdfName.ROLEMAP);
                if (asDict == null || !asDict.Contains(structureType))
                {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("unknown.structure.element.role.1", structureType.ToString()));
                }

                this.structureType = asDict.GetAsName(structureType);
            }
            else
            {
                this.structureType = structureType;
            }

            PdfObject pdfObject = parent.Get(PdfName.K);
            PdfArray pdfArray = null;
            if (pdfObject == null)
            {
                pdfArray = new PdfArray();
                parent.Put(PdfName.K, pdfArray);
            }
            else if (pdfObject is PdfArray)
            {
                pdfArray = (PdfArray)pdfObject;
            }
            else
            {
                pdfArray = new PdfArray();
                pdfArray.Add(pdfObject);
                parent.Put(PdfName.K, pdfArray);
            }

            if (pdfArray.Size > 0)
            {
                if (pdfArray.GetAsNumber(0) != null)
                {
                    pdfArray.Remove(0);
                }

                if (pdfArray.Size > 0)
                {
                    PdfDictionary asDict2 = pdfArray.GetAsDict(0);
                    if (asDict2 != null && PdfName.MCR.Equals(asDict2.GetAsName(PdfName.TYPE)))
                    {
                        pdfArray.Remove(0);
                    }
                }
            }

            pdfArray.Add(this);
            Put(PdfName.S, structureType);
            reference = top.Writer.PdfIndirectReference;
        }

        public virtual PdfDictionary GetParent(bool includeStructTreeRoot)
        {
            if (parent == null && includeStructTreeRoot)
            {
                return top;
            }

            return parent;
        }

        internal virtual void SetPageMark(int page, int mark)
        {
            if (mark >= 0)
            {
                Put(PdfName.K, new PdfNumber(mark));
            }

            top.SetPageMark(page, reference);
        }

        internal virtual void SetAnnotation(PdfAnnotation annot, PdfIndirectReference currentPage)
        {
            PdfArray pdfArray = GetAsArray(PdfName.K);
            if (pdfArray == null)
            {
                pdfArray = new PdfArray();
                PdfObject pdfObject = Get(PdfName.K);
                if (pdfObject != null)
                {
                    pdfArray.Add(pdfObject);
                }

                Put(PdfName.K, pdfArray);
            }

            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.TYPE, PdfName.OBJR);
            pdfDictionary.Put(PdfName.OBJ, annot.IndirectReference);
            if (annot.Role == PdfName.FORM)
            {
                pdfDictionary.Put(PdfName.PG, currentPage);
            }

            pdfArray.Add(pdfDictionary);
        }

        public virtual PdfObject GetAttribute(PdfName name)
        {
            PdfDictionary asDict = GetAsDict(PdfName.A);
            if (asDict != null && asDict.Contains(name))
            {
                return asDict.Get(name);
            }

            PdfDictionary pdfDictionary = Parent;
            if (pdfDictionary is PdfStructureElement)
            {
                return ((PdfStructureElement)pdfDictionary).GetAttribute(name);
            }

            if (pdfDictionary is PdfStructureTreeRoot)
            {
                return ((PdfStructureTreeRoot)pdfDictionary).GetAttribute(name);
            }

            return new PdfNull();
        }

        public virtual void SetAttribute(PdfName name, PdfObject obj)
        {
            PdfDictionary pdfDictionary = GetAsDict(PdfName.A);
            if (pdfDictionary == null)
            {
                pdfDictionary = new PdfDictionary();
                Put(PdfName.A, pdfDictionary);
            }

            pdfDictionary.Put(name, obj);
        }

        public virtual void WriteAttributes(IAccessibleElement element)
        {
            if (element is ListItem)
            {
                WriteAttributes((ListItem)element);
            }
            else if (element is Paragraph)
            {
                WriteAttributes((Paragraph)element);
            }
            else if (element is Chunk)
            {
                WriteAttributes((Chunk)element);
            }
            else if (element is Image)
            {
                WriteAttributes((Image)element);
            }
            else if (element is List)
            {
                WriteAttributes((List)element);
            }
            else if (element is ListLabel)
            {
                WriteAttributes((ListLabel)element);
            }
            else if (element is ListBody)
            {
                WriteAttributes((ListBody)element);
            }
            else if (element is PdfPTable)
            {
                WriteAttributes((PdfPTable)element);
            }
            else if (element is PdfPRow)
            {
                WriteAttributes((PdfPRow)element);
            }
            else if (element is PdfPHeaderCell)
            {
                WriteAttributes((PdfPHeaderCell)element);
            }
            else if (element is PdfPCell)
            {
                WriteAttributes((PdfPCell)element);
            }
            else if (element is PdfPTableHeader)
            {
                WriteAttributes((PdfPTableHeader)element);
            }
            else if (element is PdfPTableFooter)
            {
                WriteAttributes((PdfPTableFooter)element);
            }
            else if (element is PdfPTableBody)
            {
                WriteAttributes((PdfPTableBody)element);
            }
            else if (element is PdfDiv)
            {
                WriteAttributes((PdfDiv)element);
            }
            else if (element is PdfTemplate)
            {
                WriteAttributes((PdfTemplate)element);
            }
            else if (element is Document)
            {
                WriteAttributes((Document)element);
            }

            if (element.GetAccessibleAttributes() == null)
            {
                return;
            }

            foreach (PdfName key in element.GetAccessibleAttributes().Keys)
            {
                if (key.Equals(PdfName.LANG) || key.Equals(PdfName.ALT) || key.Equals(PdfName.ACTUALTEXT) || key.Equals(PdfName.E) || key.Equals(PdfName.T))
                {
                    Put(key, element.GetAccessibleAttribute(key));
                }
                else
                {
                    SetAttribute(key, element.GetAccessibleAttribute(key));
                }
            }
        }

        private void WriteAttributes(Chunk chunk)
        {
            if (chunk == null)
            {
                return;
            }

            if (chunk.GetImage() != null)
            {
                WriteAttributes(chunk.GetImage());
                return;
            }

            Dictionary<string, object> attributes = chunk.Attributes;
            if (attributes == null)
            {
                return;
            }

            SetAttribute(PdfName.O, PdfName.LAYOUT);
            if (attributes.ContainsKey("UNDERLINE"))
            {
                SetAttribute(PdfName.TEXTDECORATIONTYPE, PdfName.UNDERLINE);
            }

            if (attributes.ContainsKey("BACKGROUND"))
            {
                BaseColor baseColor = (BaseColor)((object[])attributes["BACKGROUND"])[0];
                SetAttribute(PdfName.BACKGROUNDCOLOR, new PdfArray(new float[3]
                {
                    (float)baseColor.R / 255f,
                    (float)baseColor.G / 255f,
                    (float)baseColor.B / 255f
                }));
            }

            IPdfStructureElement pdfStructureElement = (IPdfStructureElement)GetParent(includeStructTreeRoot: true);
            PdfObject parentAttribute = GetParentAttribute(pdfStructureElement, PdfName.COLOR);
            if (chunk.Font != null && chunk.Font.Color != null)
            {
                BaseColor color = chunk.Font.Color;
                SetColorAttribute(color, parentAttribute, PdfName.COLOR);
            }

            PdfObject parentAttribute2 = GetParentAttribute(pdfStructureElement, PdfName.TEXTDECORATIONTHICKNESS);
            PdfObject parentAttribute3 = GetParentAttribute(pdfStructureElement, PdfName.TEXTDECORATIONCOLOR);
            if (attributes.ContainsKey("UNDERLINE"))
            {
                object[] obj = ((object[][])attributes["UNDERLINE"])[^1];
                BaseColor baseColor2 = (BaseColor)obj[0];
                float value = ((float[])obj[1])[0];
                if (parentAttribute2 is PdfNumber)
                {
                    float floatValue = ((PdfNumber)parentAttribute2).FloatValue;
                    if (value.CompareTo(floatValue) != 0)
                    {
                        SetAttribute(PdfName.TEXTDECORATIONTHICKNESS, new PdfNumber(value));
                    }
                }
                else
                {
                    SetAttribute(PdfName.TEXTDECORATIONTHICKNESS, new PdfNumber(value));
                }

                if (baseColor2 != null)
                {
                    SetColorAttribute(baseColor2, parentAttribute3, PdfName.TEXTDECORATIONCOLOR);
                }
            }

            if (!attributes.ContainsKey("LINEHEIGHT"))
            {
                return;
            }

            float value2 = (float)attributes["LINEHEIGHT"];
            PdfObject parentAttribute4 = GetParentAttribute(pdfStructureElement, PdfName.LINEHEIGHT);
            if (parentAttribute4 is PdfNumber)
            {
                if (((PdfNumber)parentAttribute4).FloatValue.CompareTo(value2) != 0)
                {
                    SetAttribute(PdfName.LINEHEIGHT, new PdfNumber(value2));
                }
            }
            else
            {
                SetAttribute(PdfName.LINEHEIGHT, new PdfNumber(value2));
            }
        }

        private void WriteAttributes(Image image)
        {
            if (image != null)
            {
                SetAttribute(PdfName.O, PdfName.LAYOUT);
                if (image.Width > 0f)
                {
                    SetAttribute(PdfName.WIDTH, new PdfNumber(image.Width));
                }

                if (image.Height > 0f)
                {
                    SetAttribute(PdfName.HEIGHT, new PdfNumber(image.Height));
                }

                PdfRectangle obj = new PdfRectangle(image, ((Rectangle)image).Rotation);
                SetAttribute(PdfName.BBOX, obj);
                if (image.BackgroundColor != null)
                {
                    BaseColor backgroundColor = image.BackgroundColor;
                    SetAttribute(PdfName.BACKGROUNDCOLOR, new PdfArray(new float[3]
                    {
                        (float)backgroundColor.R / 255f,
                        (float)backgroundColor.G / 255f,
                        (float)backgroundColor.B / 255f
                    }));
                }
            }
        }

        private void WriteAttributes(PdfTemplate template)
        {
            if (template != null)
            {
                SetAttribute(PdfName.O, PdfName.LAYOUT);
                if (template.Width > 0f)
                {
                    SetAttribute(PdfName.WIDTH, new PdfNumber(template.Width));
                }

                if (template.Height > 0f)
                {
                    SetAttribute(PdfName.HEIGHT, new PdfNumber(template.Height));
                }

                PdfRectangle obj = new PdfRectangle(template.BoundingBox);
                SetAttribute(PdfName.BBOX, obj);
            }
        }

        private void WriteAttributes(Paragraph paragraph)
        {
            if (paragraph == null)
            {
                return;
            }

            SetAttribute(PdfName.O, PdfName.LAYOUT);
            if (paragraph.SpacingBefore.CompareTo(0f) != 0)
            {
                SetAttribute(PdfName.SPACEBEFORE, new PdfNumber(paragraph.SpacingBefore));
            }

            if (paragraph.SpacingAfter.CompareTo(0f) != 0)
            {
                SetAttribute(PdfName.SPACEAFTER, new PdfNumber(paragraph.SpacingAfter));
            }

            IPdfStructureElement pdfStructureElement = (IPdfStructureElement)GetParent(includeStructTreeRoot: true);
            PdfObject parentAttribute = GetParentAttribute(pdfStructureElement, PdfName.COLOR);
            if (paragraph.Font != null && paragraph.Font.Color != null)
            {
                BaseColor color = paragraph.Font.Color;
                SetColorAttribute(color, parentAttribute, PdfName.COLOR);
            }

            parentAttribute = GetParentAttribute(pdfStructureElement, PdfName.TEXTINDENT);
            if (paragraph.FirstLineIndent.CompareTo(0f) != 0)
            {
                bool flag = true;
                if (parentAttribute is PdfNumber && ((PdfNumber)parentAttribute).FloatValue.CompareTo(paragraph.FirstLineIndent) == 0)
                {
                    flag = false;
                }

                if (flag)
                {
                    SetAttribute(PdfName.TEXTINDENT, new PdfNumber(paragraph.FirstLineIndent));
                }
            }

            parentAttribute = GetParentAttribute(pdfStructureElement, PdfName.STARTINDENT);
            if (parentAttribute is PdfNumber)
            {
                if (((PdfNumber)parentAttribute).FloatValue.CompareTo(paragraph.IndentationLeft) != 0)
                {
                    SetAttribute(PdfName.STARTINDENT, new PdfNumber(paragraph.IndentationLeft));
                }
            }
            else if (Math.Abs(paragraph.IndentationLeft) > float.Epsilon)
            {
                SetAttribute(PdfName.STARTINDENT, new PdfNumber(paragraph.IndentationLeft));
            }

            parentAttribute = GetParentAttribute(pdfStructureElement, PdfName.ENDINDENT);
            if (parentAttribute is PdfNumber)
            {
                if (((PdfNumber)parentAttribute).FloatValue.CompareTo(paragraph.IndentationRight) != 0)
                {
                    SetAttribute(PdfName.ENDINDENT, new PdfNumber(paragraph.IndentationRight));
                }
            }
            else if (paragraph.IndentationRight.CompareTo(0f) != 0)
            {
                SetAttribute(PdfName.ENDINDENT, new PdfNumber(paragraph.IndentationRight));
            }

            SetTextAlignAttribute(paragraph.Alignment);
        }

        private void WriteAttributes(List list)
        {
            if (list == null)
            {
                return;
            }

            SetAttribute(PdfName.O, PdfName.LIST);
            if (list.Autoindent)
            {
                if (list.Numbered)
                {
                    if (list.Lettered)
                    {
                        if (list.IsLowercase)
                        {
                            SetAttribute(PdfName.LISTNUMBERING, PdfName.LOWERROMAN);
                        }
                        else
                        {
                            SetAttribute(PdfName.LISTNUMBERING, PdfName.UPPERROMAN);
                        }
                    }
                    else
                    {
                        SetAttribute(PdfName.LISTNUMBERING, PdfName.DECIMAL);
                    }
                }
                else if (list.Lettered)
                {
                    if (list.IsLowercase)
                    {
                        SetAttribute(PdfName.LISTNUMBERING, PdfName.LOWERALPHA);
                    }
                    else
                    {
                        SetAttribute(PdfName.LISTNUMBERING, PdfName.UPPERALPHA);
                    }
                }
            }

            PdfObject parentAttribute = GetParentAttribute(parent, PdfName.STARTINDENT);
            if (parentAttribute is PdfNumber)
            {
                if (((PdfNumber)parentAttribute).FloatValue != list.IndentationLeft)
                {
                    SetAttribute(PdfName.STARTINDENT, new PdfNumber(list.IndentationLeft));
                }
            }
            else if (Math.Abs(list.IndentationLeft) > float.Epsilon)
            {
                SetAttribute(PdfName.STARTINDENT, new PdfNumber(list.IndentationLeft));
            }

            parentAttribute = GetParentAttribute(parent, PdfName.ENDINDENT);
            if (parentAttribute is PdfNumber)
            {
                if (((PdfNumber)parentAttribute).FloatValue != list.IndentationRight)
                {
                    SetAttribute(PdfName.ENDINDENT, new PdfNumber(list.IndentationRight));
                }
            }
            else if (list.IndentationRight > float.Epsilon)
            {
                SetAttribute(PdfName.ENDINDENT, new PdfNumber(list.IndentationRight));
            }
        }

        private void WriteAttributes(ListItem listItem)
        {
            if (listItem == null)
            {
                return;
            }

            PdfObject parentAttribute = parent.GetParentAttribute(parent, PdfName.STARTINDENT);
            if (parentAttribute is PdfNumber)
            {
                if (((PdfNumber)parentAttribute).FloatValue.CompareTo(listItem.IndentationLeft) != 0)
                {
                    SetAttribute(PdfName.STARTINDENT, new PdfNumber(listItem.IndentationLeft));
                }
            }
            else if (Math.Abs(listItem.IndentationLeft) > float.Epsilon)
            {
                SetAttribute(PdfName.STARTINDENT, new PdfNumber(listItem.IndentationLeft));
            }

            parentAttribute = GetParentAttribute(parent, PdfName.ENDINDENT);
            if (parentAttribute is PdfNumber)
            {
                if (((PdfNumber)parentAttribute).FloatValue.CompareTo(listItem.IndentationRight) != 0)
                {
                    SetAttribute(PdfName.ENDINDENT, new PdfNumber(listItem.IndentationRight));
                }
            }
            else if (Math.Abs(listItem.IndentationRight) > float.Epsilon)
            {
                SetAttribute(PdfName.ENDINDENT, new PdfNumber(listItem.IndentationRight));
            }
        }

        private void WriteAttributes(ListBody listBody)
        {
        }

        private void WriteAttributes(ListLabel listLabel)
        {
            if (listLabel == null)
            {
                return;
            }

            PdfObject parentAttribute = GetParentAttribute(parent, PdfName.STARTINDENT);
            if (parentAttribute is PdfNumber)
            {
                if (((PdfNumber)parentAttribute).FloatValue != listLabel.Indentation)
                {
                    SetAttribute(PdfName.STARTINDENT, new PdfNumber(listLabel.Indentation));
                }
            }
            else if (Math.Abs(listLabel.Indentation) > float.Epsilon)
            {
                SetAttribute(PdfName.STARTINDENT, new PdfNumber(listLabel.Indentation));
            }
        }

        private void WriteAttributes(PdfPTable table)
        {
            if (table != null)
            {
                SetAttribute(PdfName.O, PdfName.TABLE);
                if (table.SpacingBefore > float.Epsilon)
                {
                    SetAttribute(PdfName.SPACEBEFORE, new PdfNumber(table.SpacingBefore));
                }

                if (table.SpacingAfter > float.Epsilon)
                {
                    SetAttribute(PdfName.SPACEAFTER, new PdfNumber(table.SpacingAfter));
                }

                if (table.TotalHeight > 0f)
                {
                    SetAttribute(PdfName.HEIGHT, new PdfNumber(table.TotalHeight));
                }

                if (table.TotalWidth > 0f)
                {
                    SetAttribute(PdfName.WIDTH, new PdfNumber(table.TotalWidth));
                }
            }
        }

        private void WriteAttributes(PdfPRow row)
        {
            if (row != null)
            {
                SetAttribute(PdfName.O, PdfName.TABLE);
            }
        }

        private void WriteAttributes(PdfPCell cell)
        {
            if (cell == null)
            {
                return;
            }

            SetAttribute(PdfName.O, PdfName.TABLE);
            if (cell.Colspan != 1)
            {
                SetAttribute(PdfName.COLSPAN, new PdfNumber(cell.Colspan));
            }

            if (cell.Rowspan != 1)
            {
                SetAttribute(PdfName.ROWSPAN, new PdfNumber(cell.Rowspan));
            }

            if (cell.Headers != null)
            {
                PdfArray pdfArray = new PdfArray();
                foreach (PdfPHeaderCell header in cell.Headers)
                {
                    if (header.Name != null)
                    {
                        pdfArray.Add(new PdfString(header.Name));
                    }
                }

                if (!pdfArray.IsEmpty())
                {
                    SetAttribute(PdfName.HEADERS, pdfArray);
                }
            }

            if (cell.CalculatedHeight > 0f)
            {
                SetAttribute(PdfName.HEIGHT, new PdfNumber(cell.CalculatedHeight));
            }

            if (cell.Width > 0f)
            {
                SetAttribute(PdfName.WIDTH, new PdfNumber(cell.Width));
            }

            if (cell.BackgroundColor != null)
            {
                BaseColor backgroundColor = cell.BackgroundColor;
                SetAttribute(PdfName.BACKGROUNDCOLOR, new PdfArray(new float[3]
                {
                    (float)backgroundColor.R / 255f,
                    (float)backgroundColor.G / 255f,
                    (float)backgroundColor.B / 255f
                }));
            }
        }

        private void WriteAttributes(PdfPHeaderCell headerCell)
        {
            if (headerCell == null)
            {
                return;
            }

            if (headerCell.Scope != 0)
            {
                switch (headerCell.Scope)
                {
                    case 1:
                        SetAttribute(PdfName.SCOPE, PdfName.ROW);
                        break;
                    case 2:
                        SetAttribute(PdfName.SCOPE, PdfName.COLUMN);
                        break;
                    case 3:
                        SetAttribute(PdfName.SCOPE, PdfName.BOTH);
                        break;
                }
            }

            if (headerCell.Name != null)
            {
                SetAttribute(PdfName.NAME, new PdfName(headerCell.Name));
            }

            WriteAttributes((PdfPCell)headerCell);
        }

        private void WriteAttributes(PdfPTableHeader header)
        {
            if (header != null)
            {
                SetAttribute(PdfName.O, PdfName.TABLE);
            }
        }

        private void WriteAttributes(PdfPTableBody body)
        {
        }

        private void WriteAttributes(PdfPTableFooter footer)
        {
        }

        private void WriteAttributes(PdfDiv div)
        {
            if (div != null)
            {
                if (div.BackgroundColor != null)
                {
                    SetColorAttribute(div.BackgroundColor, null, PdfName.BACKGROUNDCOLOR);
                }

                SetTextAlignAttribute(div.TextAlignment);
            }
        }

        private void WriteAttributes(Document document)
        {
        }

        private bool ColorsEqual(PdfArray parentColor, float[] color)
        {
            if (color[0].CompareTo(parentColor.GetAsNumber(0).FloatValue) != 0)
            {
                return false;
            }

            if (color[1].CompareTo(parentColor.GetAsNumber(1).FloatValue) != 0)
            {
                return false;
            }

            if (color[2].CompareTo(parentColor.GetAsNumber(2).FloatValue) != 0)
            {
                return false;
            }

            return true;
        }

        private void SetColorAttribute(BaseColor newColor, PdfObject oldColor, PdfName attributeName)
        {
            float[] array = new float[3]
            {
                (float)newColor.R / 255f,
                (float)newColor.G / 255f,
                (float)newColor.B / 255f
            };
            if (oldColor != null && oldColor is PdfArray)
            {
                PdfArray parentColor = (PdfArray)oldColor;
                if (ColorsEqual(parentColor, array))
                {
                    SetAttribute(attributeName, new PdfArray(array));
                }
                else
                {
                    SetAttribute(attributeName, new PdfArray(array));
                }
            }
            else
            {
                SetAttribute(attributeName, new PdfArray(array));
            }
        }

        private void SetTextAlignAttribute(int elementAlign)
        {
            PdfName pdfName = null;
            switch (elementAlign)
            {
                case 0:
                    pdfName = PdfName.START;
                    break;
                case 1:
                    pdfName = PdfName.CENTER;
                    break;
                case 2:
                    pdfName = PdfName.END;
                    break;
                case 3:
                    pdfName = PdfName.JUSTIFY;
                    break;
            }

            PdfObject parentAttribute = GetParentAttribute(parent, PdfName.TEXTALIGN);
            if (parentAttribute is PdfName)
            {
                PdfName pdfName2 = (PdfName)parentAttribute;
                if (pdfName != null && !pdfName2.Equals(pdfName))
                {
                    SetAttribute(PdfName.TEXTALIGN, pdfName);
                }
            }
            else if (pdfName != null && !PdfName.START.Equals(pdfName))
            {
                SetAttribute(PdfName.TEXTALIGN, pdfName);
            }
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            PdfWriter.CheckPdfIsoConformance(writer, 16, this);
            base.ToPdf(writer, os);
        }

        private PdfObject GetParentAttribute(IPdfStructureElement parent, PdfName name)
        {
            return parent?.GetAttribute(name);
        }
    }
}
