namespace Sign.itext.xml.simpleparser.handler
{
    public class NeverNewLineHandler : INewLineHandler
    {
        public virtual bool IsNewLineTag(string tag)
        {
            return false;
        }
    }
}
