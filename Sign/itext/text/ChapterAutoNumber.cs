using Sign.itext.error_messages;

namespace Sign.itext.text
{
    public class ChapterAutoNumber : Chapter
    {
        protected bool numberSet;

        public ChapterAutoNumber(Paragraph para)
            : base(para, 0)
        {
        }

        public ChapterAutoNumber(string title)
            : base(title, 0)
        {
        }

        public override Section AddSection(string title)
        {
            if (AddedCompletely)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("this.largeelement.has.already.been.added.to.the.document"));
            }

            return AddSection(title, 2);
        }

        public override Section AddSection(Paragraph title)
        {
            if (AddedCompletely)
            {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("this.largeelement.has.already.been.added.to.the.document"));
            }

            return AddSection(title, 2);
        }

        public virtual int SetAutomaticNumber(int number)
        {
            if (!numberSet)
            {
                number++;
                base.SetChapterNumber(number);
                numberSet = true;
            }

            return number;
        }
    }
}
