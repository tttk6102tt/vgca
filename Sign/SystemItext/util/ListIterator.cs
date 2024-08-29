namespace Sign.SystemItext.util
{
    public class ListIterator<T>
    {
        private IList<T> col;

        private int cursor;

        private int lastRet = -1;

        public ListIterator(IList<T> col)
        {
            this.col = col;
        }

        public virtual bool HasNext()
        {
            return cursor != col.Count;
        }

        public virtual T Next()
        {
            T result = col[cursor];
            lastRet = cursor++;
            return result;
        }

        public virtual T Previous()
        {
            int index = cursor - 1;
            T result = col[index];
            lastRet = (cursor = index);
            return result;
        }

        public virtual void Remove()
        {
            if (lastRet == -1)
            {
                throw new InvalidOperationException();
            }

            col.RemoveAt(lastRet);
            if (lastRet < cursor)
            {
                cursor--;
            }

            lastRet = -1;
        }
    }
}
