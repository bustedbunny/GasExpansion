using GasExpansion.Gas.Pipe.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class AngledSegment : Segment
    {
        public override (Rot4, short, short)[] Offsets => _offsets;
        private static readonly (Rot4, short, short)[] _offsets =
        {
            (Rot4.North,0, 3),
            (Rot4.South,1, 2),
            (Rot4.West,2, 0),
            (Rot4.East,3, 1)
        };

    }
}

