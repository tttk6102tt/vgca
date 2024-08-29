using Sign.itext.text;

namespace Sign.itext
{
    public interface IDocListener : IElementListener, IDisposable
    {
        int PageCount { set; }

        void Open();

        void Close();

        bool NewPage();

        bool SetPageSize(Rectangle pageSize);

        bool SetMargins(float marginLeft, float marginRight, float marginTop, float marginBottom);

        bool SetMarginMirroring(bool marginMirroring);

        bool SetMarginMirroringTopBottom(bool marginMirroringTopBottom);

        void ResetPageCount();
    }
}
