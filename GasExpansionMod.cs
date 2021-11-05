using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GasExpansion
{
    public class GasExpansionMod : Mod
    {
        public GasExpansionMod(ModContentPack content) : base(content)
        {
            mod = this;
        }
        public static GasExpansionMod mod;
        public AssetBundle MainBundle
        {
            get
            {
                if (Content?.assetBundles?.loadedAssetBundles[0] != null)
                {
                    return Content.assetBundles.loadedAssetBundles[0];
                }
                return null;
            }
        }
    }
}

