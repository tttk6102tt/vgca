using Sign.itext.error_messages;
using System.Text;

namespace Sign.itext.text
{
    public class Phrase : List<IElement>, ITextElementArray, IElement
    {
        protected float leading = float.NaN;

        protected float multipliedLeading;

        protected Font font;

        protected IHyphenationEvent hyphenation;

        protected TabSettings tabSettings;

        public virtual int Type => 11;

        public virtual IList<Chunk> Chunks
        {
            get
            {
                List<Chunk> list = new List<Chunk>();
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    IElement current = enumerator.Current;
                    list.AddRange(current.Chunks);
                }

                return list;
            }
        }

        public virtual float MultipliedLeading
        {
            get
            {
                return multipliedLeading;
            }
            set
            {
                leading = 0f;
                multipliedLeading = value;
            }
        }

        public virtual float Leading
        {
            get
            {
                if (float.IsNaN(leading) && font != null)
                {
                    return font.GetCalculatedLeading(1.5f);
                }

                return leading;
            }
            set
            {
                leading = value;
                multipliedLeading = 0f;
            }
        }

        public virtual float TotalLeading
        {
            get
            {
                float num = ((font == null) ? (12f * multipliedLeading) : font.GetCalculatedLeading(multipliedLeading));
                if (num > 0f && !HasLeading())
                {
                    return num;
                }

                return Leading + num;
            }
        }

        public virtual Font Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
            }
        }

        public virtual string Content
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Chunk chunk in Chunks)
                {
                    stringBuilder.Append(chunk.ToString());
                }

                return stringBuilder.ToString();
            }
        }

        public virtual IHyphenationEvent Hyphenation
        {
            get
            {
                return hyphenation;
            }
            set
            {
                hyphenation = value;
            }
        }

        public virtual TabSettings TabSettings
        {
            get
            {
                return tabSettings;
            }
            set
            {
                tabSettings = value;
            }
        }

        public Phrase()
            : this(16f)
        {
        }

        public Phrase(Phrase phrase)
        {
            AddAll(phrase);
            SetLeading(phrase.Leading, phrase.MultipliedLeading);
            font = phrase.Font;
            tabSettings = phrase.TabSettings;
            hyphenation = phrase.hyphenation;
        }

        public Phrase(float leading)
        {
            this.leading = leading;
            font = new Font();
        }

        public Phrase(Chunk chunk)
        {
            base.Add((IElement)chunk);
            font = chunk.Font;
            hyphenation = chunk.GetHyphenation();
        }

        public Phrase(float leading, Chunk chunk)
        {
            this.leading = leading;
            base.Add((IElement)chunk);
            font = chunk.Font;
            hyphenation = chunk.GetHyphenation();
        }

        public Phrase(string str)
            : this(float.NaN, str, new Font())
        {
        }

        public Phrase(string str, Font font)
            : this(float.NaN, str, font)
        {
        }

        public Phrase(float leading, string str)
            : this(leading, str, new Font())
        {
        }

        public Phrase(float leading, string str, Font font)
        {
            this.leading = leading;
            this.font = font;
            if (!string.IsNullOrEmpty(str))
            {
                base.Add((IElement)new Chunk(str, font));
            }
        }

        public virtual bool Process(IElementListener listener)
        {
            try
            {
                using (Enumerator enumerator = GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        IElement current = enumerator.Current;
                        listener.Add(current);
                    }
                }

                return true;
            }
            catch (DocumentException)
            {
                return false;
            }
        }

        public virtual bool IsContent()
        {
            return true;
        }

        public virtual bool IsNestable()
        {
            return true;
        }

        public virtual void Add(int index, IElement element)
        {
            if (element == null)
            {
                return;
            }

            switch (element.Type)
            {
                case 10:
                    {
                        Chunk chunk = (Chunk)element;
                        if (!font.IsStandardFont())
                        {
                            chunk.Font = font.Difference(chunk.Font);
                        }

                        if (hyphenation != null && chunk.GetHyphenation() == null && !chunk.IsEmpty())
                        {
                            chunk.SetHyphenation(hyphenation);
                        }

                        Insert(index, chunk);
                        break;
                    }
                case 11:
                case 12:
                case 14:
                case 17:
                case 23:
                case 29:
                case 37:
                case 50:
                case 55:
                case 666:
                    Insert(index, element);
                    break;
                default:
                    throw new Exception(MessageLocalization.GetComposedMessage("insertion.of.illegal.element.1", element.Type.ToString()));
            }
        }

        public virtual bool Add(string s)
        {
            if (s == null)
            {
                return false;
            }

            base.Add((IElement)new Chunk(s, font));
            return true;
        }

        public new virtual bool Add(IElement element)
        {
            if (element == null)
            {
                return false;
            }

            try
            {
                switch (element.Type)
                {
                    case 10:
                        return AddChunk((Chunk)element);
                    case 11:
                    case 12:
                        {
                            Phrase obj = (Phrase)element;
                            bool flag = true;
                            foreach (IElement item in obj)
                            {
                                flag = ((!(item is Chunk)) ? (flag & Add(item)) : (flag & AddChunk((Chunk)item)));
                            }

                            return flag;
                        }
                    case 14:
                    case 17:
                    case 23:
                    case 29:
                    case 37:
                    case 50:
                    case 55:
                    case 666:
                        base.Add(element);
                        return true;
                    default:
                        throw new Exception(element.Type.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(MessageLocalization.GetComposedMessage("insertion.of.illegal.element.1", ex.Message));
            }
        }

        public virtual bool AddAll<T>(ICollection<T> collection) where T : IElement
        {
            if (collection.Count == 0)
            {
                return false;
            }

            foreach (T item in collection)
            {
                IElement element = item;
                Add(element);
            }

            return true;
        }

        protected virtual bool AddChunk(Chunk chunk)
        {
            Font font = chunk.Font;
            string content = chunk.Content;
            if (this.font != null && !this.font.IsStandardFont())
            {
                font = this.font.Difference(chunk.Font);
            }

            if (base.Count > 0 && !chunk.HasAttributes())
            {
                try
                {
                    Chunk chunk2 = (Chunk)base[base.Count - 1];
                    if (!chunk2.HasAttributes() && (font == null || font.CompareTo(chunk2.Font) == 0) && chunk2.Font.CompareTo(font) == 0 && !"".Equals(chunk2.Content.Trim()) && !"".Equals(content.Trim()))
                    {
                        chunk2.Append(content);
                        return true;
                    }
                }
                catch
                {
                }
            }

            Chunk chunk3 = new Chunk(content, font);
            chunk3.Attributes = chunk.Attributes;
            chunk3.role = chunk.Role;
            chunk3.accessibleAttributes = chunk.GetAccessibleAttributes();
            if (hyphenation != null && chunk3.GetHyphenation() == null && !chunk3.IsEmpty())
            {
                chunk3.SetHyphenation(hyphenation);
            }

            base.Add((IElement)chunk3);
            return true;
        }

        public virtual void AddSpecial(IElement obj)
        {
            base.Add(obj);
        }

        public virtual bool IsEmpty()
        {
            switch (base.Count)
            {
                case 0:
                    return true;
                case 1:
                    {
                        IElement element = base[0];
                        if (element.Type == 10 && ((Chunk)element).IsEmpty())
                        {
                            return true;
                        }

                        return false;
                    }
                default:
                    return false;
            }
        }

        public virtual bool HasLeading()
        {
            if (float.IsNaN(leading))
            {
                return false;
            }

            return true;
        }

        public virtual void SetLeading(float fixedLeading, float multipliedLeading)
        {
            leading = fixedLeading;
            this.multipliedLeading = multipliedLeading;
        }

        private Phrase(bool dummy)
        {
        }

        public static Phrase GetInstance(string str)
        {
            return GetInstance(16, str, new Font());
        }

        public static Phrase GetInstance(int leading, string str)
        {
            return GetInstance(leading, str, new Font());
        }

        public static Phrase GetInstance(int leading, string str, Font font)
        {
            Phrase phrase = new Phrase(dummy: true);
            phrase.Leading = leading;
            phrase.font = font;
            if (font.Family != Font.FontFamily.SYMBOL && font.Family != Font.FontFamily.ZAPFDINGBATS && font.BaseFont == null)
            {
                int num;
                while ((num = SpecialSymbol.Index(str)) > -1)
                {
                    if (num > 0)
                    {
                        string content = str.Substring(0, num);
                        phrase.Add(new Chunk(content, font));
                        str = str.Substring(num);
                    }

                    Font font2 = new Font(Font.FontFamily.SYMBOL, font.Size, font.Style, font.Color);
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(SpecialSymbol.GetCorrespondingSymbol(str[0]));
                    str = str.Substring(1);
                    while (SpecialSymbol.Index(str) == 0)
                    {
                        stringBuilder.Append(SpecialSymbol.GetCorrespondingSymbol(str[0]));
                        str = str.Substring(1);
                    }

                    phrase.Add(new Chunk(stringBuilder.ToString(), font2));
                }
            }

            if (!string.IsNullOrEmpty(str))
            {
                phrase.Add(new Chunk(str, font));
            }

            return phrase;
        }

        public virtual bool Trim()
        {
            while (base.Count > 0)
            {
                IElement element = base[0];
                if (!(element is Chunk) || !((Chunk)element).IsWhitespace())
                {
                    break;
                }

                Remove(element);
            }

            while (base.Count > 0)
            {
                IElement element2 = base[base.Count - 1];
                if (!(element2 is Chunk) || !((Chunk)element2).IsWhitespace())
                {
                    break;
                }

                Remove(element2);
            }

            return base.Count > 0;
        }
    }
}
