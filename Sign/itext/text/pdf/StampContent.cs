using Sign.itext.pdf;

namespace Sign.itext.text.pdf
{
    public class StampContent : PdfContentByte
    {
        internal PdfStamperImp.PageStamp ps;

        internal PageResources pageResources;

        public override PdfContentByte Duplicate => new StampContent((PdfStamperImp)writer, ps);

        internal override PageResources PageResources => pageResources;

        internal StampContent(PdfStamperImp stamper, PdfStamperImp.PageStamp ps)
            : base(stamper)
        {
            this.ps = ps;
            pageResources = ps.pageResources;
        }

        public override void SetAction(PdfAction action, float llx, float lly, float urx, float ury)
        {
            ((PdfStamperImp)writer).AddAnnotation(writer.CreateAnnotation(llx, lly, urx, ury, action, null), ps.pageN);
        }

        internal override void AddAnnotation(PdfAnnotation annot)
        {
            ((PdfStamperImp)writer).AddAnnotation(annot, ps.pageN);
        }
    }
}
