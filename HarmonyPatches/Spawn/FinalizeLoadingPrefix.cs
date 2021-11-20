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
    // This patch is only required for loading games to load all required references, that initially get assigned in class constructor.
    // In short it allows to keep less data in save file.

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
