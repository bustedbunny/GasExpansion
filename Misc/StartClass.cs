using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using HarmonyLib;
using Verse.AI;

namespace GasExpansion
{
    [StaticConstructorOnStartup]
    public static class StartUp
    {
        static StartUp()
        {
            var harmony = new Harmony("GasExpansion.patch");
            harmony.PatchAll();

            if (true)
            {
                harmony.Patch(PathFinderTranspiler.original, transpiler: new HarmonyMethod(AccessTools.Method( typeof(PathFinderTranspiler),"Transpiler")));
                harmony.Patch(GetDangerForPrefix.original, prefix: new HarmonyMethod(AccessTools.Method(typeof(GetDangerForPrefix), "Prefix")));
                harmony.Patch(DangerForTranspiler.original, transpiler: new HarmonyMethod(AccessTools.Method(typeof(DangerForTranspiler), "Transpiler")));
                harmony.Patch(ReachabilityUtilityCanReachPrefix.original, prefix: new HarmonyMethod(AccessTools.Method(typeof(ReachabilityUtilityCanReachPrefix), "Prefix")));
                harmony.Patch(ReachabilityCanReachPrefix.original, prefix: new HarmonyMethod(AccessTools.Method(typeof(ReachabilityCanReachPrefix), "Prefix")));
                harmony.Patch(CanWanderToCellPostfix.original, postfix: new HarmonyMethod(AccessTools.Method(typeof(CanWanderToCellPostfix), "Postfix")));
            }
            if (!ModLister.HasActiveModWithName("Combat Extended"))
            {
                harmony.Patch(DoDatePostfix.original, postfix: new HarmonyMethod(AccessTools.Method(typeof(DoDatePostfix), "Postfix")));
            }
        }
    }
}
