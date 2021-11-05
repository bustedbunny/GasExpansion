using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{


    [HarmonyPatch(typeof(Map), "FinalizeLoading")]
    public class FinalizeLoadingPrefix
    {
        public static void Prefix(Map __instance)
        {
            GasMapComponent comp = __instance.GetComponent<GasMapComponent>();
            if (comp != null)
            {
                comp.PreLoadInit();
            }
        }
    }
}
