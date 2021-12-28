using GasExpansion.Gas.Pipe.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion.Gas.Pipe
{
    public class Link
    {
        public Link(Node a, Node b)
        {
            aNode = a;
            bNode = b;
            linksA = new List<Link>();
            linksB = new List<Link>();
        }
        private readonly Node aNode;
        private readonly Node bNode;
        public Node ANode => aNode;
        public Node BNode => bNode;

        private readonly List<Link> linksA;
        private readonly List<Link> linksB;

        public List<Link> LinksA;
        public List<Link> LinksB;

        private readonly List<Segment> segments;
        public bool Contains(Node node)
        {
            return (ANode == node || BNode == node);
        }

        public bool IsDirty()
        {
            if (ANode == null || BNode == null)
            {
                return true;
            }
            if (!ANode.Spawned || !ANode.ConnectedNodes.Contains(BNode)) return true;
            if (!BNode.Spawned || !BNode.ConnectedNodes.Contains(ANode)) return true;
            return false;
        }
    }
}
