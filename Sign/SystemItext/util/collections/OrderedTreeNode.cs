namespace Sign.SystemItext.util.collections
{
    public class OrderedTreeNode
    {
        public const bool RED = false;

        public const bool BLACK = true;

        private IComparable ordKey;

        private object objData;

        private bool intColor;

        private OrderedTreeNode rbnLeft;

        private OrderedTreeNode rbnRight;

        private OrderedTreeNode rbnParent;

        public virtual IComparable Key
        {
            get
            {
                return ordKey;
            }
            set
            {
                ordKey = value;
            }
        }

        public virtual object Data
        {
            get
            {
                return objData;
            }
            set
            {
                objData = value;
            }
        }

        public virtual bool Color
        {
            get
            {
                return intColor;
            }
            set
            {
                intColor = value;
            }
        }

        public virtual OrderedTreeNode Left
        {
            get
            {
                return rbnLeft;
            }
            set
            {
                rbnLeft = value;
            }
        }

        public virtual OrderedTreeNode Right
        {
            get
            {
                return rbnRight;
            }
            set
            {
                rbnRight = value;
            }
        }

        public virtual OrderedTreeNode Parent
        {
            get
            {
                return rbnParent;
            }
            set
            {
                rbnParent = value;
            }
        }

        public OrderedTreeNode()
        {
            Color = false;
        }
    }
}
