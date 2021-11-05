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
    [HarmonyPatch]
    public class BestAttackTargetTranspiler
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Inner(typeof(AttackTargetFinder), "<>c__DisplayClass5_0").GetMethod("<BestAttackTarget>b__0", (BindingFlags.NonPublic | BindingFlags.Instance));
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            MethodInfo losValidator = AccessTools.Method(typeof(BestAttackTargetTranspiler), nameof(BestAttackTargetTranspiler.LosValidator));
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 5 && codes[i].opcode == OpCodes.Call && codes[i + 1].opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Call, losValidator);
                    yield return codes[codes.Count - 1];
                    yield break;
                }
                yield return codes[i];
            }
        }
        private static bool LosValidator(IntVec3 cell, Map map)
        {
            IEnumerable<GasGrid> list = map.GetComponent<GasMapComponent>().gasGrids.Where(x => x.def.blockVision);
            foreach (GasGrid grid in list)
            {
                if (grid.DensityInCell(cell) < 100)
                {
                    continue;
                }
                return false;
            }
            return true;
        }
    }

}
