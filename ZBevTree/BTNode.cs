using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZBevTree
{
    public class BTNode
    {
        private List<BTNode> Childrens { get; set; }

        private int MaxChildCount { get; set; }

        public BTNode() : this(-1) { }

        public BTNode(int maxChildCount)
        {
            Childrens = new List<BTNode>();
            if (maxChildCount > 0)
            {
                Childrens.Capacity = maxChildCount;
            }
            MaxChildCount = maxChildCount;
        }

        public BTNode AddChild(BTNode node)
        {
            if (MaxChildCount >= 0 && Childrens.Count >= MaxChildCount)
            {
                throw new Exception("BevTree超出最大子节点个数");
            }
            Childrens.Add(node);
            return this;
        }

        public int GetChildCount()
        {
            return Childrens.Count;
        }

        public bool IsIndexValid(int index)
        {
            return index >= 0 && index < Childrens.Count;
        }

        public T GetChild<T>(int index) where T : BTNode
        {
            if (index < 0 || index > Childrens.Count) return null;
            return Childrens[index] as T;
        }
    }
}
