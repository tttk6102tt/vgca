using Sign.itext.pdf;
using Sign.itext.text.log;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class FdfReader : PdfReader
    {
        internal Dictionary<string, PdfDictionary> fields;

        internal string fileSpec;

        internal PdfName encoding;

        protected new static ICounter COUNTER = CounterFactory.GetCounter(typeof(FdfReader));

        public virtual Dictionary<string, PdfDictionary> Fields => fields;

        public virtual string FileSpec => fileSpec;

        public FdfReader(string filename)
            : base(filename)
        {
        }

        public FdfReader(byte[] pdfIn)
            : base(pdfIn)
        {
        }

        public FdfReader(Uri url)
            : base(url)
        {
        }

        public FdfReader(Stream isp)
            : base(isp)
        {
        }

        protected override ICounter GetCounter()
        {
            return COUNTER;
        }

        protected internal override void ReadPdf()
        {
            fields = new Dictionary<string, PdfDictionary>();
            tokens.CheckFdfHeader();
            RebuildXref();
            ReadDocObj();
            ReadFields();
        }

        protected virtual void KidNode(PdfDictionary merged, string name)
        {
            PdfArray asArray = merged.GetAsArray(PdfName.KIDS);
            if (asArray == null || asArray.Size == 0)
            {
                if (name.Length > 0)
                {
                    name = name.Substring(1);
                }

                fields[name] = merged;
                return;
            }

            merged.Remove(PdfName.KIDS);
            for (int i = 0; i < asArray.Size; i++)
            {
                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Merge(merged);
                PdfDictionary asDict = asArray.GetAsDict(i);
                PdfString asString = asDict.GetAsString(PdfName.T);
                string text = name;
                if (asString != null)
                {
                    text = text + "." + asString.ToUnicodeString();
                }

                pdfDictionary.Merge(asDict);
                pdfDictionary.Remove(PdfName.T);
                KidNode(pdfDictionary, text);
            }
        }

        protected virtual void ReadFields()
        {
            catalog = trailer.GetAsDict(PdfName.ROOT);
            PdfDictionary asDict = catalog.GetAsDict(PdfName.FDF);
            if (asDict != null)
            {
                PdfString asString = asDict.GetAsString(PdfName.F);
                if (asString != null)
                {
                    fileSpec = asString.ToUnicodeString();
                }

                PdfArray asArray = asDict.GetAsArray(PdfName.FIELDS);
                if (asArray != null)
                {
                    encoding = asDict.GetAsName(PdfName.ENCODING);
                    PdfDictionary pdfDictionary = new PdfDictionary();
                    pdfDictionary.Put(PdfName.KIDS, asArray);
                    KidNode(pdfDictionary, "");
                }
            }
        }

        public virtual PdfDictionary GetField(string name)
        {
            fields.TryGetValue(name, out var value);
            return value;
        }

        public virtual byte[] GetAttachedFile(string name)
        {
            PdfDictionary field = GetField(name);
            if (field != null)
            {
                PdfIndirectReference pdfIndirectReference = (PRIndirectReference)field.Get(PdfName.V);
                pdfIndirectReference = (PRIndirectReference)((PdfDictionary)GetPdfObject(pdfIndirectReference.Number)).GetAsDict(PdfName.EF).Get(PdfName.F);
                return PdfReader.GetStreamBytes((PRStream)GetPdfObject(pdfIndirectReference.Number));
            }

            return new byte[0];
        }

        public virtual string GetFieldValue(string name)
        {
            PdfDictionary field = GetField(name);
            if (field == null)
            {
                return null;
            }

            PdfObject pdfObject = PdfReader.GetPdfObject(field.Get(PdfName.V));
            if (pdfObject == null)
            {
                return null;
            }

            if (pdfObject.IsName())
            {
                return PdfName.DecodeName(((PdfName)pdfObject).ToString());
            }

            if (pdfObject.IsString())
            {
                PdfString pdfString = (PdfString)pdfObject;
                if (encoding == null || pdfString.Encoding != null)
                {
                    return pdfString.ToUnicodeString();
                }

                byte[] bytes = pdfString.GetBytes();
                if (bytes.Length >= 2 && bytes[0] == 254 && bytes[1] == byte.MaxValue)
                {
                    return pdfString.ToUnicodeString();
                }

                try
                {
                    if (encoding.Equals(PdfName.SHIFT_JIS))
                    {
                        return Encoding.GetEncoding(932).GetString(bytes);
                    }

                    if (encoding.Equals(PdfName.UHC))
                    {
                        return Encoding.GetEncoding(949).GetString(bytes);
                    }

                    if (encoding.Equals(PdfName.GBK))
                    {
                        return Encoding.GetEncoding(936).GetString(bytes);
                    }

                    if (encoding.Equals(PdfName.BIGFIVE))
                    {
                        return Encoding.GetEncoding(950).GetString(bytes);
                    }

                    if (encoding.Equals(PdfName.UTF_8))
                    {
                        return Encoding.UTF8.GetString(bytes);
                    }
                }
                catch
                {
                }

                return pdfString.ToUnicodeString();
            }

            return null;
        }
    }
}
