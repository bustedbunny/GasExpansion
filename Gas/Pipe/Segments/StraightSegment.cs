using GasExpansion.Gas.Pipe.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class StraightSegment : Segment
    {

        public override (Rot4, short, short)[] Offsets => _offsets;
        private static readonly (Rot4, short, short)[] _offsets =
        {
            (Rot4.North,0, 1),
            (Rot4.South,1, 0),
            (Rot4.West,2, 3),
            (Rot4.East,3, 2)
        };

    }
}
