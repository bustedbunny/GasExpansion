using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion.Gas.Pipe
{
    public class Node : Segment
    {
        public GasMapComponent mapComp;

        public override void UpdateConnections()
        {

        }


        public void FindLink()
        {
            if (zeroSegment == null)
            {
                return;
            }
            Segment _curSegment = zeroSegment;
            Segment _prevSegment = this;
            while (_curSegment != null)
            {
                if (_curSegment is Node)
                {
                    mapComp.pipeGrid.AddLink(this, _curSegment as Node);
                    
                }
                else
                {
                    _curSegment = (_curSegment.oneSegment != _prevSegment) ? _curSegment.oneSegment : zeroSegment.zeroSegment;
                }
            }
        }
    }
}
