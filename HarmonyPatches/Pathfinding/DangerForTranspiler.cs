using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class DangerForTranspiler
    {
        public static readonly MethodInfo original = AccessTools.Method(typeof(Region), "DangerFor");
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo cells = AccessTools.PropertyGetter(typeof(Region), "Cells");
            MethodInfo map = AccessTools.PropertyGetter(typeof(Region), "Map");
            MethodInfo changeDanger = AccessTools.Method(typeof(DangerForTranspiler), "ChangeDanger");

            var codes = new List<CodeInstruction>(instructions);
            for (var i = codes.Count - 1; i >= 0; i--)
            {
                if (codes[i].opcode == OpCodes.Stloc_1)
                {
                    List<Label> labels = codes[i + 1].labels;
                    codes[i + 1].labels = new List<Label>();
                    codes.InsertRange(i + 1, new List<CodeInstruction>()
                    {
                        new CodeInstruction (OpCodes.Ldarg_0).WithLabels(labels),
                        new CodeInstruction(OpCodes.Call, cells),
                        new CodeInstruction (OpCodes.Ldarg_0),
                        new CodeInstruction (OpCodes.Call, map),
                        new CodeInstruction (OpCodes.Ldarg_1),
                        new CodeInstruction (OpCodes.Ldloc_1),
                        new CodeInstruction (OpCodes.Call, changeDanger),
                        new CodeInstruction (OpCodes.Stloc_1)
                    });
                    break;
                }
            }
            return codes;

        }

        public static Danger ChangeDanger(IEnumerable<IntVec3> cells, Map map, Pawn pawn, Danger danger)
        {
            if (danger == Danger.Deadly)
            {
                return danger;
            }
            GasMapComponent comp = map.GetComponent<GasMapComponent>();
            int num = 0;
            foreach (IntVec3 cell in cells)
            {
                if (comp.grid.pathTracker.ShouldPathThrough(cell, pawn))
                {
                    num++;
                }
                if (num > 3)
                {
                    return Danger.Deadly;
                }
            }
            if (num > 0)
            {
                return Danger.Some;
            }
            else return danger;

        }
    }
    
}
