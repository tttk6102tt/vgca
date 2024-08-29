namespace Sign.itext.text
{
    public class Chapter : Section
    {
        public override int Type => 16;

        public Chapter(int number)
            : base(null, 1)
        {
            numbers = new List<int>();
            numbers.Add(number);
            triggerNewPage = true;
        }

        public Chapter(Paragraph title, int number)
            : base(title, 1)
        {
            numbers = new List<int>();
            numbers.Add(number);
            triggerNewPage = true;
        }

        public Chapter(string title, int number)
            : this(new Paragraph(title), number)
        {
        }

        public override bool IsNestable()
        {
            return false;
        }
    }
}
