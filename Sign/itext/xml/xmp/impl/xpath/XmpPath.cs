using System.Collections;
using System.Text;

namespace Sign.itext.xml.xmp.impl.xpath
{
    public class XmpPath
    {
        public const uint STRUCT_FIELD_STEP = 1u;

        public const uint QUALIFIER_STEP = 2u;

        public const uint ARRAY_INDEX_STEP = 3u;

        public const uint ARRAY_LAST_STEP = 4u;

        public const uint QUAL_SELECTOR_STEP = 5u;

        public const uint FIELD_SELECTOR_STEP = 6u;

        public const uint SCHEMA_NODE = 2147483648u;

        public const uint STEP_SCHEMA = 0u;

        public const uint STEP_ROOT_PROP = 1u;

        private readonly IList _segments = new ArrayList(5);

        public virtual void Add(XmpPathSegment segment)
        {
            _segments.Add(segment);
        }

        public virtual XmpPathSegment GetSegment(int index)
        {
            return (XmpPathSegment)_segments[index];
        }

        public virtual int Size()
        {
            return _segments.Count;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 1; i < Size(); i++)
            {
                stringBuilder.Append(GetSegment(i));
                if (i < Size() - 1)
                {
                    uint kind = GetSegment(i + 1).Kind;
                    if (kind == 1 || kind == 2)
                    {
                        stringBuilder.Append('/');
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }
}
