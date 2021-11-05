using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    [HarmonyPatch(typeof(Explosion), "ShouldCellBeAffectedOnlyByDamage")]
    internal class ShouldCellBeAffectedOnlyByDamagePostfix
    {
        internal static void Postfix(Explosion __instance, IntVec3 c)
        {
            foreach (GasGrid grid in __instance.Map.GetComponent<GasMapComponent>().explosiveGrids)
            {
                grid.Explode(c, __instance.instigator);
            }

        }
    }
}
