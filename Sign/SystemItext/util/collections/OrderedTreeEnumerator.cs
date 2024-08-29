using System.Collections;

namespace Sign.SystemItext.util.collections
{
    public class OrderedTreeEnumerator : IEnumerator
    {
        private Stack stack;

        private bool keys;

        private bool ascending;

        private OrderedTreeNode tnode;

        private OrderedTreeNode sentinelNode;

        private bool pre = true;

        private IComparable ordKey;

        private object objValue;

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

        public virtual object Value
        {
            get
            {
                return objValue;
            }
            set
            {
                objValue = value;
            }
        }

        public virtual object Current
        {
            get
            {
                if (pre)
                {
                    throw new InvalidOperationException("Current");
                }

                if (!keys)
                {
                    return Value;
                }

                return Key;
            }
        }

        private OrderedTreeEnumerator()
        {
        }

        public OrderedTreeEnumerator(OrderedTreeNode tnode, bool keys, bool ascending, OrderedTreeNode sentinelNode)
        {
            this.sentinelNode = sentinelNode;
            stack = new Stack();
            this.keys = keys;
            this.ascending = ascending;
            this.tnode = tnode;
            Reset();
        }

        public virtual void Reset()
        {
            pre = true;
            stack.Clear();
            if (ascending)
            {
                while (tnode != sentinelNode)
                {
                    stack.Push(tnode);
                    tnode = tnode.Left;
                }
            }
            else
            {
                while (tnode != sentinelNode)
                {
                    stack.Push(tnode);
                    tnode = tnode.Right;
                }
            }
        }

        public virtual bool HasMoreElements()
        {
            return stack.Count > 0;
        }

        public virtual object NextElement()
        {
            if (stack.Count == 0)
            {
                throw new InvalidOperationException("Element not found");
            }

            OrderedTreeNode orderedTreeNode = (OrderedTreeNode)stack.Peek();
            if (ascending)
            {
                if (orderedTreeNode.Right == sentinelNode)
                {
                    OrderedTreeNode orderedTreeNode2 = (OrderedTreeNode)stack.Pop();
                    while (HasMoreElements() && ((OrderedTreeNode)stack.Peek()).Right == orderedTreeNode2)
                    {
                        orderedTreeNode2 = (OrderedTreeNode)stack.Pop();
                    }
                }
                else
                {
                    for (OrderedTreeNode orderedTreeNode3 = orderedTreeNode.Right; orderedTreeNode3 != sentinelNode; orderedTreeNode3 = orderedTreeNode3.Left)
                    {
                        stack.Push(orderedTreeNode3);
                    }
                }
            }
            else if (orderedTreeNode.Left == sentinelNode)
            {
                OrderedTreeNode orderedTreeNode4 = (OrderedTreeNode)stack.Pop();
                while (HasMoreElements() && ((OrderedTreeNode)stack.Peek()).Left == orderedTreeNode4)
                {
                    orderedTreeNode4 = (OrderedTreeNode)stack.Pop();
                }
            }
            else
            {
                for (OrderedTreeNode orderedTreeNode5 = orderedTreeNode.Left; orderedTreeNode5 != sentinelNode; orderedTreeNode5 = orderedTreeNode5.Right)
                {
                    stack.Push(orderedTreeNode5);
                }
            }

            Key = orderedTreeNode.Key;
            Value = orderedTreeNode.Data;
            if (!keys)
            {
                return orderedTreeNode.Data;
            }

            return orderedTreeNode.Key;
        }

        public virtual bool MoveNext()
        {
            if (HasMoreElements())
            {
                NextElement();
                pre = false;
                return true;
            }

            pre = true;
            return false;
        }

        public virtual OrderedTreeEnumerator GetEnumerator()
        {
            return this;
        }
    }
}
