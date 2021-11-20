using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace GasExpansion
{
    internal class DoDatePostfix
    {
        internal static readonly MethodInfo original = AccessTools.Method(typeof(GlobalControlsUtility), "DoDate");
        internal static void Postfix(ref float curBaseY)
        {
            float num = (float)UI.screenWidth - 200f;
            Find.CurrentMap?.GetComponent<GasMapComponent>().weather.DoWindGUI(num + 8f, ref curBaseY);
        }
    }


}
