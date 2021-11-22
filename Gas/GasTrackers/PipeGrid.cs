using GasExpansion.Gas.Pipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasExpansion.Gas.GasTrackers
{
    public class Link
    {
        public Node firstNode;
        public Node secondNode;
    }
    public class PipeGrid
    {
        List<Link> links = new();

        public void AddLink(Node first, Node second)
        {
            foreach (Link link in links)
            {
                if (link.firstNode == first && link.secondNode == second || link.firstNode == second && link.secondNode == first)
                {
                    return;
                }
            }
            Link newLink = new Link();
            newLink.firstNode = first;
            newLink.secondNode = second;
            links.Add(newLink);
        }
    }
}
