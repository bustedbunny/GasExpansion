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
        private static Dictionary<string, ComputeShader> lookupComputeShades;
        //   private static Dictionary<string, Shader> lookupShades;
        public static readonly ComputeShader GasGridCompute = LoadComputeShader("gasspreadcompute");

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
        public static ComputeShader LoadComputeShader(string shaderName)
        {
            if (lookupComputeShades == null)
            {
                lookupComputeShades = new Dictionary<string, ComputeShader>();
            }
            if (!lookupComputeShades.ContainsKey(shaderName))
            {
                lookupComputeShades.Add(shaderName, GasExpansionBundle.LoadAsset<ComputeShader>(shaderName));
            }
            ComputeShader computeShader = lookupComputeShades.TryGetValue(shaderName);
            if (computeShader == null)
            {
                Log.Warning("Could not load shader '" + shaderName + "'");
                return null;
            }
            return computeShader;
        }
    }
}
