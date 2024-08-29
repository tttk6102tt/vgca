using Sign.itext.pdf.draw;

namespace Sign.itext.text
{
    public class TabStop
    {
        public enum Alignment
        {
            LEFT,
            RIGHT,
            CENTER,
            ANCHOR
        }

        protected float position;

        protected Alignment alignment;

        protected IDrawInterface leader;

        protected char anchorChar = '.';

        public virtual float Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public virtual Alignment Align
        {
            get
            {
                return alignment;
            }
            set
            {
                alignment = value;
            }
        }

        public virtual IDrawInterface Leader
        {
            get
            {
                return leader;
            }
            set
            {
                leader = value;
            }
        }

        public virtual char AnchorChar
        {
            get
            {
                return anchorChar;
            }
            set
            {
                anchorChar = value;
            }
        }

        public static TabStop NewInstance(float currentPosition, float tabInterval)
        {
            currentPosition = (float)Math.Round(currentPosition * 1000f) / 1000f;
            tabInterval = (float)Math.Round(tabInterval * 1000f) / 1000f;
            return new TabStop(currentPosition + tabInterval - currentPosition % tabInterval);
        }

        public TabStop(float position)
            : this(position, Alignment.LEFT)
        {
        }

        public TabStop(float position, IDrawInterface leader)
            : this(position, leader, Alignment.LEFT)
        {
        }

        public TabStop(float position, Alignment alignment)
            : this(position, null, alignment)
        {
        }

        public TabStop(float position, Alignment alignment, char anchorChar)
            : this(position, null, alignment, anchorChar)
        {
        }

        public TabStop(float position, IDrawInterface leader, Alignment alignment)
            : this(position, leader, alignment, '.')
        {
        }

        public TabStop(float position, IDrawInterface leader, Alignment alignment, char anchorChar)
        {
            this.position = position;
            this.leader = leader;
            this.alignment = alignment;
            this.anchorChar = anchorChar;
        }

        public TabStop(TabStop tabStop)
            : this(tabStop.Position, tabStop.Leader, tabStop.Align, tabStop.AnchorChar)
        {
        }

        public virtual float GetPosition(float tabPosition, float currentPosition, float anchorPosition)
        {
            float result = position;
            float num = currentPosition - tabPosition;
            switch (alignment)
            {
                case Alignment.RIGHT:
                    result = ((!(tabPosition + num < position)) ? tabPosition : (position - num));
                    break;
                case Alignment.CENTER:
                    result = ((!(tabPosition + num / 2f < position)) ? tabPosition : (position - num / 2f));
                    break;
                case Alignment.ANCHOR:
                    result = (float.IsNaN(anchorPosition) ? ((!(tabPosition + num < position)) ? tabPosition : (position - num)) : ((!(anchorPosition < position)) ? tabPosition : (position - (anchorPosition - tabPosition))));
                    break;
            }

            return result;
        }
    }
}
