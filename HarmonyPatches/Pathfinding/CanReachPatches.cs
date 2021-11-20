using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace GasExpansion
{
    public class ReachabilityUtilityCanReachPrefix
    {
        public static readonly MethodInfo original = AccessTools.Method(typeof(ReachabilityUtility), "CanReach");
        public static bool Prefix(ref bool __result, Pawn pawn, LocalTargetInfo dest, Danger maxDanger)
        {
            if (maxDanger == Danger.Deadly)
            {
                return true;
            }
            if (!pawn.Spawned)
            {
                __result = false;
                return false;
            }
            if (!pawn.Map.GetComponent<GasMapComponent>().grid.pathTracker.ShouldPathThrough(dest.Cell, pawn))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
    public class ReachabilityCanReachPrefix
    {
        public static readonly MethodInfo original = AccessTools.Method(typeof(Reachability), "CanReach", new Type[] { typeof(IntVec3), typeof(LocalTargetInfo), typeof(PathEndMode), typeof(TraverseMode), typeof(Danger) });

        public static bool Prefix(ref bool __result, LocalTargetInfo dest, Danger maxDanger, Map ___map)
        {
            if (maxDanger == Danger.Deadly)
            {
                return true;
            }
            if (!___map.GetComponent<GasMapComponent>().grid.pathTracker.IsDangerCell(dest.Cell))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
    public class CanWanderToCellPostfix
    {
        public static readonly MethodInfo original = AccessTools.Method(typeof(RCellFinder), "CanWanderToCell");
        public static void Postfix(ref bool __result, IntVec3 c, Danger maxDanger, Pawn pawn)
        {
            if (!__result || maxDanger == Danger.Deadly)
            {
                return;
            }
            if (!pawn.Spawned)
            {
                __result = false;
                return;
            }
            if (!pawn.Map.GetComponent<GasMapComponent>().grid.pathTracker.ShouldPathThrough(c, pawn))
            {
                __result = false;
                return;
            }
        }

    }
}
