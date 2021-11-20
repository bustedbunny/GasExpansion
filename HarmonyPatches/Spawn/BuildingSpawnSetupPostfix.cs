using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{


    [HarmonyPatch(typeof(Building), "SpawnSetup")]
    public class BuildingSpawnSetupPostfix
    {
        public static void Postfix(Building __instance)
        {
            IntVec3 pos = __instance.Position;
            Map map = __instance.Map;
            if (!pos.InBounds(map))
            {
                return;
            }
            if (map != null && pos != null)
            {
                try
                {
                    GasMapComponent comp = map.GetComponent<GasMapComponent>();
                    if (comp != null)
                    {
                        foreach (GasGrid grid in comp.grid.gasGrids)
                        {
                            grid.MoveSomewhereElse(pos);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
