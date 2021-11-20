using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace GasExpansion
{
    public class GasExpansion_PawnResistance
    {
        public bool toxic = false;
        public bool corrosive = false;
    }
    public class PathFinderTranspiler
    {
        public static readonly MethodInfo original = AccessTools.Method(typeof(PathFinder), "FindPath", new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms), typeof(PathEndMode), typeof(PathFinderCostTuning) });
            
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            FieldInfo fieldInfo = AccessTools.Field(typeof(PathFinder), "map");
            FieldInfo fieldInfo2 = AccessTools.Field(typeof(TerrainDef), "extraNonDraftedPerceivedPathCost");
        //    MethodInfo methodinfo3 = AccessTools.Method(typeof(GasMapComponent), "GetResistance");
            MethodInfo methodInfo = AccessTools.Method(typeof(PathFinderTranspiler), "PathCostAdjastment");
            MethodInfo methodInfo2 = AccessTools.Method(typeof(PathFinderTranspiler), "GetComp");
            bool flag = false;
            List<CodeInstruction> list = new(instructions);

            bool skip1 = true;
            for (int i = 20; i < list.Count; i++)
            {
                if (skip1 && list[i].opcode == OpCodes.Ldarg_0 && list[i + 1].opcode == OpCodes.Ldarg_0 &&
                    list[i + 2].opcode == OpCodes.Ldfld && list[i + 3].opcode == OpCodes.Ldfld && list[i + 4].opcode == OpCodes.Stfld)
                {
                    il.DeclareLocal(typeof(GasMapComponent));
            //        il.DeclareLocal(typeof(GasExpansion_PawnResistance));
                    list.InsertRange(i + 5, new List<CodeInstruction>
                    {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, fieldInfo),
                    new CodeInstruction(OpCodes.Call, methodInfo2),
                    new CodeInstruction(OpCodes.Stloc_S, 59),
               //     new CodeInstruction(OpCodes.Ldloc_0),
               //     new CodeInstruction(OpCodes.Call, methodinfo3),
               //     new CodeInstruction(OpCodes.Stloc_S, 60)
                });
                    skip1 = false;
                }

                if (list[i].opcode == OpCodes.Ldfld && list[i].OperandIs(fieldInfo2))
                {
                    if (skip1)
                    {
                        Log.Error("[GasExpansion] Pathfinding transpiler is not applied correctly.");
                        break;
                    }
                    list.InsertRange(i + 3, new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldloc_S,46),
                    new CodeInstruction(OpCodes.Ldloc_S, 59),
                    new CodeInstruction(OpCodes.Ldloc_S, 43),
              //      new CodeInstruction(OpCodes.Ldloc_S, 60),
                    new CodeInstruction(OpCodes.Call, methodInfo),
                    new CodeInstruction(OpCodes.Stloc_S,46)
                });

                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                Log.Warning("[GasExpansion] Transpiler could not find target. There may be a mod conflict, or RimWorld updated?");
            }
            return list.AsEnumerable();
        }

        internal static int PathCostAdjastment(int cost, GasMapComponent comp, int ind)
        {
            if (comp.grid.pathTracker.IsDangerCell(ind))
            {
                return cost + 400;
            }
            return cost;
        }
        internal static GasMapComponent GetComp(Map map)
        {
            return map.GetComponent<GasMapComponent>();
        }

    }



}
