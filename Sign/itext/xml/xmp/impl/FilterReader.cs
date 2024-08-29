namespace Sign.itext.xml.xmp.impl
{
    public abstract class FilterReader : TextReader
    {
        protected TextReader inp;

        protected FilterReader(TextReader inp)
        {
            this.inp = TextReader.Synchronized(inp);
        }

        public override int Read()
        {
            return inp.Read();
        }

        public override int Read(char[] cbuf, int off, int len)
        {
            return inp.Read(cbuf, off, len);
        }

        public override void Close()
        {
            inp.Close();
        }
    }
}
