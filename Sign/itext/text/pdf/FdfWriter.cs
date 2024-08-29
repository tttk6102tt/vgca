using Sign.itext.pdf;
using Sign.itext.text.log;
using Sign.SystemItext.util;

namespace Sign.itext.text.pdf
{
    public class FdfWriter
    {
        internal class Wrt : PdfWriter
        {
            private FdfWriter fdf;

            internal Wrt(Stream os, FdfWriter fdf)
                : base(new PdfDocument(), os)
            {
                this.fdf = fdf;
                base.os.Write(HEADER_FDF, 0, HEADER_FDF.Length);
                body = new PdfBody(this);
            }

            internal void Write()
            {
                foreach (PdfReaderInstance value in readerInstances.Values)
                {
                    PdfReaderInstance pdfReaderInstance = (currentPdfReaderInstance = value);
                    currentPdfReaderInstance.WriteAllPages();
                }

                PdfDictionary pdfDictionary = new PdfDictionary();
                pdfDictionary.Put(PdfName.FIELDS, Calculate(fdf.fields));
                if (fdf.file != null)
                {
                    pdfDictionary.Put(PdfName.F, new PdfString(fdf.file, "UnicodeBig"));
                }

                if (!string.IsNullOrEmpty(fdf.statusMessage))
                {
                    pdfDictionary.Put(PdfName.STATUS, new PdfString(fdf.statusMessage));
                }

                PdfDictionary pdfDictionary2 = new PdfDictionary();
                pdfDictionary2.Put(PdfName.FDF, pdfDictionary);
                PdfIndirectReference indirectReference = AddToBody(pdfDictionary2).IndirectReference;
                byte[] iSOBytes = DocWriter.GetISOBytes("trailer\n");
                os.Write(iSOBytes, 0, iSOBytes.Length);
                PdfDictionary pdfDictionary3 = new PdfDictionary();
                pdfDictionary3.Put(PdfName.ROOT, indirectReference);
                pdfDictionary3.ToPdf(null, os);
                iSOBytes = DocWriter.GetISOBytes("\n%%EOF\n");
                os.Write(iSOBytes, 0, iSOBytes.Length);
                os.Close();
            }

            internal PdfArray Calculate(Dictionary<string, object> map)
            {
                PdfArray pdfArray = new PdfArray();
                foreach (KeyValuePair<string, object> item in map)
                {
                    string key = item.Key;
                    object value = item.Value;
                    PdfDictionary pdfDictionary = new PdfDictionary();
                    pdfDictionary.Put(PdfName.T, new PdfString(key, "UnicodeBig"));
                    if (value is Dictionary<string, object>)
                    {
                        pdfDictionary.Put(PdfName.KIDS, Calculate((Dictionary<string, object>)value));
                    }
                    else if (value is PdfAction)
                    {
                        pdfDictionary.Put(PdfName.A, (PdfAction)value);
                    }
                    else if (value is PdfAnnotation)
                    {
                        pdfDictionary.Put(PdfName.AA, (PdfAnnotation)value);
                    }
                    else if (value is PdfDictionary && ((PdfDictionary)value).Size == 1 && ((PdfDictionary)value).Contains(PdfName.N))
                    {
                        pdfDictionary.Put(PdfName.AP, (PdfDictionary)value);
                    }
                    else
                    {
                        pdfDictionary.Put(PdfName.V, (PdfObject)value);
                    }

                    pdfArray.Add(pdfDictionary);
                }

                return pdfArray;
            }
        }

        private static readonly byte[] HEADER_FDF = DocWriter.GetISOBytes("%FDF-1.4\n%âãÏÓ\n");

        private Dictionary<string, object> fields = new Dictionary<string, object>();

        private Wrt wrt;

        private string file;

        private string statusMessage;

        protected ICounter COUNTER = CounterFactory.GetCounter(typeof(FdfWriter));

        public virtual string StatusMessage
        {
            get
            {
                return statusMessage;
            }
            set
            {
                statusMessage = value;
            }
        }

        public virtual string File
        {
            get
            {
                return file;
            }
            set
            {
                file = value;
            }
        }

        public FdfWriter()
        {
        }

        public FdfWriter(Stream os)
        {
            wrt = new Wrt(os, this);
        }

        public virtual void WriteTo(Stream os)
        {
            if (wrt == null)
            {
                wrt = new Wrt(os, this);
            }

            wrt.Write();
        }

        public virtual void Write()
        {
            wrt.Write();
        }

        internal bool SetField(string field, PdfObject value)
        {
            Dictionary<string, object> dictionary = fields;
            StringTokenizer stringTokenizer = new StringTokenizer(field, ".");
            if (!stringTokenizer.HasMoreTokens())
            {
                return false;
            }

            string key;
            object value2;
            while (true)
            {
                key = stringTokenizer.NextToken();
                dictionary.TryGetValue(key, out value2);
                if (!stringTokenizer.HasMoreTokens())
                {
                    break;
                }

                if (value2 == null)
                {
                    value2 = (dictionary[key] = new Dictionary<string, object>());
                    dictionary = (Dictionary<string, object>)value2;
                    continue;
                }

                if (value2 is Dictionary<string, object>)
                {
                    dictionary = (Dictionary<string, object>)value2;
                    continue;
                }

                return false;
            }

            if (!(value2 is Dictionary<string, object>))
            {
                dictionary[key] = value;
                return true;
            }

            return false;
        }

        internal void IterateFields(Dictionary<string, object> values, Dictionary<string, object> map, string name)
        {
            foreach (KeyValuePair<string, object> item in map)
            {
                string key = item.Key;
                object value = item.Value;
                if (value is Dictionary<string, object>)
                {
                    IterateFields(values, (Dictionary<string, object>)value, name + "." + key);
                }
                else
                {
                    values[(name + "." + key).Substring(1)] = value;
                }
            }
        }

        public virtual bool RemoveField(string field)
        {
            Dictionary<string, object> dictionary = fields;
            StringTokenizer stringTokenizer = new StringTokenizer(field, ".");
            if (!stringTokenizer.HasMoreTokens())
            {
                return false;
            }

            List<object> list = new List<object>();
            object value;
            while (true)
            {
                string text = stringTokenizer.NextToken();
                dictionary.TryGetValue(text, out value);
                if (value == null)
                {
                    return false;
                }

                list.Add(dictionary);
                list.Add(text);
                if (!stringTokenizer.HasMoreTokens())
                {
                    break;
                }

                if (value is Dictionary<string, object>)
                {
                    dictionary = (Dictionary<string, object>)value;
                    continue;
                }

                return false;
            }

            if (value is Dictionary<string, object>)
            {
                return false;
            }

            for (int num = list.Count - 2; num >= 0; num -= 2)
            {
                dictionary = (Dictionary<string, object>)list[num];
                string key = (string)list[num + 1];
                dictionary.Remove(key);
                if (dictionary.Count > 0)
                {
                    break;
                }
            }

            return true;
        }

        public virtual Dictionary<string, object> GetFields()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            IterateFields(dictionary, fields, "");
            return dictionary;
        }

        public virtual string GetField(string field)
        {
            Dictionary<string, object> dictionary = fields;
            StringTokenizer stringTokenizer = new StringTokenizer(field, ".");
            if (!stringTokenizer.HasMoreTokens())
            {
                return null;
            }

            object value;
            while (true)
            {
                string key = stringTokenizer.NextToken();
                dictionary.TryGetValue(key, out value);
                if (value == null)
                {
                    return null;
                }

                if (!stringTokenizer.HasMoreTokens())
                {
                    break;
                }

                if (value is Dictionary<string, object>)
                {
                    dictionary = (Dictionary<string, object>)value;
                    continue;
                }

                return null;
            }

            if (value is Dictionary<string, object>)
            {
                return null;
            }

            if (((PdfObject)value).IsString())
            {
                return ((PdfString)value).ToUnicodeString();
            }

            return PdfName.DecodeName(value.ToString());
        }

        public virtual bool SetFieldAsName(string field, string value)
        {
            return SetField(field, new PdfName(value));
        }

        public virtual bool SetFieldAsString(string field, string value)
        {
            return SetField(field, new PdfString(value, "UnicodeBig"));
        }

        public virtual bool SetFieldAsAction(string field, PdfAction action)
        {
            return SetField(field, action);
        }

        public virtual bool SetFieldAsTemplate(string field, PdfTemplate template)
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            if (template is PdfImportedPage)
            {
                pdfDictionary.Put(PdfName.N, template.IndirectReference);
            }
            else
            {
                PdfStream formXObject = template.GetFormXObject(0);
                PdfIndirectReference indirectReference = wrt.AddToBody(formXObject).IndirectReference;
                pdfDictionary.Put(PdfName.N, indirectReference);
            }

            return SetField(field, pdfDictionary);
        }

        public virtual bool SetFieldAsImage(string field, Image image)
        {
            if (float.IsNaN(image.AbsoluteX))
            {
                image.SetAbsolutePosition(0f, image.AbsoluteY);
            }

            if (float.IsNaN(image.AbsoluteY))
            {
                image.SetAbsolutePosition(image.AbsoluteY, 0f);
            }

            PdfTemplate pdfTemplate = PdfTemplate.CreateTemplate(wrt, image.Width, image.Height);
            pdfTemplate.AddImage(image);
            PdfStream formXObject = pdfTemplate.GetFormXObject(0);
            PdfIndirectReference indirectReference = wrt.AddToBody(formXObject).IndirectReference;
            PdfDictionary pdfDictionary = new PdfDictionary();
            pdfDictionary.Put(PdfName.N, indirectReference);
            return SetField(field, pdfDictionary);
        }

        public virtual bool SetFieldAsJavascript(string field, PdfName jsTrigName, string js)
        {
            PdfAnnotation pdfAnnotation = wrt.CreateAnnotation(null, null);
            PdfAction value = PdfAction.JavaScript(js, wrt);
            pdfAnnotation.Put(jsTrigName, value);
            return SetField(field, pdfAnnotation);
        }

        public virtual PdfImportedPage GetImportedPage(PdfReader reader, int pageNumber)
        {
            return wrt.GetImportedPage(reader, pageNumber);
        }

        public virtual PdfTemplate CreateTemplate(float width, float height)
        {
            return PdfTemplate.CreateTemplate(wrt, width, height);
        }

        public virtual void SetFields(FdfReader fdf)
        {
            foreach (KeyValuePair<string, PdfDictionary> field in fdf.Fields)
            {
                string key = field.Key;
                PdfDictionary value = field.Value;
                PdfObject pdfObject = value.Get(PdfName.V);
                if (pdfObject != null)
                {
                    SetField(key, pdfObject);
                }

                pdfObject = value.Get(PdfName.A);
                if (pdfObject != null)
                {
                    SetField(key, pdfObject);
                }
            }
        }

        public virtual void SetFields(PdfReader pdf)
        {
            SetFields(pdf.AcroFields);
        }

        public virtual void SetFields(AcroFields af)
        {
            foreach (KeyValuePair<string, AcroFields.Item> field in af.Fields)
            {
                string key = field.Key;
                PdfDictionary merged = field.Value.GetMerged(0);
                PdfObject pdfObjectRelease = PdfReader.GetPdfObjectRelease(merged.Get(PdfName.V));
                if (pdfObjectRelease != null)
                {
                    PdfObject pdfObjectRelease2 = PdfReader.GetPdfObjectRelease(merged.Get(PdfName.FT));
                    if (pdfObjectRelease2 != null && !PdfName.SIG.Equals(pdfObjectRelease2))
                    {
                        SetField(key, pdfObjectRelease);
                    }
                }
            }
        }

        protected virtual ICounter GetCounter()
        {
            return COUNTER;
        }
    }
}
