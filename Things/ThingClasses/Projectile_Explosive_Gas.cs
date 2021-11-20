using CombatExtended;
using ProjectileImpactFX;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace GasExpansion
{
    public class Projectile_Explosive_Gas : Projectile_Explosive
    {
        private int ticksToDetonation;
        private bool exploded = false;

        public override void Tick()
        {
            base.Tick();
            if (!exploded)
            {
                if (ticksToDetonation > 0)
                {
                    ticksToDetonation--;
                    if (ticksToDetonation <= 0)
                    {
                        Explode();
                    }
                }
            }
            else
            {
                if (ticksToDetonation < 0)
                {
                    Destroy();
                }
                ticksToDetonation--;
            }

        }
        protected override void Impact(Thing hitThing)
        {
            if (def.projectile.explosionDelay == 0)
            {
                Explode();
                return;
            }
            landed = true;
            ticksToDetonation = def.projectile.explosionDelay;
            GenExplosion.NotifyNearbyPawnsOfDangerousExplosive(this, def.projectile.damageDef, launcher.Faction, launcher);
        }

        private new void Explode()
        {
            if (def.projectile.explosionEffect != null)
            {
                Effecter effecter = def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(base.Position, Map), new TargetInfo(base.Position, Map));
                effecter.Cleanup();
            }
            GasDefModExtension extension = def.GetModExtension<GasDefModExtension>();
            GasGrid grid = this.Map.GetComponent<GasMapComponent>().grid.gasGrids.Find(x => x.def == extension.gasDef);
            if (grid == null)
            {
                Log.Error("GasDefModExtension is not valid. Couldn't find GasGrid. GasDef must be defined in GasDefModExtension.");
                return;
            }
            FleckMaker.Static(Position, Map, FleckDefOf.ExplosionFlash);
            FleckMaker.ThrowDustPuffThick(Position.ToVector3Shifted(), Map, 4f, Color.grey);
            DefOfClass.GasExpansion_GasHissing.PlayOneShot(new TargetInfo(Position, Map));
            grid.CreateGas(Position, Mathf.Pow(def.projectile.explosionRadius, 2) * extension.densityMultiplier);
            exploded = true;
            ticksToDetonation = 420;
        }
    }
}
