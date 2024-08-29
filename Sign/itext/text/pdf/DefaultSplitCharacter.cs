namespace Sign.itext.text.pdf
{
    public class DefaultSplitCharacter : ISplitCharacter
    {
        public static readonly ISplitCharacter DEFAULT = new DefaultSplitCharacter();

        protected char[] characters;

        public DefaultSplitCharacter()
        {
        }

        public DefaultSplitCharacter(char character)
            : this(new char[1] { character })
        {
        }

        public DefaultSplitCharacter(char[] characters)
        {
            this.characters = characters;
        }

        public virtual bool IsSplitCharacter(int start, int current, int end, char[] cc, PdfChunk[] ck)
        {
            char currentCharacter = GetCurrentCharacter(current, cc, ck);
            if (characters != null)
            {
                for (int i = 0; i < characters.Length; i++)
                {
                    if (currentCharacter == characters[i])
                    {
                        return true;
                    }
                }

                return false;
            }

            if (currentCharacter <= ' ' || currentCharacter == '-' || currentCharacter == '‐')
            {
                return true;
            }

            if (currentCharacter < '\u2002')
            {
                return false;
            }

            if ((currentCharacter < '\u2002' || currentCharacter > '\u200b') && (currentCharacter < '⺀' || currentCharacter >= '힠') && (currentCharacter < '豈' || currentCharacter >= 'ﬀ') && (currentCharacter < '︰' || currentCharacter >= '﹐'))
            {
                if (currentCharacter >= '｡')
                {
                    return currentCharacter < 'ﾠ';
                }

                return false;
            }

            return true;
        }

        protected virtual char GetCurrentCharacter(int current, char[] cc, PdfChunk[] ck)
        {
            if (ck == null)
            {
                return cc[current];
            }

            return (char)ck[Math.Min(current, ck.Length - 1)].GetUnicodeEquivalent(cc[current]);
        }
    }
}
