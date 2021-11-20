using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static Verse.DamageWorker;

namespace GasExpansion
{
    public class GasGrid_DealDamage : GasGrid
    {

        public DamageDef gasDamageType;
        public float damageDivider = 51.2f;
        public float maxGasDensity = 256f;
        public bool damageAdjacentWalls = true;
        public StatDef armorStat;
        public override void Initialize()
        {
            base.Initialize();
            try
            {
                GasProperties_DealDamage props = def.props.First(x => x is GasProperties_DealDamage) as GasProperties_DealDamage;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get GasProperties_DealDamage fields: " + ex);
                map.GetComponent<GasMapComponent>().grid.gasGrids.Remove(this);
            }

        }
        public override void PostLoad()
        {
            base.PostLoad();
            try
            {
                GasProperties_DealDamage props = def.props.First(x => x is GasProperties_DealDamage) as GasProperties_DealDamage;
                damageDivider = props.damageDivider;
                maxGasDensity = props.maxGasDensity;
                gasDamageType = props.gasDamageType;
                damageAdjacentWalls = props.damageAdjacentWalls;
                armorStat = props.armorStat;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get GasProperties_DealDamage fields: " + ex);
                map.GetComponent<GasMapComponent>().grid.gasGrids.Remove(this);
            }
        }
        private int[] cache = new int[0];
        private int iterations = 0;
        private int curIteration = 0;
        private int curStopInd = 0;
        public override void TickThreaded()
        {
            base.TickThreaded();
            for (int i = curIteration; i < curStopInd; i++)
            {
                Damage(cache[i]);
                curIteration++;
            }
            curStopInd += (int)Math.Ceiling(iterations / 16f);
            curStopInd = Math.Min(curStopInd, iterations - 1);
        }
        public override void TickRare()
        {
            iterations = gases.Count;
            cache = new int[iterations];
            gases.CopyTo(cache, 0);
            curStopInd = (int)Math.Ceiling(iterations / 16f);
            curIteration = 0;
        }

        public void Damage(int ind)
        {

            if (gasGrid[ind] < 30)
            {

                return;
            }
            float num = (Math.Min(gasGrid[ind], maxGasDensity) / damageDivider);
            lock (map.thingGrid.ThingsListAtFast(ind))
            {
                List<Thing> things = map.thingGrid.ThingsListAtFast(ind);
                if (things.Count > 0)
                {
                    for (int i = things.Count - 1; i >= 0; i--)
                    {
                        if (things[i].Map == null || things[i].Position == null)
                        {
                            continue;
                        }
                        if (things[i] is Pawn pawn)
                        {
                            DamageInfo dinfo = new(gasDamageType, num);
                            dinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                            if (pawn.RaceProps.IsMechanoid)
                            {
                                dinfo.SetIgnoreArmor(true);
                                things[i].TakeDamage(dinfo);
                                continue;
                            }
                            float armor = 0;
                            if (pawn.apparel != null)
                            {
                                foreach (Thing apparel in pawn.apparel.WornApparel)
                                {
                                    armor += apparel.GetStatValue(armorStat);
                                }

                                if (armor > 1.4f)
                                {
                                    if (pawn.apparel.WornApparel.TryRandomElement(out var result))
                                    {
                                        result.TakeDamage(dinfo);
                                    }
                                    continue;
                                }
                            }
                            DamageResult dmgResult = things[i].TakeDamage(dinfo);
                            if (dmgResult.totalDamageDealt > 1f && !pawn.Downed &&
                                (pawn.CurJob == null || pawn.CurJob.def.checkOverrideOnDamage == CheckJobOverrideOnDamageMode.Never ||
                                (!pawn.CurJob.playerForced && !pawn.Drafted && pawn.CurJobDef != JobDefOf.Flee)) && !pawn.Dead &&
                                RCellFinder.TryFindDirectFleeDestination(pawn.Position, 9f, pawn, out var newPos))
                            {
                                Job job = JobMaker.MakeJob(JobDefOf.Goto, newPos);
                                job.locomotionUrgency = LocomotionUrgency.Sprint;
                                pawn.jobs.StartJob(job, JobCondition.InterruptForced);
                            }
                        }
                        else
                        {
                            things[i].TakeDamage(new DamageInfo(DamageDefOf.Burn, num));
                        }
                    }
                }
            }
            if (!damageAdjacentWalls)
            {
                return;
            }
            for (int i = 0; i < 4; i++)
            {
                int ind2;
                switch (i)
                {
                    case 0:
                        if (ind % map.Size.x == map.Size.x - 1)
                        {
                            continue;
                        }
                        ind2 = ind + 1;
                        break;
                    case 1:
                        if (ind % map.Size.x == 0)
                        {
                            continue;
                        }
                        ind2 = ind - 1;
                        break;
                    case 2:
                        ind2 = ind + map.Size.x;
                        if (ind2 > gasGrid.Length - 1)
                        {
                            continue;
                        }
                        break;
                    default:
                        ind2 = ind - map.Size.x;
                        if (ind2 < 0)
                        {
                            continue;
                        }
                        break;
                }
                if (gasGrid[ind2] > 0) continue;

                lock ((object)ind2)
                {
                    Building edifice = map.edificeGrid[ind2];

                    if (edifice != null && edifice.def.Fillage == FillCategory.Full)
                    {
                        edifice.TakeDamage(new DamageInfo(DamageDefOf.Burn, num * 3));
                    }
                }




            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            //   Scribe_Values.Look(ref tickToAdjacent, "tickToAdjacent");
        }
    }
}
