using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace GasExpansion
{

    [HarmonyPatch(typeof(ShotReport), nameof(ShotReport.HitReportFor))]
    public class HitReportForTranspiler
    {
        private static readonly MethodInfo gasFactor = AccessTools.Method(typeof(HitReportForTranspiler), nameof(GasFactor));
        private static readonly FieldInfo factorToChange = AccessTools.Field(typeof(ShotReport), nameof(ShotReport.factorFromShooterAndDist));

        private static float GasFactor(IEnumerable<IntVec3> list, Thing caster)
        {
            
            Map map = caster.Map;
            IEnumerable<GasGrid> list2 = caster.Map.GetComponent<GasMapComponent>().grid.gasGrids.Where(x => x.def.blockVision);
            float multiplier = 1f;
            foreach (IntVec3 cell in list)
            {
                foreach (GasGrid grid in list2)
                {
                    float num = grid.DensityInCell(cell);
                    if (num < 30)
                    {
                        continue;
                    }
                    multiplier *= 1 - (Math.Min(256, num) / 256);
                }
            }
            Log.Message(multiplier.ToString());
            return Math.Max(0.3f, multiplier);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 5 && codes[i].opcode == OpCodes.Ldloca_S && codes[i + 1].opcode == OpCodes.Ldflda && codes[i + 2].opcode == OpCodes.Call)
                {
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return codes[i];
                    yield return codes[i + 1];
                    yield return codes[i + 2];
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, gasFactor);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);

                    yield return new CodeInstruction(OpCodes.Ldfld, factorToChange);
                    yield return new CodeInstruction(OpCodes.Mul);

                    yield return new CodeInstruction(OpCodes.Stfld, factorToChange);
                    i += 39;
                }
                {
                    yield return codes[i];
                }
            }
        }
    }

}
