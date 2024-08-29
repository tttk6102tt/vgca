namespace Sign.itext.text.pdf
{
    public class PdfTextArray
    {
        private List<object> arrayList = new List<object>();

        private string lastStr;

        private float lastNum = float.NaN;

        internal List<object> ArrayList => arrayList;

        public PdfTextArray(string str)
        {
            Add(str);
        }

        public PdfTextArray()
        {
        }

        public virtual void Add(PdfNumber number)
        {
            Add((float)number.DoubleValue);
        }

        public virtual void Add(float number)
        {
            if (number == 0f)
            {
                return;
            }

            if (!float.IsNaN(lastNum))
            {
                lastNum += number;
                if (lastNum != 0f)
                {
                    ReplaceLast(lastNum);
                }
                else
                {
                    arrayList.RemoveAt(arrayList.Count - 1);
                }
            }
            else
            {
                lastNum = number;
                arrayList.Add(lastNum);
            }

            lastStr = null;
        }

        public virtual void Add(string str)
        {
            if (str.Length > 0)
            {
                if (lastStr != null)
                {
                    lastStr += str;
                    ReplaceLast(lastStr);
                }
                else
                {
                    lastStr = str;
                    arrayList.Add(lastStr);
                }

                lastNum = float.NaN;
            }
        }

        private void ReplaceLast(object obj)
        {
            arrayList[arrayList.Count - 1] = obj;
        }
    }
}
