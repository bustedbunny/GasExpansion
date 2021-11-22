using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion.Gas.Pipe
{
    public static class PipeUtility
    {
        public static Segment GetFirstSegment(IntVec3 cell, Map map)
        {
            foreach (Thing item in cell.GetThingList(map))
            {
                if (item is Segment segment)
                {
                    return segment;
                }
            }
            return null;
        }
    }
}
