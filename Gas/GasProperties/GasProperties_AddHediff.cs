using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class GasProperties_AddHediff : CompProperties
    {
        public HediffDef hediff;
        public float severityDivider;
        public float severityCap;
        public bool affectsMechanoids;
    }
}
