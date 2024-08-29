using Sign.itext.error_messages;
using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfSpotColor : ICachedColorSpace, IPdfSpecialColorSpace
    {
        public PdfName name;

        public BaseColor altcs;

        public ColorDetails altColorDetails;

        public virtual BaseColor AlternativeCS => altcs;

        public virtual PdfName Name => name;

        public PdfSpotColor(string name, BaseColor altcs)
        {
            this.name = new PdfName(name);
            this.altcs = altcs;
        }

        public virtual ColorDetails[] GetColorantDetails(PdfWriter writer)
        {
            if (altColorDetails == null && altcs is ExtendedColor && ((ExtendedColor)altcs).Type == 7)
            {
                altColorDetails = writer.AddSimple(((LabColor)altcs).LabColorSpace);
            }

            return new ColorDetails[1] { altColorDetails };
        }

        [Obsolete]
        protected internal virtual PdfObject GetSpotObject(PdfWriter writer)
        {
            return GetPdfObject(writer);
        }

        public virtual PdfObject GetPdfObject(PdfWriter writer)
        {
            PdfArray pdfArray = new PdfArray(PdfName.SEPARATION);
            pdfArray.Add(name);
            PdfFunction pdfFunction = null;
            if (altcs is ExtendedColor)
            {
                switch (((ExtendedColor)altcs).Type)
                {
                    case 1:
                        pdfArray.Add(PdfName.DEVICEGRAY);
                        pdfFunction = PdfFunction.Type2(writer, new float[2] { 0f, 1f }, null, new float[1] { 1f }, new float[1] { ((GrayColor)altcs).Gray }, 1f);
                        break;
                    case 2:
                        {
                            pdfArray.Add(PdfName.DEVICECMYK);
                            CMYKColor cMYKColor = (CMYKColor)altcs;
                            pdfFunction = PdfFunction.Type2(writer, new float[2] { 0f, 1f }, null, new float[4], new float[4] { cMYKColor.Cyan, cMYKColor.Magenta, cMYKColor.Yellow, cMYKColor.Black }, 1f);
                            break;
                        }
                    case 7:
                        {
                            LabColor labColor = (LabColor)altcs;
                            if (altColorDetails != null)
                            {
                                pdfArray.Add(altColorDetails.IndirectReference);
                            }
                            else
                            {
                                pdfArray.Add(labColor.LabColorSpace.GetPdfObject(writer));
                            }

                            pdfFunction = PdfFunction.Type2(writer, new float[2] { 0f, 1f }, null, new float[3] { 100f, 0f, 0f }, new float[3] { labColor.L, labColor.A, labColor.B }, 1f);
                            break;
                        }
                    default:
                        throw new Exception(MessageLocalization.GetComposedMessage("only.rgb.gray.and.cmyk.are.supported.as.alternative.color.spaces"));
                }
            }
            else
            {
                pdfArray.Add(PdfName.DEVICERGB);
                pdfFunction = PdfFunction.Type2(writer, new float[2] { 0f, 1f }, null, new float[3] { 1f, 1f, 1f }, new float[3]
                {
                    (float)altcs.R / 255f,
                    (float)altcs.G / 255f,
                    (float)altcs.B / 255f
                }, 1f);
            }

            pdfArray.Add(pdfFunction.Reference);
            return pdfArray;
        }

        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }

            if (!(o is PdfSpotColor))
            {
                return false;
            }

            PdfSpotColor pdfSpotColor = (PdfSpotColor)o;
            if (!altcs.Equals(pdfSpotColor.altcs))
            {
                return false;
            }

            if (!name.Equals(pdfSpotColor.name))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hashCode = name.GetHashCode();
            return 31 * hashCode + altcs.GetHashCode();
        }
    }
}
