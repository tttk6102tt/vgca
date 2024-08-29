namespace Sign.SystemItext.util.collections
{
    public class OrderedTree
    {
        private int intCount;

        private OrderedTreeNode rbTree;

        private OrderedTreeNode sentinelNode;

        private OrderedTreeNode lastNodeFound;

        public object this[IComparable key]
        {
            get
            {
                return GetData(key);
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                int num = 0;
                OrderedTreeNode orderedTreeNode = new OrderedTreeNode();
                for (OrderedTreeNode orderedTreeNode2 = rbTree; orderedTreeNode2 != sentinelNode; orderedTreeNode2 = ((num <= 0) ? orderedTreeNode2.Left : orderedTreeNode2.Right))
                {
                    orderedTreeNode.Parent = orderedTreeNode2;
                    num = key.CompareTo(orderedTreeNode2.Key);
                    if (num == 0)
                    {
                        lastNodeFound = orderedTreeNode2;
                        orderedTreeNode2.Data = value;
                        return;
                    }
                }

                orderedTreeNode.Key = key;
                orderedTreeNode.Data = value;
                orderedTreeNode.Left = sentinelNode;
                orderedTreeNode.Right = sentinelNode;
                if (orderedTreeNode.Parent != null)
                {
                    num = orderedTreeNode.Key.CompareTo(orderedTreeNode.Parent.Key);
                    if (num > 0)
                    {
                        orderedTreeNode.Parent.Right = orderedTreeNode;
                    }
                    else
                    {
                        orderedTreeNode.Parent.Left = orderedTreeNode;
                    }
                }
                else
                {
                    rbTree = orderedTreeNode;
                }

                RestoreAfterInsert(orderedTreeNode);
                lastNodeFound = orderedTreeNode;
                intCount++;
            }
        }

        public virtual OrderedTreeEnumerator Keys => KeyElements(ascending: true);

        public virtual OrderedTreeEnumerator Values => Elements(ascending: true);

        public virtual int Count => intCount;

        public OrderedTree()
        {
            sentinelNode = new OrderedTreeNode();
            sentinelNode.Left = (sentinelNode.Right = sentinelNode);
            sentinelNode.Parent = null;
            sentinelNode.Color = true;
            rbTree = sentinelNode;
            lastNodeFound = sentinelNode;
        }

        public virtual void Add(IComparable key, object data)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            OrderedTreeNode orderedTreeNode = new OrderedTreeNode();
            OrderedTreeNode orderedTreeNode2 = rbTree;
            while (orderedTreeNode2 != sentinelNode)
            {
                orderedTreeNode.Parent = orderedTreeNode2;
                int num = key.CompareTo(orderedTreeNode2.Key);
                if (num == 0)
                {
                    throw new ArgumentException("Key duplicated");
                }

                orderedTreeNode2 = ((num <= 0) ? orderedTreeNode2.Left : orderedTreeNode2.Right);
            }

            orderedTreeNode.Key = key;
            orderedTreeNode.Data = data;
            orderedTreeNode.Left = sentinelNode;
            orderedTreeNode.Right = sentinelNode;
            if (orderedTreeNode.Parent != null)
            {
                if (orderedTreeNode.Key.CompareTo(orderedTreeNode.Parent.Key) > 0)
                {
                    orderedTreeNode.Parent.Right = orderedTreeNode;
                }
                else
                {
                    orderedTreeNode.Parent.Left = orderedTreeNode;
                }
            }
            else
            {
                rbTree = orderedTreeNode;
            }

            RestoreAfterInsert(orderedTreeNode);
            lastNodeFound = orderedTreeNode;
            intCount++;
        }

        private void RestoreAfterInsert(OrderedTreeNode x)
        {
            while (x != rbTree && !x.Parent.Color)
            {
                OrderedTreeNode right;
                if (x.Parent == x.Parent.Parent.Left)
                {
                    right = x.Parent.Parent.Right;
                    if (right != null && !right.Color)
                    {
                        x.Parent.Color = true;
                        right.Color = true;
                        x.Parent.Parent.Color = false;
                        x = x.Parent.Parent;
                        continue;
                    }

                    if (x == x.Parent.Right)
                    {
                        x = x.Parent;
                        RotateLeft(x);
                    }

                    x.Parent.Color = true;
                    x.Parent.Parent.Color = false;
                    RotateRight(x.Parent.Parent);
                    continue;
                }

                right = x.Parent.Parent.Left;
                if (right != null && !right.Color)
                {
                    x.Parent.Color = true;
                    right.Color = true;
                    x.Parent.Parent.Color = false;
                    x = x.Parent.Parent;
                    continue;
                }

                if (x == x.Parent.Left)
                {
                    x = x.Parent;
                    RotateRight(x);
                }

                x.Parent.Color = true;
                x.Parent.Parent.Color = false;
                RotateLeft(x.Parent.Parent);
            }

            rbTree.Color = true;
        }

        public virtual void RotateLeft(OrderedTreeNode x)
        {
            OrderedTreeNode right = x.Right;
            x.Right = right.Left;
            if (right.Left != sentinelNode)
            {
                right.Left.Parent = x;
            }

            if (right != sentinelNode)
            {
                right.Parent = x.Parent;
            }

            if (x.Parent != null)
            {
                if (x == x.Parent.Left)
                {
                    x.Parent.Left = right;
                }
                else
                {
                    x.Parent.Right = right;
                }
            }
            else
            {
                rbTree = right;
            }

            right.Left = x;
            if (x != sentinelNode)
            {
                x.Parent = right;
            }
        }

        public virtual void RotateRight(OrderedTreeNode x)
        {
            OrderedTreeNode left = x.Left;
            x.Left = left.Right;
            if (left.Right != sentinelNode)
            {
                left.Right.Parent = x;
            }

            if (left != sentinelNode)
            {
                left.Parent = x.Parent;
            }

            if (x.Parent != null)
            {
                if (x == x.Parent.Right)
                {
                    x.Parent.Right = left;
                }
                else
                {
                    x.Parent.Left = left;
                }
            }
            else
            {
                rbTree = left;
            }

            left.Right = x;
            if (x != sentinelNode)
            {
                x.Parent = left;
            }
        }

        public virtual bool ContainsKey(IComparable key)
        {
            OrderedTreeNode orderedTreeNode = rbTree;
            int num = 0;
            while (orderedTreeNode != sentinelNode)
            {
                num = key.CompareTo(orderedTreeNode.Key);
                if (num == 0)
                {
                    lastNodeFound = orderedTreeNode;
                    return true;
                }

                orderedTreeNode = ((num >= 0) ? orderedTreeNode.Right : orderedTreeNode.Left);
            }

            return false;
        }

        public virtual object GetData(IComparable key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            OrderedTreeNode orderedTreeNode = rbTree;
            while (orderedTreeNode != sentinelNode)
            {
                int num = key.CompareTo(orderedTreeNode.Key);
                if (num == 0)
                {
                    lastNodeFound = orderedTreeNode;
                    return orderedTreeNode.Data;
                }

                orderedTreeNode = ((num >= 0) ? orderedTreeNode.Right : orderedTreeNode.Left);
            }

            return null;
        }

        public virtual IComparable GetMinKey()
        {
            OrderedTreeNode left = rbTree;
            if (left == null || left == sentinelNode)
            {
                throw new InvalidOperationException("Tree is empty");
            }

            while (left.Left != sentinelNode)
            {
                left = left.Left;
            }

            lastNodeFound = left;
            return left.Key;
        }

        public virtual IComparable GetMaxKey()
        {
            OrderedTreeNode right = rbTree;
            if (right == null || right == sentinelNode)
            {
                throw new InvalidOperationException("Tree is empty");
            }

            while (right.Right != sentinelNode)
            {
                right = right.Right;
            }

            lastNodeFound = right;
            return right.Key;
        }

        public virtual object GetMinValue()
        {
            return GetData(GetMinKey());
        }

        public virtual object GetMaxValue()
        {
            return GetData(GetMaxKey());
        }

        public virtual OrderedTreeEnumerator GetEnumerator()
        {
            return Elements(ascending: true);
        }

        public virtual OrderedTreeEnumerator KeyElements(bool ascending)
        {
            return new OrderedTreeEnumerator(rbTree, keys: true, ascending, sentinelNode);
        }

        public virtual OrderedTreeEnumerator Elements()
        {
            return Elements(ascending: true);
        }

        public virtual OrderedTreeEnumerator Elements(bool ascending)
        {
            return new OrderedTreeEnumerator(rbTree, keys: false, ascending, sentinelNode);
        }

        public virtual bool IsEmpty()
        {
            if (rbTree != null)
            {
                return rbTree == sentinelNode;
            }

            return true;
        }

        public virtual void Remove(IComparable key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            OrderedTreeNode orderedTreeNode;
            if (((lastNodeFound.Key == null) ? (-1) : key.CompareTo(lastNodeFound.Key)) == 0)
            {
                orderedTreeNode = lastNodeFound;
            }
            else
            {
                orderedTreeNode = rbTree;
                while (orderedTreeNode != sentinelNode)
                {
                    int num = key.CompareTo(orderedTreeNode.Key);
                    if (num == 0)
                    {
                        break;
                    }

                    orderedTreeNode = ((num >= 0) ? orderedTreeNode.Right : orderedTreeNode.Left);
                }

                if (orderedTreeNode == sentinelNode)
                {
                    return;
                }
            }

            Delete(orderedTreeNode);
            intCount--;
        }

        private void Delete(OrderedTreeNode z)
        {
            OrderedTreeNode orderedTreeNode = new OrderedTreeNode();
            OrderedTreeNode orderedTreeNode2;
            if (z.Left == sentinelNode || z.Right == sentinelNode)
            {
                orderedTreeNode2 = z;
            }
            else
            {
                orderedTreeNode2 = z.Right;
                while (orderedTreeNode2.Left != sentinelNode)
                {
                    orderedTreeNode2 = orderedTreeNode2.Left;
                }
            }

            orderedTreeNode = ((orderedTreeNode2.Left == sentinelNode) ? orderedTreeNode2.Right : orderedTreeNode2.Left);
            orderedTreeNode.Parent = orderedTreeNode2.Parent;
            if (orderedTreeNode2.Parent != null)
            {
                if (orderedTreeNode2 == orderedTreeNode2.Parent.Left)
                {
                    orderedTreeNode2.Parent.Left = orderedTreeNode;
                }
                else
                {
                    orderedTreeNode2.Parent.Right = orderedTreeNode;
                }
            }
            else
            {
                rbTree = orderedTreeNode;
            }

            if (orderedTreeNode2 != z)
            {
                z.Key = orderedTreeNode2.Key;
                z.Data = orderedTreeNode2.Data;
            }

            if (orderedTreeNode2.Color)
            {
                RestoreAfterDelete(orderedTreeNode);
            }

            lastNodeFound = sentinelNode;
        }

        private void RestoreAfterDelete(OrderedTreeNode x)
        {
            while (x != rbTree && x.Color)
            {
                OrderedTreeNode right;
                if (x == x.Parent.Left)
                {
                    right = x.Parent.Right;
                    if (!right.Color)
                    {
                        right.Color = true;
                        x.Parent.Color = false;
                        RotateLeft(x.Parent);
                        right = x.Parent.Right;
                    }

                    if (right.Left.Color && right.Right.Color)
                    {
                        right.Color = false;
                        x = x.Parent;
                        continue;
                    }

                    if (right.Right.Color)
                    {
                        right.Left.Color = true;
                        right.Color = false;
                        RotateRight(right);
                        right = x.Parent.Right;
                    }

                    right.Color = x.Parent.Color;
                    x.Parent.Color = true;
                    right.Right.Color = true;
                    RotateLeft(x.Parent);
                    x = rbTree;
                    continue;
                }

                right = x.Parent.Left;
                if (!right.Color)
                {
                    right.Color = true;
                    x.Parent.Color = false;
                    RotateRight(x.Parent);
                    right = x.Parent.Left;
                }

                if (right.Right.Color && right.Left.Color)
                {
                    right.Color = false;
                    x = x.Parent;
                    continue;
                }

                if (right.Left.Color)
                {
                    right.Right.Color = true;
                    right.Color = false;
                    RotateLeft(right);
                    right = x.Parent.Left;
                }

                right.Color = x.Parent.Color;
                x.Parent.Color = true;
                right.Left.Color = true;
                RotateRight(x.Parent);
                x = rbTree;
            }

            x.Color = true;
        }

        public virtual void RemoveMin()
        {
            if (rbTree != null && rbTree != sentinelNode)
            {
                Remove(GetMinKey());
            }
        }

        public virtual void RemoveMax()
        {
            if (rbTree != null && rbTree != sentinelNode)
            {
                Remove(GetMaxKey());
            }
        }

        public virtual void Clear()
        {
            rbTree = sentinelNode;
            intCount = 0;
        }
    }
}
