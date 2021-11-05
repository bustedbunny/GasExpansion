using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class CompProperties_Explosive_Gas : CompProperties_Explosive
    {
        public GasDef gasDef;
        public float densityMultiplier;

        public CompProperties_Explosive_Gas()
        {
            compClass = typeof(CompExplosive_Gas);
        }

    }
}
