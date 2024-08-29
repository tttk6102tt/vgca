using Sign.itext.pdf;
using Sign.itext.text.pdf.interfaces;

namespace Sign.itext.text.pdf.intern
{
    public class PdfVersionImp : IPdfVersion
    {
        public static readonly byte[][] HEADER = new byte[3][]
        {
            DocWriter.GetISOBytes("\n"),
            DocWriter.GetISOBytes("%PDF-"),
            DocWriter.GetISOBytes("\n%âãÏÓ\n")
        };

        protected bool headerWasWritten;

        protected bool appendmode;

        protected char header_version = '4';

        protected PdfName catalog_version;

        protected char version = '4';

        protected PdfDictionary extensions;

        public virtual char PdfVersion
        {
            set
            {
                version = value;
                if (headerWasWritten || appendmode)
                {
                    SetPdfVersion(GetVersionAsName(value));
                }
                else
                {
                    header_version = value;
                }
            }
        }

        public virtual char Version => version;

        public virtual void SetAtLeastPdfVersion(char version)
        {
            if (version > header_version)
            {
                PdfVersion = version;
            }
        }

        public virtual void SetPdfVersion(PdfName version)
        {
            if (catalog_version == null || catalog_version.CompareTo(version) < 0)
            {
                catalog_version = version;
            }
        }

        public virtual void SetAppendmode(bool appendmode)
        {
            this.appendmode = appendmode;
        }

        public virtual void WriteHeader(OutputStreamCounter os)
        {
            if (appendmode)
            {
                os.Write(HEADER[0], 0, HEADER[0].Length);
                return;
            }

            os.Write(HEADER[1], 0, HEADER[1].Length);
            os.Write(GetVersionAsByteArray(header_version), 0, GetVersionAsByteArray(header_version).Length);
            os.Write(HEADER[2], 0, HEADER[2].Length);
            headerWasWritten = true;
        }

        public virtual PdfName GetVersionAsName(char version)
        {
            return version switch
            {
                '2' => PdfWriter.PDF_VERSION_1_2,
                '3' => PdfWriter.PDF_VERSION_1_3,
                '4' => PdfWriter.PDF_VERSION_1_4,
                '5' => PdfWriter.PDF_VERSION_1_5,
                '6' => PdfWriter.PDF_VERSION_1_6,
                '7' => PdfWriter.PDF_VERSION_1_7,
                _ => PdfWriter.PDF_VERSION_1_4,
            };
        }

        public virtual byte[] GetVersionAsByteArray(char version)
        {
            return DocWriter.GetISOBytes(GetVersionAsName(version).ToString().Substring(1));
        }

        public virtual void AddToCatalog(PdfDictionary catalog)
        {
            if (catalog_version != null)
            {
                catalog.Put(PdfName.VERSION, catalog_version);
            }

            if (extensions != null)
            {
                catalog.Put(PdfName.EXTENSIONS, extensions);
            }
        }

        public virtual void AddDeveloperExtension(PdfDeveloperExtension de)
        {
            if (extensions == null)
            {
                extensions = new PdfDictionary();
            }
            else
            {
                PdfDictionary asDict = extensions.GetAsDict(de.Prefix);
                if (asDict != null && (de.Baseversion.CompareTo(asDict.GetAsName(PdfName.BASEVERSION)) < 0 || de.ExtensionLevel - asDict.GetAsNumber(PdfName.EXTENSIONLEVEL).IntValue <= 0))
                {
                    return;
                }
            }

            extensions.Put(de.Prefix, de.GetDeveloperExtensions());
        }
    }
}
