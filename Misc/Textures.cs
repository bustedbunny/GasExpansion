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
    public static class Textures
    {
        public static readonly Texture2D tex = ContentFinder<Texture2D>.Get("GasTexture");
    }
}
