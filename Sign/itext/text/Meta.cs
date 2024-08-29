using System.Text;

namespace Sign.itext.text
{
    public class Meta : IElement
    {
        private int type;

        private StringBuilder content;

        public const string UNKNOWN = "unknown";

        public const string PRODUCER = "producer";

        public const string CREATIONDATE = "creationdate";

        public const string AUTHOR = "author";

        public const string KEYWORDS = "keywords";

        public const string SUBJECT = "subject";

        public const string TITLE = "title";

        public virtual int Type => type;

        public virtual IList<Chunk> Chunks => new List<Chunk>();

        public virtual string Content => content.ToString();

        public virtual string Name => type switch
        {
            2 => "subject",
            3 => "keywords",
            4 => "author",
            1 => "title",
            5 => "producer",
            6 => "creationdate",
            _ => "unknown",
        };

        public Meta(int type, string content)
        {
            this.type = type;
            this.content = new StringBuilder(content);
        }

        public Meta(string tag, string content)
        {
            type = GetType(tag);
            this.content = new StringBuilder(content);
        }

        public virtual bool Process(IElementListener listener)
        {
            try
            {
                return listener.Add(this);
            }
            catch (DocumentException)
            {
                return false;
            }
        }

        public virtual bool IsContent()
        {
            return false;
        }

        public virtual bool IsNestable()
        {
            return false;
        }

        public virtual StringBuilder Append(string str)
        {
            return content.Append(str);
        }

        public static int GetType(string tag)
        {
            if ("subject".Equals(tag))
            {
                return 2;
            }

            if ("keywords".Equals(tag))
            {
                return 3;
            }

            if ("author".Equals(tag))
            {
                return 4;
            }

            if ("title".Equals(tag))
            {
                return 1;
            }

            if ("producer".Equals(tag))
            {
                return 5;
            }

            if ("creationdate".Equals(tag))
            {
                return 6;
            }

            return 0;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
