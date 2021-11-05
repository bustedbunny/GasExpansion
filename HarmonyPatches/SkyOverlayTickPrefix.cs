using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using HotSwap;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasExpansion
{
    [HarmonyPatch(typeof(SkyOverlay), "TickOverlay")]
#if DEBUG
    [HotSwappable]
#endif
    internal class SkyOverlayTickPrefix
    {
        internal static bool Prefix(Map map, SkyOverlay __instance)
        {
            if (__instance is not WeatherOverlay_Fog)
            {
                return true;
            }
            GasMapComponent comp = map.GetComponent<GasMapComponent>();

            Material mat = __instance.worldOverlayMat;
            if (mat != null)
            {
                float windSpeed = map.windManager.WindSpeed / 1f;
                windSpeed = Math.Max(0.001f, windSpeed);
                Vector2 angle = new(-Mathf.Cos(comp.windDirectionFast - Mathf.PI / 2), Mathf.Sin(comp.windDirectionFast - Mathf.PI/2));
                angle.Normalize();
                angle = angle * (Find.TickManager.TicksGame % 360000) / 100000f * windSpeed;
                comp.winddir += angle / 1000f;
                mat.SetTextureOffset("_MainTex", comp.winddir);
                if (mat.HasProperty("_MainTex2"))
                {
                    mat.SetTextureOffset("_MainTex2", comp.winddir);
                }
            }
            return false;

        }
    }
}
