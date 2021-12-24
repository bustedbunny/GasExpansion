using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Verse;
using static UnityEngine.UI.Button;

namespace GasExpansion
{
    public class GasExpansionMod : Mod
    {
        public GasExpansionMod(ModContentPack content) : base(content)
        {
            mod = this;
            this.settings = GetSettings<ModSettings>();
        }
        public static GasExpansionMod mod;
        public ModSettings settings;
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

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new();
            listing.Begin(inRect);
            listing.CheckboxLabeled("Enable debug drawing for gas density", ref ModSettings.DebugDrawing);
            listing.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Gas Expansion";
        }
    }

    public class ModSettings : Verse.ModSettings
    {
        public static bool DebugDrawing;
    }
}

