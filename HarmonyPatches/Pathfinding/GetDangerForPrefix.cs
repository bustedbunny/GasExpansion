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
    public class GetDangerForPrefix
    {
        public static readonly MethodInfo original = AccessTools.Method(typeof(DangerUtility), "GetDangerFor");

        public static bool Prefix(ref Danger __result, IntVec3 c, Map map)
        {
            if (map.GetComponent<GasMapComponent>().IsDangerCell(c))
            {
                __result = Danger.Deadly;
                return false;
            }
            return true;
        }
    }
    
}
