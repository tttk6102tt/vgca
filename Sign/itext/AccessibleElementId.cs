namespace Sign.itext
{
    public class AccessibleElementId : IComparable<AccessibleElementId>
    {
        private static int id_counter;

        private readonly int id;

        public AccessibleElementId()
        {
            id = ++id_counter;
        }

        public override string ToString()
        {
            int num = id;
            return num.ToString();
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override bool Equals(object o)
        {
            if (o is AccessibleElementId)
            {
                return id == ((AccessibleElementId)o).id;
            }

            return false;
        }

        public virtual int CompareTo(AccessibleElementId elementId)
        {
            if (id < elementId.id)
            {
                return -1;
            }

            if (id > elementId.id)
            {
                return 1;
            }

            return 0;
        }
    }
}
