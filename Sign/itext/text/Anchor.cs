namespace Sign.itext.text
{
    public class Anchor : Phrase
    {
        protected string name;

        protected string reference;

        public override IList<Chunk> Chunks
        {
            get
            {
                List<Chunk> list = new List<Chunk>();
                bool localDestination = reference != null && reference.StartsWith("#");
                bool notGotoOK = true;
                using Enumerator enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    IElement current = enumerator.Current;
                    if (current is Chunk)
                    {
                        Chunk chunk = (Chunk)current;
                        notGotoOK = ApplyAnchor(chunk, notGotoOK, localDestination);
                        list.Add(chunk);
                        continue;
                    }

                    foreach (Chunk chunk2 in current.Chunks)
                    {
                        notGotoOK = ApplyAnchor(chunk2, notGotoOK, localDestination);
                        list.Add(chunk2);
                    }
                }

                return list;
            }
        }

        public override int Type => 17;

        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public virtual string Reference
        {
            get
            {
                return reference;
            }
            set
            {
                reference = value;
            }
        }

        public virtual Uri Url
        {
            get
            {
                try
                {
                    return new Uri(reference);
                }
                catch
                {
                    return null;
                }
            }
        }

        public Anchor()
            : base(16f)
        {
        }

        public Anchor(float leading)
            : base(leading)
        {
        }

        public Anchor(Chunk chunk)
            : base(chunk)
        {
        }

        public Anchor(string str)
            : base(str)
        {
        }

        public Anchor(string str, Font font)
            : base(str, font)
        {
        }

        public Anchor(float leading, Chunk chunk)
            : base(leading, chunk)
        {
        }

        public Anchor(float leading, string str)
            : base(leading, str)
        {
        }

        public Anchor(float leading, string str, Font font)
            : base(leading, str, font)
        {
        }

        public Anchor(Phrase phrase)
            : base(phrase)
        {
            if (phrase is Anchor)
            {
                Anchor anchor = (Anchor)phrase;
                Name = anchor.name;
                Reference = anchor.reference;
            }
        }

        public override bool Process(IElementListener listener)
        {
            try
            {
                bool flag = reference != null && reference.StartsWith("#");
                bool flag2 = true;
                foreach (Chunk chunk in Chunks)
                {
                    if (name != null && flag2 && !chunk.IsEmpty())
                    {
                        chunk.SetLocalDestination(name);
                        flag2 = false;
                    }

                    if (flag)
                    {
                        chunk.SetLocalGoto(reference.Substring(1));
                    }
                    else if (reference != null)
                    {
                        chunk.SetAnchor(reference);
                    }

                    listener.Add(chunk);
                }

                return true;
            }
            catch (DocumentException)
            {
                return false;
            }
        }

        protected virtual bool ApplyAnchor(Chunk chunk, bool notGotoOK, bool localDestination)
        {
            if (name != null && notGotoOK && !chunk.IsEmpty())
            {
                chunk.SetLocalDestination(name);
                notGotoOK = false;
            }

            if (localDestination)
            {
                chunk.SetLocalGoto(reference.Substring(1));
            }
            else if (reference != null)
            {
                chunk.SetAnchor(reference);
            }

            return notGotoOK;
        }
    }
}
