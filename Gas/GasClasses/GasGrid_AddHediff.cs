using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace GasExpansion
{
    public class GasGrid_AddHediff : GasGrid
    {
        public HediffDef hediff;
        public float severityDivider;
        public float severityCap;
        public bool affectsMechanoids;

        public override void Initialize()
        {
            base.Initialize();
            try
            {
                GasProperties_AddHediff props = def.props.First(x => x is GasProperties_AddHediff) as GasProperties_AddHediff;
                hediff = props.hediff;
                severityDivider = props.severityDivider;
                severityCap = props.severityCap;
                affectsMechanoids = props.affectsMechanoids;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get GasProperties_AddHediff fields: " + ex);
                map.GetComponent<GasMapComponent>().grid.gasGrids.Remove(this);
            }

        }

        public override void PostLoad()
        {
            base.PostLoad();
            try
            {
                GasProperties_AddHediff props = def.props.First(x => x is GasProperties_AddHediff) as GasProperties_AddHediff;
                hediff = props.hediff;
                severityDivider = props.severityDivider;
                severityCap = props.severityCap;
            }
            catch (Exception ex)
            {
                Log.Error("Failed to get GasProperties_AddHediff fields: " + ex);
                map.GetComponent<GasMapComponent>().grid.gasGrids.Remove(this);
            }
        }

        public override void TickRare()
        {
            foreach (int ind in gases)
            {

                if (gasGrid[ind] < 30)
                {
                    continue;
                }
                lock (map.thingGrid.ThingsListAtFast(ind))
                {
                    List<Thing> pawns = map.thingGrid.ThingsListAtFast(ind);
                    for (int i = 0; i < pawns.Count; i++)
                    {
                        if (pawns[i] is Pawn pawn)
                        {
                            if (pawn.RaceProps.IsMechanoid != affectsMechanoids)
                            {
                                continue;
                            }
                            if (pawn.health == null)
                            {
                                continue;
                            }

                            float num = pawn.GetStatValue(StatDefOf.ToxicSensitivity);

                            if (num > 1f)
                            {
                                continue;
                            }
                            num = 0;
                            if (pawn.apparel != null)
                            {
                                foreach (Apparel apparel in pawn.apparel.WornApparel)
                                {
                                    if (apparel.def.apparel.bodyPartGroups.Contains(DefOfClass.Mouth))
                                    {
                                        num += apparel.GetStatValue(StatDefOf.ToxicSensitivity);
                                    }
                                }
                                if (num > 1f)
                                {
                                    continue;
                                }
                            }
                            num = Math.Min(gasGrid[ind] / severityDivider, severityCap) * Math.Max(1 - num, 0);
                            HealthUtility.AdjustSeverity(pawn, hediff, num);
                            if (num > severityCap / 2 && !pawn.Downed &&
                                (pawn.CurJob == null || pawn.CurJob.def.checkOverrideOnDamage == CheckJobOverrideOnDamageMode.Never ||
                                (!pawn.CurJob.playerForced && !pawn.Drafted && pawn.CurJobDef != JobDefOf.Flee)) && !pawn.Dead &&
                                RCellFinder.TryFindDirectFleeDestination(pawn.Position, 9f, pawn, out var newPos))
                            {
                                Job job = JobMaker.MakeJob(JobDefOf.Goto, newPos);
                                job.locomotionUrgency = LocomotionUrgency.Sprint;
                                pawn.jobs.StartJob(job, JobCondition.InterruptForced);
                            }
                        }
                    }
                }
                
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }
}
