using GasExpansion.Gas.Pipe;
using GasExpansion.Gas.Pipe.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class PipeTracker
    {
        private readonly HashSet<Link> links;

        public HashSet<Link> Links => links;

        public PipeTracker()
        {
            links = new HashSet<Link>();
        }


        private Link Contains(Node a, Node b)
        {
            foreach (Link link in links)
            {
                if ((link.ANode == a && link.BNode == b) || (link.ANode == b && link.BNode == a))
                {
                    return link;
                }
            }
            return null;
        }

        public Link TryRegisterLink(Node a, Node b)
        {
            Link link = Contains(a, b);
            if (link != null)
            {
                return link;
            }
            return RegisterLink(a, b);
        }

        private Link RegisterLink(Node a, Node b)
        {
            Link link = new Link(a, b);
            if (links.Add(link))
            {
                return link;
            }
            return null;
        }

        public void DeregisterAllDirtyRelatedLinks(Node a)
        {
            if (!a.Spawned)
            {
                links.RemoveWhere(link => link.ANode == a || link.BNode == a);
                return;
            }
            links.RemoveWhere(link => (link.Contains(a) && link.IsDirty()));
        }
        public void RegenerateLinks()
        {
            links.RemoveWhere(link => link.IsDirty());
        }

        public int RemoveLinksOfMap(Map map)
        {
            return Links.RemoveWhere(link => (link?.ANode.Map == null || link.ANode.Map == map) || (link?.BNode.Map == null || link.BNode.Map == map));
        }

        public void Tick()
        {

        }


        public void LogLinks()
        {
            Log.Message("Logging links.");
            foreach (Link link in links)
            {
                Log.Message($"Link: {link.ANode.LabelCap} {link.BNode.LabelCap}");
            }
        }
    }
}
