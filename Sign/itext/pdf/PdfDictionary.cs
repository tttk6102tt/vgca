using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class PdfDictionary : PdfObject
    {
        public static PdfName FONT = PdfName.FONT;

        public static PdfName OUTLINES = PdfName.OUTLINES;

        public static PdfName PAGE = PdfName.PAGE;

        public static PdfName PAGES = PdfName.PAGES;

        public static PdfName CATALOG = PdfName.CATALOG;

        private PdfName dictionaryType;

        protected internal Dictionary<PdfName, PdfObject> hashMap;

        public virtual Dictionary<PdfName, PdfObject>.KeyCollection Keys => hashMap.Keys;

        public virtual int Size => hashMap.Count;

        public PdfDictionary()
            : base(6)
        {
            hashMap = new Dictionary<PdfName, PdfObject>();
        }

        public PdfDictionary(PdfName type)
            : this()
        {
            dictionaryType = type;
            Put(PdfName.TYPE, dictionaryType);
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            PdfWriter.CheckPdfIsoConformance(writer, 11, this);
            os.WriteByte(60);
            os.WriteByte(60);
            foreach (KeyValuePair<PdfName, PdfObject> item in hashMap)
            {
                PdfObject value = item.Value;
                item.Key.ToPdf(writer, os);
                int num = value.Type;
                if (num != 5 && num != 6 && num != 4 && num != 3)
                {
                    os.WriteByte(32);
                }

                value.ToPdf(writer, os);
            }

            os.WriteByte(62);
            os.WriteByte(62);
        }

        public virtual void Put(PdfName key, PdfObject value)
        {
            if (value == null || value.IsNull())
            {
                hashMap.Remove(key);
            }
            else
            {
                hashMap[key] = value;
            }
        }

        public virtual void PutEx(PdfName key, PdfObject value)
        {
            if (value != null)
            {
                Put(key, value);
            }
        }

        public virtual void PutAll(PdfDictionary dic)
        {
            foreach (KeyValuePair<PdfName, PdfObject> item in dic.hashMap)
            {
                if (hashMap.ContainsKey(item.Key))
                {
                    hashMap[item.Key] = item.Value;
                }
                else
                {
                    hashMap.Add(item.Key, item.Value);
                }
            }
        }

        public virtual void Remove(PdfName key)
        {
            hashMap.Remove(key);
        }

        public virtual void Clear()
        {
            hashMap.Clear();
        }

        public virtual PdfObject Get(PdfName key)
        {
            if (hashMap.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }

        public virtual bool IsFont()
        {
            return CheckType(FONT);
        }

        public virtual bool IsPage()
        {
            return CheckType(PAGE);
        }

        public virtual bool IsPages()
        {
            return CheckType(PAGES);
        }

        public virtual bool IsCatalog()
        {
            return CheckType(CATALOG);
        }

        public virtual bool IsOutlineTree()
        {
            return CheckType(OUTLINES);
        }

        public virtual bool CheckType(PdfName type)
        {
            if (type == null)
            {
                return false;
            }

            if (dictionaryType == null)
            {
                dictionaryType = GetAsName(PdfName.TYPE);
            }

            return type.Equals(dictionaryType);
        }

        public virtual void Merge(PdfDictionary other)
        {
            foreach (PdfName key in other.hashMap.Keys)
            {
                hashMap[key] = other.hashMap[key];
            }
        }

        public virtual void MergeDifferent(PdfDictionary other)
        {
            foreach (PdfName key in other.hashMap.Keys)
            {
                if (!hashMap.ContainsKey(key))
                {
                    hashMap[key] = other.hashMap[key];
                }
            }
        }

        public virtual bool Contains(PdfName key)
        {
            return hashMap.ContainsKey(key);
        }

        public virtual Dictionary<PdfName, PdfObject>.Enumerator GetEnumerator()
        {
            return hashMap.GetEnumerator();
        }

        public override string ToString()
        {
            if (Get(PdfName.TYPE) == null)
            {
                return "Dictionary";
            }

            return "Dictionary of type: " + Get(PdfName.TYPE);
        }

        public virtual PdfObject GetDirectObject(PdfName key)
        {
            return PdfReader.GetPdfObject(Get(key));
        }

        public virtual PdfDictionary GetAsDict(PdfName key)
        {
            PdfDictionary result = null;
            PdfObject directObject = GetDirectObject(key);
            if (directObject != null && directObject.IsDictionary())
            {
                result = (PdfDictionary)directObject;
            }

            return result;
        }

        public virtual PdfArray GetAsArray(PdfName key)
        {
            PdfArray result = null;
            PdfObject directObject = GetDirectObject(key);
            if (directObject != null && directObject.IsArray())
            {
                result = (PdfArray)directObject;
            }

            return result;
        }

        public virtual PdfStream GetAsStream(PdfName key)
        {
            PdfStream result = null;
            PdfObject directObject = GetDirectObject(key);
            if (directObject != null && directObject.IsStream())
            {
                result = (PdfStream)directObject;
            }

            return result;
        }

        public virtual PdfString GetAsString(PdfName key)
        {
            PdfString result = null;
            PdfObject directObject = GetDirectObject(key);
            if (directObject != null && directObject.IsString())
            {
                result = (PdfString)directObject;
            }

            return result;
        }

        public virtual PdfNumber GetAsNumber(PdfName key)
        {
            PdfNumber result = null;
            PdfObject directObject = GetDirectObject(key);
            if (directObject != null && directObject.IsNumber())
            {
                result = (PdfNumber)directObject;
            }

            return result;
        }

        public virtual PdfName GetAsName(PdfName key)
        {
            PdfName result = null;
            PdfObject directObject = GetDirectObject(key);
            if (directObject != null && directObject.IsName())
            {
                result = (PdfName)directObject;
            }

            return result;
        }

        public virtual PdfBoolean GetAsBoolean(PdfName key)
        {
            PdfBoolean result = null;
            PdfObject directObject = GetDirectObject(key);
            if (directObject != null && directObject.IsBoolean())
            {
                result = (PdfBoolean)directObject;
            }

            return result;
        }

        public virtual PdfIndirectReference GetAsIndirectObject(PdfName key)
        {
            PdfIndirectReference result = null;
            PdfObject pdfObject = Get(key);
            if (pdfObject != null && pdfObject.IsIndirect())
            {
                result = (PdfIndirectReference)pdfObject;
            }

            return result;
        }
    }
}
