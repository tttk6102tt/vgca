using Sign.itext.pdf.interfaces;
using Sign.itext.text;
using Sign.itext.text.pdf;

namespace Sign.itext.pdf
{
    public class PdfTemplate : PdfContentByte, IAccessibleElement
    {
        public const int TYPE_TEMPLATE = 1;

        public const int TYPE_IMPORTED = 2;

        public const int TYPE_PATTERN = 3;

        protected int type;

        protected PdfIndirectReference thisReference;

        protected PageResources pageResources;

        protected Rectangle bBox = new Rectangle(0f, 0f);

        protected PdfArray matrix;

        protected PdfTransparencyGroup group;

        protected IPdfOCG layer;

        protected PdfIndirectReference pageReference;

        protected bool contentTagged;

        private PdfDictionary additional;

        protected PdfName role = PdfName.FIGURE;

        protected Dictionary<PdfName, PdfObject> accessibleAttributes;

        private AccessibleElementId id;

        public virtual float Width
        {
            get
            {
                return bBox.Width;
            }
            set
            {
                bBox.Left = 0f;
                bBox.Right = value;
            }
        }

        public virtual float Height
        {
            get
            {
                return bBox.Height;
            }
            set
            {
                bBox.Bottom = 0f;
                bBox.Top = value;
            }
        }

        public virtual Rectangle BoundingBox
        {
            get
            {
                return bBox;
            }
            set
            {
                bBox = value;
            }
        }

        public virtual IPdfOCG Layer
        {
            get
            {
                return layer;
            }
            set
            {
                layer = value;
            }
        }

        internal PdfArray Matrix => matrix;

        public virtual PdfIndirectReference IndirectReference
        {
            get
            {
                if (thisReference == null)
                {
                    thisReference = writer.PdfIndirectReference;
                }

                return thisReference;
            }
        }

        internal virtual PdfObject Resources => PageResources.Resources;

        public override PdfContentByte Duplicate
        {
            get
            {
                PdfTemplate pdfTemplate = new PdfTemplate();
                pdfTemplate.writer = writer;
                pdfTemplate.pdf = pdf;
                pdfTemplate.thisReference = thisReference;
                pdfTemplate.pageResources = pageResources;
                pdfTemplate.bBox = new Rectangle(bBox);
                pdfTemplate.group = group;
                pdfTemplate.layer = layer;
                if (matrix != null)
                {
                    pdfTemplate.matrix = new PdfArray(matrix);
                }

                pdfTemplate.separator = separator;
                pdfTemplate.additional = additional;
                return pdfTemplate;
            }
        }

        public virtual int Type => type;

        internal override PageResources PageResources => pageResources;

        public virtual PdfTransparencyGroup Group
        {
            get
            {
                return group;
            }
            set
            {
                group = value;
            }
        }

        public virtual PdfDictionary Additional
        {
            get
            {
                return additional;
            }
            set
            {
                additional = value;
            }
        }

        protected override PdfIndirectReference CurrentPage => pageReference ?? writer.CurrentPage;

        public virtual PdfIndirectReference PageReference
        {
            get
            {
                return pageReference;
            }
            set
            {
                pageReference = value;
            }
        }

        public virtual bool ContentTagged
        {
            get
            {
                return contentTagged;
            }
            set
            {
                contentTagged = value;
            }
        }

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
                if (id == null)
                {
                    id = new AccessibleElementId();
                }

                return id;
            }
            set
            {
                id = value;
            }
        }

        public virtual bool IsInline => true;

        protected PdfTemplate()
            : base(null)
        {
            type = 1;
        }

        internal PdfTemplate(PdfWriter wr)
            : base(wr)
        {
            type = 1;
            pageResources = new PageResources();
            pageResources.AddDefaultColor(wr.DefaultColorspace);
            thisReference = writer.PdfIndirectReference;
        }

        public static PdfTemplate CreateTemplate(PdfWriter writer, float width, float height)
        {
            return CreateTemplate(writer, width, height, null);
        }

        internal static PdfTemplate CreateTemplate(PdfWriter writer, float width, float height, PdfName forcedName)
        {
            PdfTemplate pdfTemplate = new PdfTemplate(writer);
            pdfTemplate.Width = width;
            pdfTemplate.Height = height;
            writer.AddDirectTemplateSimple(pdfTemplate, forcedName);
            return pdfTemplate;
        }

        public override bool IsTagged()
        {
            if (base.IsTagged())
            {
                return contentTagged;
            }

            return false;
        }

        public virtual void SetMatrix(float a, float b, float c, float d, float e, float f)
        {
            matrix = new PdfArray();
            matrix.Add(new PdfNumber(a));
            matrix.Add(new PdfNumber(b));
            matrix.Add(new PdfNumber(c));
            matrix.Add(new PdfNumber(d));
            matrix.Add(new PdfNumber(e));
            matrix.Add(new PdfNumber(f));
        }

        public virtual void BeginVariableText()
        {
            content.Append("/Tx BMC ");
        }

        public virtual void EndVariableText()
        {
            content.Append("EMC ");
        }

        public virtual PdfStream GetFormXObject(int compressionLevel)
        {
            return new PdfFormXObject(this, compressionLevel);
        }

        public virtual PdfObject GetAccessibleAttribute(PdfName key)
        {
            if (accessibleAttributes != null)
            {
                accessibleAttributes.TryGetValue(key, out var value);
                return value;
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
    }
}
