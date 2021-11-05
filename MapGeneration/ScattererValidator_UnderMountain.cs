using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class ScattererValidator_UnderMountain : ScattererValidator
    {
        public int radius;
        public TerrainAffordanceDef affordance;
        public override bool Allows(IntVec3 c, Map map)
        {
            CellRect cellRect = CellRect.CenteredOn(c, radius);
            for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
            {
                for (int j = cellRect.minX; j <= cellRect.maxX; j++)
                {
                    IntVec3 c2 = new(j, 0, i);
                    if (!c2.InBounds(map))
                    {
                        return false;
                    }
                    if (c2.InNoBuildEdgeArea(map))
                    {
                        return false;
                    }
                    if (affordance != null && !c2.GetTerrain(map).affordances.Contains(affordance))
                    {
                        return false;
                    }
                    if (c2.GetEdifice(map) is Mineable)
                    {

                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
