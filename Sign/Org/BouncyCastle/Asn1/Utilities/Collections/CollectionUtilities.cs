using System.Collections;
using System.Text;

namespace Sign.Org.BouncyCastle.Asn1.Utilities.Collections
{
    public sealed class CollectionUtilities
    {
        private CollectionUtilities()
        {
        }

        public static void AddRange(IList to, ICollection range)
        {
            foreach (object item in range)
            {
                to.Add(item);
            }
        }

        public static bool CheckElementsAreOfType(IEnumerable e, Type t)
        {
            foreach (object item in e)
            {
                if (!t.IsInstanceOfType(item))
                {
                    return false;
                }
            }

            return true;
        }

        public static IDictionary ReadOnly(IDictionary d)
        {
            return new UnmodifiableDictionaryProxy(d);
        }

        public static IList ReadOnly(IList l)
        {
            return new UnmodifiableListProxy(l);
        }

        public static ISet ReadOnly(ISet s)
        {
            return new UnmodifiableSetProxy(s);
        }

        public static string ToString(IEnumerable c)
        {
            StringBuilder stringBuilder = new StringBuilder("[");
            IEnumerator enumerator = c.GetEnumerator();
            if (enumerator.MoveNext())
            {
                stringBuilder.Append(enumerator.Current.ToString());
                while (enumerator.MoveNext())
                {
                    stringBuilder.Append(", ");
                    stringBuilder.Append(enumerator.Current.ToString());
                }
            }

            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }
    }
}
