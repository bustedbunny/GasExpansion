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
    public class CompExplosive_Gas : CompExplosive
    {


        private int countdownTicksLeft = -1;

        public new CompProperties_Explosive_Gas Props => (CompProperties_Explosive_Gas)props;


        private bool CanEverExplodeFromDamage
        {
            get
            {
                if (Props.chanceNeverExplodeFromDamage < 1E-05f)
                {
                    return true;
                }
                Rand.PushState();
                Rand.Seed = parent.thingIDNumber.GetHashCode();
                bool result = Rand.Value > Props.chanceNeverExplodeFromDamage;
                Rand.PopState();
                return result;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref wickStarted, "wickStarted", defaultValue: false);
            Scribe_Values.Look(ref wickTicksLeft, "wickTicksLeft", 0);
            Scribe_Values.Look(ref destroyedThroughDetonation, "destroyedThroughDetonation", defaultValue: false);
            Scribe_Values.Look(ref countdownTicksLeft, "countdownTicksLeft", 0);
            Scribe_Values.Look(ref customExplosiveRadius, "explosiveRadius");
        }

        public override void CompTick()
        {
            if (countdownTicksLeft > 0)
            {
                countdownTicksLeft--;
                if (countdownTicksLeft == 0)
                {
                    StartWick();
                    countdownTicksLeft = -1;
                }
            }
            if (!wickStarted)
            {
                return;
            }
            if (wickSoundSustainer == null)
            {
                StartWickSustainer();
            }
            else
            {
                wickSoundSustainer.Maintain();
            }
            if (Props.wickMessages != null)
            {
                foreach (WickMessage wickMessage in Props.wickMessages)
                {
                    if (wickMessage.ticksLeft == wickTicksLeft && wickMessage.wickMessagekey != null)
                    {
                        Messages.Message(wickMessage.wickMessagekey.Translate(parent, wickTicksLeft.ToStringSecondsFromTicks()), parent, wickMessage.messageType ?? MessageTypeDefOf.NeutralEvent, historical: false);
                    }
                }
            }
            wickTicksLeft--;
            if (wickTicksLeft <= 0)
            {
                Detonate(parent.MapHeld);
            }
        }

        private void StartWickSustainer()
        {
            SoundDefOf.MetalHitImportant.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
            SoundInfo info = SoundInfo.InMap(parent, MaintenanceType.PerTick);
            wickSoundSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
        }

        private void EndWickSustainer()
        {
            if (wickSoundSustainer != null)
            {
                wickSoundSustainer.End();
                wickSoundSustainer = null;
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (mode == DestroyMode.KillFinalize && Props.explodeOnKilled)
            {
                Detonate(previousMap, ignoreUnspawned: true);
            }
        }
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (!CanEverExplodeFromDamage)
            {
                return;
            }
            if (dinfo.Def.ExternalViolenceFor(parent) && dinfo.Amount >= (float)parent.HitPoints && CanExplodeFromDamageType(dinfo.Def))
            {
                if (parent.MapHeld != null)
                {
                    Detonate(parent.MapHeld);
                    if (parent.Destroyed)
                    {
                        absorbed = true;
                    }
                }
            }
            else if (!wickStarted && Props.startWickOnDamageTaken != null && Props.startWickOnDamageTaken.Contains(dinfo.Def))
            {
                StartWick(dinfo.Instigator);
            }
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (CanEverExplodeFromDamage && CanExplodeFromDamageType(dinfo.Def) && !parent.Destroyed)
            {
                if (wickStarted && dinfo.Def == DamageDefOf.Stun)
                {
                    StopWick();
                }
                else if (!wickStarted && parent.HitPoints <= StartWickThreshold && dinfo.Def.ExternalViolenceFor(parent))
                {
                    StartWick(dinfo.Instigator);
                }
            }
        }

        public new float ExplosiveRadius()
        {
            CompProperties_Explosive_Gas compProperties_Explosive = Props;
            float num = customExplosiveRadius ?? Props.explosiveRadius;
            if (parent.stackCount > 1 && compProperties_Explosive.explosiveExpandPerStackcount > 0f)
            {
                num += Mathf.Sqrt((float)(parent.stackCount - 1) * compProperties_Explosive.explosiveExpandPerStackcount);
            }
            if (compProperties_Explosive.explosiveExpandPerFuel > 0f && parent.GetComp<CompRefuelable>() != null)
            {
                num += Mathf.Sqrt(parent.GetComp<CompRefuelable>().Fuel * compProperties_Explosive.explosiveExpandPerFuel);
            }
            return num;
        }

        protected new void Detonate(Map map, bool ignoreUnspawned = false)
        {
            if (!ignoreUnspawned && !parent.SpawnedOrAnyParentSpawned)
            {
                return;
            }
            CompProperties_Explosive_Gas compProperties_Explosive = Props;
            float num = ExplosiveRadius();
            if (compProperties_Explosive.explosiveExpandPerFuel > 0f && parent.GetComp<CompRefuelable>() != null)
            {
                parent.GetComp<CompRefuelable>().ConsumeFuel(parent.GetComp<CompRefuelable>().Fuel);
            }
            int stack = parent.stackCount;
            if (compProperties_Explosive.destroyThingOnExplosionSize <= num && !parent.Destroyed)
            {
                destroyedThroughDetonation = true;
                parent.Kill();
            }
            EndWickSustainer();
            wickStarted = false;
            if (map == null)
            {
                Log.Warning("Tried to detonate CompExplosive_Gas in a null map.");
                return;
            }
            if (compProperties_Explosive.explosionEffect != null)
            {
                Effecter effecter = compProperties_Explosive.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(parent.PositionHeld, map), new TargetInfo(parent.PositionHeld, map));
                effecter.Cleanup();
            }
            GasGrid grid = map.GetComponent<GasMapComponent>().grid.gasGrids.Find(x => x.def == Props.gasDef);
            if (grid == null)
            {
                Log.Error("Couldn't find Gas with gasDef defname.");
                return;
            }
            Log.Message(Mathf.Atan(stack / 10).ToString());
            FleckMaker.Static(parent.Position, map, FleckDefOf.ExplosionFlash, Mathf.Atan(stack * 0.1f) * 10);
            FleckMaker.ThrowDustPuffThick(parent.Position.ToVector3Shifted(), map, Mathf.Atan(stack * 0.4f) * 10f, Color.grey);
            DefOfClass.Explosion_Smoke.PlayOneShot(new TargetInfo(parent.Position, map));
            grid.CreateGas(parent.Position, Mathf.Pow(ExplosiveRadius(), 2) * 500f * stack);
        }

        private bool CanExplodeFromDamageType(DamageDef damage)
        {
            if (Props.requiredDamageTypeToExplode != null)
            {
                return Props.requiredDamageTypeToExplode == damage;
            }
            return true;
        }
    }
}
