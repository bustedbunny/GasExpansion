using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasExpansion
{
    [HarmonyPatch(typeof(Fire), "SpawnSmokeParticles")]
    public class SpawnSmokeParticlesPostfix
    {
        public static bool Prefix(Fire __instance, int ___ticksUntilSmoke, IntRange ___SmokeIntervalRange)
        {
            GasMapComponent comp = __instance.Map.GetComponent<GasMapComponent>();
            if (comp != null)
            {
                comp.grid.gasGrids.First(x => x.def == DefOfClass.GasExpansion_SmokeBlack).CreateGas(__instance.Position, __instance.fireSize/9);
                float num = __instance.fireSize / 2f;
                if (num > 1f)
                {
                    num = 1f;
                }
                num = 1f - num;
                ___ticksUntilSmoke = ___SmokeIntervalRange.Lerped(num) + (int)(10f * Rand.Value);
                return false;
            }
            return true;
        }
    }
}
