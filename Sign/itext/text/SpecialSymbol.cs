namespace Sign.itext.text
{
    public class SpecialSymbol
    {
        public static int Index(string str)
        {
            int length = str.Length;
            for (int i = 0; i < length; i++)
            {
                if (GetCorrespondingSymbol(str[i]) != ' ')
                {
                    return i;
                }
            }

            return -1;
        }

        public static Chunk Get(char c, Font font)
        {
            char correspondingSymbol = GetCorrespondingSymbol(c);
            if (correspondingSymbol == ' ')
            {
                return new Chunk(c.ToString(), font);
            }

            Font font2 = new Font(Font.FontFamily.SYMBOL, font.Size, font.Style, font.Color);
            return new Chunk(correspondingSymbol.ToString(), font2);
        }

        public static char GetCorrespondingSymbol(char c)
        {
            return c switch
            {
                'Α' => 'A',
                'Β' => 'B',
                'Γ' => 'G',
                'Δ' => 'D',
                'Ε' => 'E',
                'Ζ' => 'Z',
                'Η' => 'H',
                'Θ' => 'Q',
                'Ι' => 'I',
                'Κ' => 'K',
                'Λ' => 'L',
                'Μ' => 'M',
                'Ν' => 'N',
                'Ξ' => 'X',
                'Ο' => 'O',
                'Π' => 'P',
                'Ρ' => 'R',
                'Σ' => 'S',
                'Τ' => 'T',
                'Υ' => 'U',
                'Φ' => 'F',
                'Χ' => 'C',
                'Ψ' => 'Y',
                'Ω' => 'W',
                'α' => 'a',
                'β' => 'b',
                'γ' => 'g',
                'δ' => 'd',
                'ε' => 'e',
                'ζ' => 'z',
                'η' => 'h',
                'θ' => 'q',
                'ι' => 'i',
                'κ' => 'k',
                'λ' => 'l',
                'μ' => 'm',
                'ν' => 'n',
                'ξ' => 'x',
                'ο' => 'o',
                'π' => 'p',
                'ρ' => 'r',
                'ς' => 'V',
                'σ' => 's',
                'τ' => 't',
                'υ' => 'u',
                'φ' => 'f',
                'χ' => 'c',
                'ψ' => 'y',
                'ω' => 'w',
                _ => ' ',
            };
        }
    }
}
