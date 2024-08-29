using System.Collections;

namespace Sign.itext.xml.xmp
{
    public interface IXmpIterator : IEnumerator
    {
        void SkipSubtree();

        void SkipSiblings();
    }
}
