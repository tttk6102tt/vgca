using System.Text;

namespace Sign.itext.text
{
    public class Header : Meta
    {
        private StringBuilder name;

        public override string Name => name.ToString();

        public Header(string name, string content)
            : base(0, content)
        {
            this.name = new StringBuilder(name);
        }
    }
}
