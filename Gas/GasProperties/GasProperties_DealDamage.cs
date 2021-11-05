using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class GasProperties_DealDamage : CompProperties
    {
        public DamageDef gasDamageType;
        public float damageDivider;
        public float maxGasDensity;
        public bool damageAdjacentWalls;
        public StatDef armorStat;



    }
}
