using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class PdfLayerMembership : PdfDictionary, IPdfOCG
    {
        public static readonly PdfName ALLON = new PdfName("AllOn");

        public static readonly PdfName ANYON = new PdfName("AnyOn");

        public static readonly PdfName ANYOFF = new PdfName("AnyOff");

        public static readonly PdfName ALLOFF = new PdfName("AllOff");

        internal PdfIndirectReference refi;

        internal PdfArray members = new PdfArray();

        internal Dictionary<PdfLayer, object> layers = new Dictionary<PdfLayer, object>();

        public virtual PdfIndirectReference Ref => refi;

        public virtual Dictionary<PdfLayer, object>.KeyCollection Layers => layers.Keys;

        public virtual PdfName VisibilityPolicy
        {
            set
            {
                Put(PdfName.P, value);
            }
        }

        public virtual PdfVisibilityExpression VisibilityExpression
        {
            set
            {
                Put(PdfName.VE, value);
            }
        }

        public virtual PdfObject PdfObject => this;

        public PdfLayerMembership(PdfWriter writer)
            : base(PdfName.OCMD)
        {
            Put(PdfName.OCGS, members);
            refi = writer.PdfIndirectReference;
        }

        public virtual void AddMember(PdfLayer layer)
        {
            if (!layers.ContainsKey(layer))
            {
                members.Add(layer.Ref);
                layers[layer] = null;
            }
        }
    }
}
