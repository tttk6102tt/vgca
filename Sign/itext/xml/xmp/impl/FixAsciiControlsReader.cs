using System.Globalization;

namespace Sign.itext.xml.xmp.impl
{
    public class FixAsciiControlsReader : PushbackReader
    {
        private const int STATE_START = 0;

        private const int STATE_AMP = 1;

        private const int STATE_HASH = 2;

        private const int STATE_HEX = 3;

        private const int STATE_DIG1 = 4;

        private const int STATE_ERROR = 5;

        private const int BUFFER_SIZE = 8;

        private int _control;

        private int _digits;

        private int _state;

        public FixAsciiControlsReader(TextReader inp)
            : base(inp, 8)
        {
        }

        public override int Read(char[] cbuf, int off, int len)
        {
            int num = 0;
            int num2 = 0;
            int num3 = off;
            char[] array = new char[8];
            bool flag = true;
            while (flag && num2 < len)
            {
                flag = base.Read(array, num, 1) == 1;
                if (flag)
                {
                    char c = ProcessChar(array[num]);
                    if (_state == 0)
                    {
                        if (Utils.IsControlChar(c))
                        {
                            c = ' ';
                        }

                        cbuf[num3++] = c;
                        num = 0;
                        num2++;
                    }
                    else if (_state == 5)
                    {
                        Unread(array, 0, num + 1);
                        num = 0;
                    }
                    else
                    {
                        num++;
                    }
                }
                else if (num > 0)
                {
                    Unread(array, 0, num);
                    _state = 5;
                    num = 0;
                    flag = true;
                }
            }

            if (!(num2 > 0 || flag))
            {
                return -1;
            }

            return num2;
        }

        private char ProcessChar(char ch)
        {
            switch (_state)
            {
                case 0:
                    if (ch == '&')
                    {
                        _state = 1;
                    }

                    return ch;
                case 1:
                    _state = ((ch == '#') ? 2 : 5);
                    return ch;
                case 2:
                    if (ch == 'x')
                    {
                        _control = 0;
                        _digits = 0;
                        _state = 3;
                    }
                    else if ('0' <= ch && ch <= '9')
                    {
                        _control = Convert.ToInt32(ch.ToString(CultureInfo.InvariantCulture), 10);
                        _digits = 1;
                        _state = 4;
                    }
                    else
                    {
                        _state = 5;
                    }

                    return ch;
                case 4:
                    if ('0' <= ch && ch <= '9')
                    {
                        _control = _control * 10 + Convert.ToInt32(ch.ToString(CultureInfo.InvariantCulture), 10);
                        _digits++;
                        _state = ((_digits <= 5) ? 4 : 5);
                    }
                    else
                    {
                        if (ch == ';' && Utils.IsControlChar((char)_control))
                        {
                            _state = 0;
                            return (char)_control;
                        }

                        _state = 5;
                    }

                    return ch;
                case 3:
                    if (('0' <= ch && ch <= '9') || ('a' <= ch && ch <= 'f') || ('A' <= ch && ch <= 'F'))
                    {
                        _control = _control * 16 + Convert.ToInt32(ch.ToString(CultureInfo.InvariantCulture), 16);
                        _digits++;
                        _state = ((_digits <= 4) ? 3 : 5);
                    }
                    else
                    {
                        if (ch == ';' && Utils.IsControlChar((char)_control))
                        {
                            _state = 0;
                            return (char)_control;
                        }

                        _state = 5;
                    }

                    return ch;
                case 5:
                    _state = 0;
                    return ch;
                default:
                    return ch;
            }
        }
    }
}
