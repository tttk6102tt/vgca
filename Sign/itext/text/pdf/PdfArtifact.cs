using Sign.itext.error_messages;
using Sign.itext.pdf;
using Sign.itext.pdf.interfaces;
using Sign.SystemItext.util.collections;

namespace Sign.itext.text.pdf
{
    public class PdfArtifact : IAccessibleElement
    {
        public enum ArtifactType
        {
            PAGINATION,
            LAYOUT,
            PAGE,
            BACKGROUND
        }

        protected PdfName role = PdfName.ARTIFACT;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        protected AccessibleElementId id = new AccessibleElementId();

        private static readonly HashSet2<string> allowedArtifactTypes = new HashSet2<string>(new string[4] { "Pagination", "Layout", "Page", "Background" });

        public virtual PdfName Role
        {
            get
            {
                return role;
            }
            set
            {
                role = value;
            }
        }

        public virtual AccessibleElementId ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public virtual bool IsInline => true;

        public virtual PdfString Type
        {
            get
            {
                if (accessibleAttributes == null)
                {
                    return null;
                }

                accessibleAttributes.TryGetValue(PdfName.TYPE, out var value);
                return (PdfString)value;
            }
            set
            {
                if (!allowedArtifactTypes.Contains(value.ToString()))
                {
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("the.artifact.type.1.is.invalid", value));
                }

                SetAccessibleAttribute(PdfName.TYPE, value);
            }
        }

        public virtual PdfArray BBox
        {
            get
            {
                if (accessibleAttributes == null)
                {
                    return null;
                }

                accessibleAttributes.TryGetValue(PdfName.BBOX, out var value);
                return (PdfArray)value;
            }
            set
            {
                SetAccessibleAttribute(PdfName.BBOX, value);
            }
        }

        public virtual PdfArray Attached
        {
            get
            {
                if (accessibleAttributes == null)
                {
                    return null;
                }

                accessibleAttributes.TryGetValue(PdfName.ATTACHED, out var value);
                return (PdfArray)value;
            }
            set
            {
                SetAccessibleAttribute(PdfName.ATTACHED, value);
            }
        }

        public virtual PdfObject GetAccessibleAttribute(PdfName key)
        {
            if (accessibleAttributes != null)
            {
                return accessibleAttributes[key];
            }

            return null;
        }

        public virtual void SetAccessibleAttribute(PdfName key, PdfObject value)
        {
            if (accessibleAttributes == null)
            {
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            }

            accessibleAttributes[key] = value;
        }

        public virtual Dictionary<PdfName, PdfObject> GetAccessibleAttributes()
        {
            return accessibleAttributes;
        }

        public virtual void SetType(ArtifactType type)
        {
            PdfString value = null;
            switch (type)
            {
                case ArtifactType.BACKGROUND:
                    value = new PdfString("Background");
                    break;
                case ArtifactType.LAYOUT:
                    value = new PdfString("Layout");
                    break;
                case ArtifactType.PAGE:
                    value = new PdfString("Page");
                    break;
                case ArtifactType.PAGINATION:
                    value = new PdfString("Pagination");
                    break;
            }

            SetAccessibleAttribute(PdfName.TYPE, value);
        }
    }
}
