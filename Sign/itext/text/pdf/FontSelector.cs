using Sign.itext.error_messages;
using System.Globalization;
using System.Text;

namespace Sign.itext.text.pdf
{
    public class FontSelector
    {
        protected List<Font> fonts = new List<Font>();

        protected Font currentFont;

        public virtual void AddFont(Font font)
        {
            if (font.BaseFont != null)
            {
                fonts.Add(font);
                return;
            }

            Font item = new Font(font.GetCalculatedBaseFont(specialEncoding: true), font.Size, font.CalculatedStyle, font.Color);
            fonts.Add(item);
        }

        public virtual Phrase Process(string text)
        {
            if (fonts.Count == 0)
            {
                throw new ArgumentOutOfRangeException(MessageLocalization.GetComposedMessage("no.font.is.defined"));
            }

            char[] array = text.ToCharArray();
            int num = array.Length;
            StringBuilder stringBuilder = new StringBuilder();
            Phrase phrase = new Phrase();
            currentFont = null;
            for (int i = 0; i < num; i++)
            {
                Chunk chunk = ProcessChar(array, i, stringBuilder);
                if (chunk != null)
                {
                    phrase.Add(chunk);
                }
            }

            if (stringBuilder.Length > 0)
            {
                Chunk element = new Chunk(stringBuilder.ToString(), currentFont ?? fonts[0]);
                phrase.Add(element);
            }

            return phrase;
        }

        protected virtual Chunk ProcessChar(char[] cc, int k, StringBuilder sb)
        {
            Chunk result = null;
            char c = cc[k];
            if (c == '\n' || c == '\r')
            {
                sb.Append(c);
            }
            else
            {
                Font font = null;
                if (Utilities.IsSurrogatePair(cc, k))
                {
                    int num = Utilities.ConvertToUtf32(cc, k);
                    for (int i = 0; i < fonts.Count; i++)
                    {
                        font = fonts[i];
                        if (!font.BaseFont.CharExists(num) && CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(num), 0) != UnicodeCategory.Format)
                        {
                            continue;
                        }

                        if (currentFont != font)
                        {
                            if (sb.Length > 0 && currentFont != null)
                            {
                                result = new Chunk(sb.ToString(), currentFont);
                                sb.Length = 0;
                            }

                            currentFont = font;
                        }

                        sb.Append(c);
                        sb.Append(cc[++k]);
                        break;
                    }
                }
                else
                {
                    for (int j = 0; j < fonts.Count; j++)
                    {
                        font = fonts[j];
                        if (!font.BaseFont.CharExists(c) && char.GetUnicodeCategory(c) != UnicodeCategory.Format)
                        {
                            continue;
                        }

                        if (currentFont != font)
                        {
                            if (sb.Length > 0 && currentFont != null)
                            {
                                result = new Chunk(sb.ToString(), currentFont);
                                sb.Length = 0;
                            }

                            currentFont = font;
                        }

                        sb.Append(c);
                        break;
                    }
                }
            }

            return result;
        }
    }
}
