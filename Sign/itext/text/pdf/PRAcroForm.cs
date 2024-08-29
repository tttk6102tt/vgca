using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PRAcroForm : PdfDictionary
    {
        public class FieldInformation
        {
            internal string fieldName;

            internal PdfDictionary info;

            internal PRIndirectReference refi;

            public virtual string WidgetName => info.Get(PdfName.NM)?.ToString();

            public virtual string Name => fieldName;

            public virtual PdfDictionary Info => info;

            public virtual PRIndirectReference Ref => refi;

            internal FieldInformation(string fieldName, PdfDictionary info, PRIndirectReference refi)
            {
                this.fieldName = fieldName;
                this.info = info;
                this.refi = refi;
            }
        }

        internal List<FieldInformation> fields;

        internal List<PdfDictionary> stack;

        internal Dictionary<string, FieldInformation> fieldByName;

        internal PdfReader reader;

        public new int Size => fields.Count;

        public virtual List<FieldInformation> Fields => fields;

        public PRAcroForm(PdfReader reader)
        {
            this.reader = reader;
            fields = new List<FieldInformation>();
            fieldByName = new Dictionary<string, FieldInformation>();
            stack = new List<PdfDictionary>();
        }

        public virtual FieldInformation GetField(string name)
        {
            fieldByName.TryGetValue(name, out var value);
            return value;
        }

        public virtual PRIndirectReference GetRefByName(string name)
        {
            return GetField(name)?.Ref;
        }

        public virtual void ReadAcroForm(PdfDictionary root)
        {
            if (root != null)
            {
                hashMap = root.hashMap;
                PushAttrib(root);
                PdfArray pdfArray = (PdfArray)PdfReader.GetPdfObjectRelease(root.Get(PdfName.FIELDS));
                if (pdfArray != null)
                {
                    IterateFields(pdfArray, null, null);
                }
            }
        }

        protected virtual void IterateFields(PdfArray fieldlist, PRIndirectReference fieldDict, string parentPath)
        {
            foreach (PRIndirectReference array in fieldlist.ArrayList)
            {
                PdfDictionary pdfDictionary = (PdfDictionary)PdfReader.GetPdfObjectRelease(array);
                PRIndirectReference pRIndirectReference2 = fieldDict;
                string text = parentPath;
                PdfString pdfString = (PdfString)pdfDictionary.Get(PdfName.T);
                bool flag = pdfString != null;
                if (flag)
                {
                    pRIndirectReference2 = array;
                    text = ((parentPath != null) ? (parentPath + "." + pdfString.ToString()) : pdfString.ToString());
                }

                PdfArray pdfArray = (PdfArray)pdfDictionary.Get(PdfName.KIDS);
                if (pdfArray != null)
                {
                    PushAttrib(pdfDictionary);
                    IterateFields(pdfArray, pRIndirectReference2, text);
                    stack.RemoveAt(stack.Count - 1);
                }
                else if (pRIndirectReference2 != null)
                {
                    PdfDictionary pdfDictionary2 = stack[stack.Count - 1];
                    if (flag)
                    {
                        pdfDictionary2 = MergeAttrib(pdfDictionary2, pdfDictionary);
                    }

                    pdfDictionary2.Put(PdfName.T, new PdfString(text));
                    FieldInformation fieldInformation = new FieldInformation(text, pdfDictionary2, pRIndirectReference2);
                    fields.Add(fieldInformation);
                    fieldByName[text] = fieldInformation;
                }
            }
        }

        protected virtual PdfDictionary MergeAttrib(PdfDictionary parent, PdfDictionary child)
        {
            PdfDictionary pdfDictionary = new PdfDictionary();
            if (parent != null)
            {
                pdfDictionary.Merge(parent);
            }

            foreach (PdfName key in child.Keys)
            {
                if (key.Equals(PdfName.DR) || key.Equals(PdfName.DA) || key.Equals(PdfName.Q) || key.Equals(PdfName.FF) || key.Equals(PdfName.DV) || key.Equals(PdfName.V) || key.Equals(PdfName.FT) || key.Equals(PdfName.NM) || key.Equals(PdfName.F))
                {
                    pdfDictionary.Put(key, child.Get(key));
                }
            }

            return pdfDictionary;
        }

        protected virtual void PushAttrib(PdfDictionary dict)
        {
            PdfDictionary parent = null;
            if (stack.Count != 0)
            {
                parent = stack[stack.Count - 1];
            }

            parent = MergeAttrib(parent, dict);
            stack.Add(parent);
        }
    }
}
