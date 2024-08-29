using Sign.itext.pdf;
using System.Collections;
using System.Text;
namespace Sign.itext.text.pdf
{
    public class PdfArray : PdfObject, IEnumerable<PdfObject>, IEnumerable
    {
        protected List<PdfObject> arrayList;

        public PdfObject this[int idx]
        {
            get
            {
                return arrayList[idx];
            }
            set
            {
                arrayList[idx] = value;
            }
        }

        public virtual List<PdfObject> ArrayList => arrayList;

        public virtual int Size => arrayList.Count;

        public PdfArray()
            : base(5)
        {
            arrayList = new List<PdfObject>();
        }

        public PdfArray(PdfObject obj)
            : base(5)
        {
            arrayList = new List<PdfObject>();
            arrayList.Add(obj);
        }

        public PdfArray(float[] values)
            : base(5)
        {
            arrayList = new List<PdfObject>();
            Add(values);
        }

        public PdfArray(int[] values)
            : base(5)
        {
            arrayList = new List<PdfObject>();
            Add(values);
        }

        public PdfArray(IList<PdfObject> l)
            : this()
        {
            foreach (PdfObject item in l)
            {
                Add(item);
            }
        }

        public PdfArray(PdfArray array)
            : base(5)
        {
            arrayList = new List<PdfObject>(array.arrayList);
        }

        public override void ToPdf(PdfWriter writer, Stream os)
        {
            PdfWriter.CheckPdfIsoConformance(writer, 11, this);
            os.WriteByte(91);
            bool flag = true;
            int num = 0;
            foreach (PdfObject array in arrayList)
            {
                PdfObject obj = array ?? PdfNull.PDFNULL;
                num = obj.Type;
                if (!flag && num != 5 && num != 6 && num != 4 && num != 3)
                {
                    os.WriteByte(32);
                }

                flag = false;
                obj.ToPdf(writer, os);
            }

            os.WriteByte(93);
        }

        public virtual PdfObject GetPdfObject(int idx)
        {
            return arrayList[idx];
        }

        public virtual PdfObject Set(int idx, PdfObject obj)
        {
            return arrayList[idx] = obj;
        }

        public virtual PdfObject Remove(int idx)
        {
            PdfObject result = arrayList[idx];
            arrayList.RemoveAt(idx);
            return result;
        }

        public virtual bool IsEmpty()
        {
            return arrayList.Count == 0;
        }

        public virtual bool Add(PdfObject obj)
        {
            arrayList.Add(obj);
            return true;
        }

        public virtual bool Add(float[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                arrayList.Add(new PdfNumber(values[i]));
            }

            return true;
        }

        public virtual bool Add(int[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                arrayList.Add(new PdfNumber(values[i]));
            }

            return true;
        }

        public virtual void Add(int index, PdfObject element)
        {
            arrayList.Insert(index, element);
        }

        public virtual void AddFirst(PdfObject obj)
        {
            arrayList.Insert(0, obj);
        }

        public virtual bool Contains(PdfObject obj)
        {
            return arrayList.Contains(obj);
        }

        public virtual SystemItext.util.ListIterator<PdfObject> GetListIterator()
        {
            return new SystemItext.util.ListIterator<PdfObject>(arrayList);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('[');
            for (int i = 0; i < arrayList.Count; i++)
            {
                PdfObject pdfObject = arrayList[i];
                if (pdfObject != null)
                {
                    stringBuilder.Append(pdfObject.ToString());
                }

                if (i < arrayList.Count - 1)
                {
                    stringBuilder.Append(", ");
                }
            }

            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }

        public virtual PdfObject GetDirectObject(int idx)
        {
            return PdfReader.GetPdfObject(this[idx]);
        }

        public virtual PdfDictionary GetAsDict(int idx)
        {
            PdfDictionary result = null;
            PdfObject directObject = GetDirectObject(idx);
            if (directObject != null && directObject.IsDictionary())
            {
                result = (PdfDictionary)directObject;
            }

            return result;
        }

        public virtual PdfArray GetAsArray(int idx)
        {
            PdfArray result = null;
            PdfObject directObject = GetDirectObject(idx);
            if (directObject != null && directObject.IsArray())
            {
                result = (PdfArray)directObject;
            }

            return result;
        }

        public virtual PdfStream GetAsStream(int idx)
        {
            PdfStream result = null;
            PdfObject directObject = GetDirectObject(idx);
            if (directObject != null && directObject.IsStream())
            {
                result = (PdfStream)directObject;
            }

            return result;
        }

        public virtual PdfString GetAsString(int idx)
        {
            PdfString result = null;
            PdfObject directObject = GetDirectObject(idx);
            if (directObject != null && directObject.IsString())
            {
                result = (PdfString)directObject;
            }

            return result;
        }

        public virtual PdfNumber GetAsNumber(int idx)
        {
            PdfNumber result = null;
            PdfObject directObject = GetDirectObject(idx);
            if (directObject != null && directObject.IsNumber())
            {
                result = (PdfNumber)directObject;
            }

            return result;
        }

        public virtual PdfName GetAsName(int idx)
        {
            PdfName result = null;
            PdfObject directObject = GetDirectObject(idx);
            if (directObject != null && directObject.IsName())
            {
                result = (PdfName)directObject;
            }

            return result;
        }

        public virtual PdfBoolean GetAsBoolean(int idx)
        {
            PdfBoolean result = null;
            PdfObject directObject = GetDirectObject(idx);
            if (directObject != null && directObject.IsBoolean())
            {
                result = (PdfBoolean)directObject;
            }

            return result;
        }

        public virtual PdfIndirectReference GetAsIndirectObject(int idx)
        {
            PdfIndirectReference result = null;
            PdfObject pdfObject = this[idx];
            if (pdfObject is PdfIndirectReference)
            {
                result = (PdfIndirectReference)pdfObject;
            }

            return result;
        }

        public virtual IEnumerator<PdfObject> GetEnumerator()
        {
            return arrayList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return arrayList.GetEnumerator();
        }

        public virtual long[] AsLongArray()
        {
            long[] array = new long[Size];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = GetAsNumber(i).LongValue;
            }

            return array;
        }
    }
}
