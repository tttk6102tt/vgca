using System.Globalization;

namespace Sign.SystemItext.util
{
    public static class Util
    {
        public static int USR(int op1, int op2)
        {
            if (op2 < 1)
            {
                return op1;
            }

            return (int)((uint)op1 >> op2);
        }

        public static bool EqualsIgnoreCase(string s1, string s2)
        {
            return CultureInfo.InvariantCulture.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase) == 0;
        }

        public static int CompareToIgnoreCase(string s1, string s2)
        {
            return CultureInfo.InvariantCulture.CompareInfo.Compare(s1, s2, CompareOptions.IgnoreCase);
        }

        public static CultureInfo GetStandartEnUSLocale()
        {
            CultureInfo obj = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            obj.NumberFormat.CurrencySymbol = "$";
            obj.DateTimeFormat.ShortDatePattern = "M/d/yyyy";
            obj.DateTimeFormat.ShortTimePattern = "h:mm tt";
            obj.DateTimeFormat.LongDatePattern = "dddd, MMMM dd, yyyy";
            obj.DateTimeFormat.YearMonthPattern = "MMMM, yyyy";
            return obj;
        }

        public static int GetArrayHashCode<T>(T[] a)
        {
            if (a == null)
            {
                return 0;
            }

            int num = 1;
            for (int i = 0; i < a.Length; i++)
            {
                T val = a[i];
                num = 31 * num + (val?.GetHashCode() ?? 0);
            }

            return num;
        }

        public static int compare(float f1, float f2)
        {
            if (f1 < f2)
            {
                return -1;
            }

            if (f1 > f2)
            {
                return 1;
            }

            int num = BitConverter.ToInt32(BitConverter.GetBytes(f1), 0);
            int num2 = BitConverter.ToInt32(BitConverter.GetBytes(f2), 0);
            if (num != num2)
            {
                if (num >= num2)
                {
                    return 1;
                }

                return -1;
            }

            return 0;
        }

        public static bool ArraysAreEqual<T>(T[] a, T[] b)
        {
            if (a == b)
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            int num = a.Length;
            if (b.Length != num)
            {
                return false;
            }

            for (int i = 0; i < num; i++)
            {
                object obj = a[i];
                object obj2 = b[i];
                if (!(obj?.Equals(obj2) ?? (obj2 == null)))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool AreEqual<T>(Stack<T> s1, Stack<T> s2)
        {
            if (s1.Count != s2.Count)
            {
                return false;
            }

            IEnumerator<T> enumerator = s1.GetEnumerator();
            IEnumerator<T> enumerator2 = s2.GetEnumerator();
            while (enumerator.MoveNext() && enumerator2.MoveNext())
            {
                if ((enumerator.Current != null || enumerator2.Current != null) && (enumerator == null || !enumerator.Current.Equals(enumerator2.Current)))
                {
                    return false;
                }
            }

            return true;
        }

        public static T Min<T>(T[] array)
        {
            if (array.Length == 0)
            {
                throw new InvalidOperationException();
            }

            T val = array[0];
            for (int i = 1; i < array.Length; i++)
            {
                if (Comparer<T>.Default.Compare(val, array[i]) > 0)
                {
                    val = array[i];
                }
            }

            return val;
        }

        public static T Max<T>(T[] array)
        {
            if (array.Length == 0)
            {
                throw new InvalidOperationException();
            }

            T val = array[0];
            for (int i = 1; i < array.Length; i++)
            {
                if (Comparer<T>.Default.Compare(val, array[i]) < 0)
                {
                    val = array[i];
                }
            }

            return val;
        }
    }
}
