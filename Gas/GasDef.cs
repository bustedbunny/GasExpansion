using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GasExpansion
{
    public class GasDef : Def
    {
        public bool needRoof = true;
        public float unroofedDiffusion = 1;
        public float constDiffusion = 5;
        public float spreadSpeed = 15;
        public bool blockVision;
        public bool isBad;
        public bool isToxic;
        public bool isCorrosive;


        public DamageDef damageDef;
        public float damageDivider;
        public bool isExplosive;


        public Color color;
        public Type gasClass = typeof(GasGrid);
        public List<CompProperties> props = new();


    }

}
