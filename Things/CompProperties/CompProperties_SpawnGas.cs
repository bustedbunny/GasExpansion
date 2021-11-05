using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class CompProperties_SpawnGas : CompProperties
    {
        public GasDef gasDef;
        public float amountOfGas;
        public int interval;

        public CompProperties_SpawnGas()
        {
            compClass = typeof(CompSpawnGas);
        }

    }
}
