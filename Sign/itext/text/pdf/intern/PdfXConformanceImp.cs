using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.text.pdf.interfaces;

namespace Sign.itext.text.pdf.intern
{
    public class PdfXConformanceImp : IPdfXConformance, IPdfIsoConformance
    {
        protected internal int pdfxConformance;

        protected PdfWriter writer;

        public virtual int PDFXConformance
        {
            get
            {
                return pdfxConformance;
            }
            set
            {
                pdfxConformance = value;
            }
        }

        public PdfXConformanceImp(PdfWriter writer)
        {
            this.writer = writer;
        }

        public virtual bool IsPdfIso()
        {
            return IsPdfX();
        }

        public virtual bool IsPdfX()
        {
            return pdfxConformance != 0;
        }

        public virtual bool IsPdfX1A2001()
        {
            return pdfxConformance == 1;
        }

        public virtual bool IsPdfX32002()
        {
            return pdfxConformance == 2;
        }

        public virtual void CheckPdfIsoConformance(int key, object obj1)
        {
            if (writer == null || !writer.IsPdfX())
            {
                return;
            }

            int pDFXConformance = writer.PDFXConformance;
            switch (key)
            {
                case 1:
                    if (pDFXConformance != 1)
                    {
                        break;
                    }

                    if (obj1 is ExtendedColor)
                    {
                        ExtendedColor extendedColor = (ExtendedColor)obj1;
                        switch (extendedColor.Type)
                        {
                            case 1:
                            case 2:
                                break;
                            case 0:
                                throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("colorspace.rgb.is.not.allowed"));
                            case 3:
                                {
                                    SpotColor spotColor = (SpotColor)extendedColor;
                                    CheckPdfIsoConformance(1, spotColor.PdfSpotColor.AlternativeCS);
                                    break;
                                }
                            case 5:
                                {
                                    ShadingColor shadingColor = (ShadingColor)extendedColor;
                                    CheckPdfIsoConformance(1, shadingColor.PdfShadingPattern.Shading.ColorSpace);
                                    break;
                                }
                            case 4:
                                {
                                    PatternColor patternColor = (PatternColor)extendedColor;
                                    CheckPdfIsoConformance(1, patternColor.Painter.DefaultColor);
                                    break;
                                }
                        }
                    }
                    else if (obj1 is BaseColor)
                    {
                        throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("colorspace.rgb.is.not.allowed"));
                    }

                    break;
                case 3:
                    if (pDFXConformance == 1)
                    {
                        throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("colorspace.rgb.is.not.allowed"));
                    }

                    break;
                case 4:
                    if (!((BaseFont)obj1).IsEmbedded())
                    {
                        throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("all.the.fonts.must.be.embedded.this.one.isn.t.1", ((BaseFont)obj1).PostscriptFontName));
                    }

                    break;
                case 5:
                    {
                        PdfImage pdfImage = (PdfImage)obj1;
                        if (pdfImage.Get(PdfName.SMASK) != null)
                        {
                            throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("the.smask.key.is.not.allowed.in.images"));
                        }

                        if (pDFXConformance != 1)
                        {
                            break;
                        }

                        PdfObject pdfObject2 = pdfImage.Get(PdfName.COLORSPACE);
                        if (pdfObject2 == null)
                        {
                            break;
                        }

                        if (pdfObject2.IsName())
                        {
                            if (PdfName.DEVICERGB.Equals(pdfObject2))
                            {
                                throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("colorspace.rgb.is.not.allowed"));
                            }
                        }
                        else if (pdfObject2.IsArray() && PdfName.CALRGB.Equals(((PdfArray)pdfObject2)[0]))
                        {
                            throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("colorspace.calrgb.is.not.allowed"));
                        }

                        break;
                    }
                case 6:
                    {
                        PdfDictionary pdfDictionary = (PdfDictionary)obj1;
                        if (pdfDictionary != null)
                        {
                            PdfObject pdfObject = pdfDictionary.Get(PdfName.BM);
                            if (pdfObject != null && !PdfGState.BM_NORMAL.Equals(pdfObject) && !PdfGState.BM_COMPATIBLE.Equals(pdfObject))
                            {
                                throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("blend.mode.1.not.allowed", pdfObject.ToString()));
                            }

                            pdfObject = pdfDictionary.Get(PdfName.CA);
                            double num = 0.0;
                            if (pdfObject != null && (num = ((PdfNumber)pdfObject).DoubleValue) != 1.0)
                            {
                                throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("transparency.is.not.allowed.ca.eq.1", num));
                            }

                            pdfObject = pdfDictionary.Get(PdfName.ca);
                            num = 0.0;
                            if (pdfObject != null && (num = ((PdfNumber)pdfObject).DoubleValue) != 1.0)
                            {
                                throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("transparency.is.not.allowed.ca.eq.1", num));
                            }
                        }

                        break;
                    }
                case 7:
                    throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("layers.are.not.allowed"));
                case 2:
                    break;
            }
        }
    }
}
