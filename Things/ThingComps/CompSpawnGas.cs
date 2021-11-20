using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class CompSpawnGas : ThingComp
    {
        private GasDef def;
        private float amountOfGas;
        private int interval;
        GasGrid grid;
        private int nextTick;
        private CompProperties_SpawnGas Props => (CompProperties_SpawnGas)props;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            def = Props.gasDef;
            amountOfGas = Props.amountOfGas;
            interval = Props.interval;
            nextTick = Find.TickManager.TicksGame;
            List<GasGrid> grids = parent.Map.GetComponent<GasMapComponent>().grid.gasGrids;
            grid = grids.First(x => x.def == def);
        }

        public override void CompTick()
        {
            
            if (Find.TickManager.TicksGame >= nextTick)
            {
                nextTick = Find.TickManager.TicksGame + interval;
                SpawnGas();
            }
        }

        public void SpawnGas()
        {

            CompRefuelable compRefuelable = parent.TryGetComp<CompRefuelable>();
            CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
            if ((compRefuelable?.HasFuel ?? true) & (compPowerTrader?.PowerOn ?? true))
            {
                int ind = grid.CellToIndex(parent.Position);
                grid.gasGrid[ind] += amountOfGas;
                grid.gases.Add(ind);
            }

        }
    }
}
