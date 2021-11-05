using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace GasExpansion
{


    [HarmonyPatch(typeof(MouseoverReadout), "MouseoverReadoutOnGUI")]
    public class MouseoverReadoutOnGUITranspiler
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo labelMaker = AccessTools.Method(typeof(MouseoverReadoutOnGUITranspiler), "MakeLabelIfRequired");
            FieldInfo BotLeft = AccessTools.Field(typeof(MouseoverReadout), "BotLeft");
            var codes = new List<CodeInstruction>(instructions);
            int num = 0;
            bool skip = true;
            for (var i = 0; i < codes.Count; i++)
            {
                if (num == 7 && skip)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0).WithLabels(codes[i].labels);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, BotLeft);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);

                    yield return new CodeInstruction(OpCodes.Call, labelMaker);
                    yield return new CodeInstruction(OpCodes.Stloc_1);
                    skip = false;
                    yield return codes[i].WithLabels(new Label[] { });
                    continue;
                }
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Stloc_1)
                {
                    num++;
                }
            }
        }

        public static float MakeLabelIfRequired(IntVec3 cell, Vector2 BotLeft, float num)
        {
            GasMapComponent comp = Find.CurrentMap.GetComponent<GasMapComponent>();
            if (comp != null)
            {
                float rectY = num;
                int ind = comp.map.cellIndices.CellToIndex(cell);
                foreach (GasGrid grid in comp.gasGrids)
                {
                    float density = grid.DensityInCell(ind);
                    if (density > 0)
                    {
                        Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - rectY, 999f, 999f), grid.def.LabelCap + " " + Math.Round(density, 2));
                        rectY += 19f;
                    }
                }
                return rectY;
            }
            return num;

        }
        /*
        public static void Postfix()
        {
            IntVec3 c = UI.MouseCell();
            Map map = Find.CurrentMap;
            if (!c.InBounds(map))
            {
                return;
            }
            GasMapComponent comp = map.GetComponent<GasMapComponent>();
            if (comp == null)
            {
                return;
            }
            float scroll = 0;
            foreach (GasGrid grid in comp.gasGrids)
            {
                float density = grid.gasGrid[grid.CellToIndex(c)];
                if (density > 0)
                {
                    Rect rect3 = new(15f, (float)UI.screenHeight - 365f + scroll, 999f, 999f);
                    Widgets.Label(rect3, $"{grid.def.label} {density}");
                    scroll += 15f;
                }
            }
        }
        */
    }


}
