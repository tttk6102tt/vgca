using Sign.itext.pdf;
using Sign.itext.text.pdf.security;

namespace Sign.itext.text.pdf
{
    public class PdfSignature : PdfDictionary
    {
        public virtual int[] ByteRange
        {
            set
            {
                PdfArray pdfArray = new PdfArray();
                for (int i = 0; i < value.Length; i++)
                {
                    pdfArray.Add(new PdfNumber(value[i]));
                }

                Put(PdfName.BYTERANGE, pdfArray);
            }
        }

        public virtual byte[] Contents
        {
            set
            {
                Put(PdfName.CONTENTS, new PdfString(value).SetHexWriting(hexWriting: true));
            }
        }

        public virtual byte[] Cert
        {
            set
            {
                Put(PdfName.CERT, new PdfString(value));
            }
        }

        public virtual string Name
        {
            set
            {
                Put(PdfName.NAME, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual PdfDate Date
        {
            set
            {
                Put(PdfName.M, value);
            }
        }

        public virtual string Location
        {
            set
            {
                Put(PdfName.LOCATION, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual string Reason
        {
            set
            {
                Put(PdfName.REASON, new PdfString(value, "UnicodeBig"));
            }
        }

        public virtual string SignatureCreator
        {
            set
            {
                if (value != null)
                {
                    PdfSignatureBuildProperties.SignatureCreator = value;
                }
            }
        }

        private PdfSignatureBuildProperties PdfSignatureBuildProperties
        {
            get
            {
                PdfSignatureBuildProperties pdfSignatureBuildProperties = (PdfSignatureBuildProperties)GetAsDict(PdfName.PROP_BUILD);
                if (pdfSignatureBuildProperties == null)
                {
                    pdfSignatureBuildProperties = new PdfSignatureBuildProperties();
                    Put(PdfName.PROP_BUILD, pdfSignatureBuildProperties);
                }

                return pdfSignatureBuildProperties;
            }
        }

        public virtual string Contact
        {
            set
            {
                Put(PdfName.CONTACTINFO, new PdfString(value, "UnicodeBig"));
            }
        }

        public PdfSignature(PdfName filter, PdfName subFilter)
            : base(PdfName.SIG)
        {
            Put(PdfName.FILTER, filter);
            Put(PdfName.SUBFILTER, subFilter);
        }
    }
}
