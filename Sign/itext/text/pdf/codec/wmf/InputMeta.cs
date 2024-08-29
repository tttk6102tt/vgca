namespace Sign.itext.text.pdf.codec.wmf
{
    public class InputMeta
    {
        private Stream sr;

        private int length;

        public virtual int Length => length;

        public InputMeta(Stream istr)
        {
            sr = istr;
        }

        public virtual int ReadWord()
        {
            length += 2;
            int num = sr.ReadByte();
            if (num < 0)
            {
                return 0;
            }

            return (num + (sr.ReadByte() << 8)) & 0xFFFF;
        }

        public virtual int ReadShort()
        {
            int num = ReadWord();
            if (num > 32767)
            {
                num -= 65536;
            }

            return num;
        }

        public virtual int ReadInt()
        {
            length += 4;
            int num = sr.ReadByte();
            if (num < 0)
            {
                return 0;
            }

            int num2 = sr.ReadByte() << 8;
            int num3 = sr.ReadByte() << 16;
            return num + num2 + num3 + (sr.ReadByte() << 24);
        }

        public virtual int ReadByte()
        {
            length++;
            return sr.ReadByte() & 0xFF;
        }

        public virtual void Skip(int len)
        {
            length += len;
            Utilities.Skip(sr, len);
        }

        public virtual BaseColor ReadColor()
        {
            int red = ReadByte();
            int green = ReadByte();
            int blue = ReadByte();
            ReadByte();
            return new BaseColor(red, green, blue);
        }
    }
}
