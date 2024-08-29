namespace Sign.itext.xml.xmp.impl
{
    internal class ParseState
    {
        private readonly string _str;

        private int _pos;

        public ParseState(string str)
        {
            _str = str;
        }

        public virtual int Length()
        {
            return _str.Length;
        }

        public virtual bool HasNext()
        {
            return _pos < _str.Length;
        }

        public virtual char Ch(int index)
        {
            if (index >= _str.Length)
            {
                return '\0';
            }

            return _str[index];
        }

        public virtual char Ch()
        {
            if (_pos >= _str.Length)
            {
                return '\0';
            }

            return _str[_pos];
        }

        public virtual void Skip()
        {
            _pos++;
        }

        public virtual int Pos()
        {
            return _pos;
        }

        public virtual int GatherInt(string errorMsg, int maxValue)
        {
            int num = 0;
            bool flag = false;
            char c = Ch(_pos);
            while ('0' <= c && c <= '9')
            {
                num = num * 10 + (c - 48);
                flag = true;
                _pos++;
                c = Ch(_pos);
            }

            if (flag)
            {
                if (num > maxValue)
                {
                    return maxValue;
                }

                if (num < 0)
                {
                    return 0;
                }

                return num;
            }

            throw new XmpException(errorMsg, 5);
        }
    }
}
