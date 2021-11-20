using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CombatExtended;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GasExpansion
{

    [HarmonyPatch]
    public class DoExplosionPrefix
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GenExplosion), "DoExplosion");
            if (ModLister.HasActiveModWithName("Combat Extended"))
            {
                yield return AccessTools.TypeByName("GenExplosionCE").GetMethod("DoExplosion");
            }
        }
        public static bool Prefix(IntVec3 center, Map map, DamageDef damType, float radius)
        {
            if (map == null)
            {
                return true;
            }
            if (damType != DamageDefOf.Smoke && damType != DamageDefOf.Extinguish)
            {
                return true;
            }
            damType.soundExplosion.PlayOneShot(new TargetInfo(center, map));
            if (damType == DamageDefOf.Smoke)
            {
                map.GetComponent<GasMapComponent>().grid.gasGrids.Find(x => x.def == DefOfClass.GasExpansion_ConcealingSmoke).CreateGas(center, Mathf.Pow(radius, 2) * 500);
            }
            else
            {
                foreach (GasGrid grid in map.GetComponent<GasMapComponent>().grid.gasGrids)
                {
                    int num = GenRadial.NumCellsInRadius(radius);
                    for (int i = 0; i < num; i++)
                    {
                        IntVec3 intVec = center + GenRadial.RadialPattern[i];
                        if (!intVec.InBounds(map) || grid.DensityInCell(intVec) == 0)
                        {
                            continue;
                        }
                        if (GenSight.LineOfSight(center, intVec, map, skipFirstCell: true))
                        {
                            int ind = grid.CellToIndex(intVec);
                            grid.gasGrid[ind] -= 512f;
                            if (grid.gasGrid[ind] <= 0) grid.gases.Remove(ind);

                        }
                    }
                }
                return true;
            }

            return false;
        }
    }

}
