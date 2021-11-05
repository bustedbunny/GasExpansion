using HarmonyLib;
using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

namespace GasExpansion
{

    [HarmonyPatch(typeof(SmokepopBelt), "Notify_BulletImpactNearby")]
    public class Notify_BulletImpactNearbyTranspiler
    {
        private static readonly MethodInfo coveredByGas = AccessTools.Method(typeof(Notify_BulletImpactNearbyTranspiler), nameof(Notify_BulletImpactNearbyTranspiler.CoveredByGas));

        private static bool CoveredByGas(Pawn pawn)
        {
            foreach (GasGrid grid in pawn.Map.GetComponent<GasMapComponent>().gasGrids.Where(x => x.def.blockVision))
            {
               if ( grid.DensityInCell(pawn.Position) > 0)
                {
                    return true;
                }
            }
            return false;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 5 && codes[i].opcode == OpCodes.Brtrue_S && codes[i + 1].opcode == OpCodes.Ldloc_0)
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, coveredByGas);
                    i += 15;
                }
                else
                {
                    yield return codes[i];
                }
            }
        }
    }


}
