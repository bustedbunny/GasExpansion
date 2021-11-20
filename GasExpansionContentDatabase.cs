using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GasExpansion
{
    [StaticConstructorOnStartup]
    public static class GasExpansionContentDatabase
    {
        private static AssetBundle bundleInt;
        //   private static Dictionary<string, Shader> lookupShades;
        public static readonly Shader InstancedColor = GasExpansionBundle.LoadAsset<Shader>("InstancedColor");

        public static AssetBundle GasExpansionBundle
        {
            get
            {
                if (bundleInt == null)
                {
                    bundleInt = GasExpansionMod.mod.MainBundle;
                }
                return bundleInt;
            }
        }
    }
}
