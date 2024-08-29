using Sign.itext.pdf;

namespace Sign.itext.text.pdf.intern
{
    public class PdfAnnotationsImp
    {
        protected internal PdfAcroForm acroForm;

        protected internal List<PdfAnnotation> annotations;

        protected internal List<PdfAnnotation> delayedAnnotations = new List<PdfAnnotation>();

        public virtual PdfAcroForm AcroForm => acroForm;

        public virtual int SigFlags
        {
            set
            {
                acroForm.SigFlags = value;
            }
        }

        public PdfAnnotationsImp(PdfWriter writer)
        {
            acroForm = new PdfAcroForm(writer);
        }

        public virtual bool HasValidAcroForm()
        {
            return acroForm.IsValid();
        }

        public virtual void AddCalculationOrder(PdfFormField formField)
        {
            acroForm.AddCalculationOrder(formField);
        }

        public virtual void AddAnnotation(PdfAnnotation annot)
        {
            if (annot.IsForm())
            {
                PdfFormField pdfFormField = (PdfFormField)annot;
                if (pdfFormField.Parent == null)
                {
                    AddFormFieldRaw(pdfFormField);
                }
            }
            else
            {
                annotations.Add(annot);
            }
        }

        public virtual void AddPlainAnnotation(PdfAnnotation annot)
        {
            annotations.Add(annot);
        }

        private void AddFormFieldRaw(PdfFormField field)
        {
            annotations.Add(field);
            List<PdfFormField> kids = field.Kids;
            if (kids != null)
            {
                for (int i = 0; i < kids.Count; i++)
                {
                    AddFormFieldRaw(kids[i]);
                }
            }
        }

        public virtual bool HasUnusedAnnotations()
        {
            return annotations.Count > 0;
        }

        public virtual void ResetAnnotations()
        {
            annotations = delayedAnnotations;
            delayedAnnotations = new List<PdfAnnotation>();
        }

        public virtual PdfArray RotateAnnotations(PdfWriter writer, Rectangle pageSize)
        {
            PdfArray pdfArray = new PdfArray();
            int num = pageSize.Rotation % 360;
            int currentPageNumber = writer.CurrentPageNumber;
            for (int i = 0; i < annotations.Count; i++)
            {
                PdfAnnotation pdfAnnotation = annotations[i];
                if (pdfAnnotation.PlaceInPage > currentPageNumber)
                {
                    delayedAnnotations.Add(pdfAnnotation);
                    continue;
                }

                if (pdfAnnotation.IsForm())
                {
                    if (!pdfAnnotation.IsUsed())
                    {
                        Dictionary<PdfTemplate, object> templates = pdfAnnotation.Templates;
                        if (templates != null)
                        {
                            acroForm.AddFieldTemplates(templates);
                        }
                    }

                    PdfFormField pdfFormField = (PdfFormField)pdfAnnotation;
                    if (pdfFormField.Parent == null)
                    {
                        acroForm.AddDocumentField(pdfFormField.IndirectReference);
                    }
                }

                if (pdfAnnotation.IsAnnotation())
                {
                    pdfArray.Add(pdfAnnotation.IndirectReference);
                    if (!pdfAnnotation.IsUsed())
                    {
                        PdfArray asArray = pdfAnnotation.GetAsArray(PdfName.RECT);
                        PdfRectangle pdfRectangle = ((asArray.Size != 4) ? new PdfRectangle(asArray.GetAsNumber(0).FloatValue, asArray.GetAsNumber(1).FloatValue) : new PdfRectangle(asArray.GetAsNumber(0).FloatValue, asArray.GetAsNumber(1).FloatValue, asArray.GetAsNumber(2).FloatValue, asArray.GetAsNumber(3).FloatValue));
                        if (pdfRectangle != null)
                        {
                            switch (num)
                            {
                                case 90:
                                    pdfAnnotation.Put(PdfName.RECT, new PdfRectangle(pageSize.Top - pdfRectangle.Bottom, pdfRectangle.Left, pageSize.Top - pdfRectangle.Top, pdfRectangle.Right));
                                    break;
                                case 180:
                                    pdfAnnotation.Put(PdfName.RECT, new PdfRectangle(pageSize.Right - pdfRectangle.Left, pageSize.Top - pdfRectangle.Bottom, pageSize.Right - pdfRectangle.Right, pageSize.Top - pdfRectangle.Top));
                                    break;
                                case 270:
                                    pdfAnnotation.Put(PdfName.RECT, new PdfRectangle(pdfRectangle.Bottom, pageSize.Right - pdfRectangle.Left, pdfRectangle.Top, pageSize.Right - pdfRectangle.Right));
                                    break;
                            }
                        }
                    }
                }

                if (!pdfAnnotation.IsUsed())
                {
                    pdfAnnotation.SetUsed();
                    writer.AddToBody(pdfAnnotation, pdfAnnotation.IndirectReference);
                }
            }

            return pdfArray;
        }

        public static PdfAnnotation ConvertAnnotation(PdfWriter writer, Annotation annot, Rectangle defaultRect)
        {
            switch (annot.AnnotationType)
            {
                case 1:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction((Uri)annot.Attributes["url"]), null);
                case 2:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction((string)annot.Attributes["file"]), null);
                case 3:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction((string)annot.Attributes["file"], (string)annot.Attributes["destination"]), null);
                case 7:
                    {
                        bool[] array = (bool[])annot.Attributes["parameters"];
                        string text = (string)annot.Attributes["file"];
                        string mimeType = (string)annot.Attributes["mime"];
                        return PdfAnnotation.CreateScreen(fs: (!array[0]) ? PdfFileSpecification.FileExtern(writer, text) : PdfFileSpecification.FileEmbedded(writer, text, text, null), writer: writer, rect: new Rectangle(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry()), clipTitle: text, mimeType: mimeType, playOnDisplay: array[1]);
                    }
                case 4:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction((string)annot.Attributes["file"], ((int?)annot.Attributes["page"]).Value), null);
                case 5:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction(((int?)annot.Attributes["named"]).Value), null);
                case 6:
                    return writer.CreateAnnotation(annot.GetLlx(), annot.GetLly(), annot.GetUrx(), annot.GetUry(), new PdfAction((string)annot.Attributes["application"], (string)annot.Attributes["parameters"], (string)annot.Attributes["operation"], (string)annot.Attributes["defaultdir"]), null);
                default:
                    return writer.CreateAnnotation(defaultRect.Left, defaultRect.Bottom, defaultRect.Right, defaultRect.Top, new PdfString(annot.Title, "UnicodeBig"), new PdfString(annot.Content, "UnicodeBig"), null);
            }
        }
    }
}
