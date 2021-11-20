using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GasExpansion
{
    
  //  [HarmonyPatch(typeof(Verb_SmokePop), "Pop")]
    public class Verb_SmokePopPopPrefix
    {
        public static bool Prefix(CompReloadable comp)
        {
            if (comp != null && comp.CanBeUsed)
            {
                ThingWithComps parent = comp.parent;
                Pawn wearer = comp.Wearer;
                wearer.Map.GetComponent<GasMapComponent>().grid.gasGrids.Find(x => x.def == DefOfClass.GasExpansion_ConcealingSmoke).CreateGas(wearer.Position, parent.GetStatValue(StatDefOf.SmokepopBeltRadius) * 1250f);
            //    GenExplosion.DoExplosion(wearer.Position, wearer.Map, parent.GetStatValue(StatDefOf.SmokepopBeltRadius), DamageDefOf.Smoke, null, -1, -1f, null, null, null, null, ThingDefOf.Gas_Smoke, 1f);
                comp.UsedOnce();
            }
            return false;
        }
    }
    
}
