using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion.Gas.GasTrackers
{
    public class GasGridTracker : IExposable
    {
        public List<GasGrid> gasGrids;
        public List<GasGrid> badGrids;
        public List<GasGrid> concealingGrids;
        public List<GasGrid> explosiveGrids;
        public bool[] toxicGrid;
        public bool[] corrosiveGrid;

        public int totalGasCount = 0;

        public Map map;
        public GasMapComponent parent;

        public GasPathTracker pathTracker;

        public GasGridTracker()
        {
            gasGrids = new List<GasGrid>();
            badGrids = new List<GasGrid>();
            concealingGrids = new List<GasGrid>();
            explosiveGrids = new List<GasGrid>();
        }

        public void PreLoadInit()
        {
            pathTracker = new GasPathTracker
            {
                grid = this,
                map = map
            };
            toxicGrid = new bool[map.cellIndices.NumGridCells];
            corrosiveGrid = new bool[map.cellIndices.NumGridCells];
            for (int i = 0; i < gasGrids.Count; i++)
            {
                gasGrids[i].parent = parent;
            }
        }
        public void FinalizeInit()
        {
            for (int i = gasGrids.Count - 1; i >= 0; i--)
            {
                if (gasGrids[i].def == null)
                {
                    gasGrids.RemoveAt(i);
                }
            }
            foreach (GasDef def in DefDatabase<GasDef>.AllDefs)
            {
                if (!gasGrids.Any(g => g.def == def))
                {
                    GasGrid newGrid = null;
                    try
                    {
                        newGrid = (GasGrid)Activator.CreateInstance(def.gasClass);
                        newGrid.def = def;
                        newGrid.map = map;
                        newGrid.Initialize();
                        gasGrids.Add(newGrid);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not instantiate or initialize a GasGrid: " + ex);
                        gasGrids.Remove(newGrid);
                    }

                }
            }
            for (int i = 0; i < gasGrids.Count; i++)
            {
                if (gasGrids[i].def.isBad)
                {
                    badGrids.Add(gasGrids[i]);
                }
                if (gasGrids[i].def.blockVision)
                {
                    concealingGrids.Add(gasGrids[i]);
                }
                if (gasGrids[i].def.isExplosive)
                {
                    explosiveGrids.Add(gasGrids[i]);
                }
                gasGrids[i].PostLoad();
                gasGrids[i].parent = parent;
                gasGrids[i].layer = i;
            }
            CountGases();
        }

        private void CountGases()
        {
            totalGasCount = 0;
            foreach(GasGrid gasGrid in gasGrids)
            {
                totalGasCount += gasGrid.gases.Count;
            }
        }
        public void ExposeData()
        {
            Scribe_Collections.Look(ref gasGrids, "gasGrids", LookMode.Deep);
        }
    }

    public class GasPathTracker
    {
        public Map map;
        public GasGridTracker grid;
        public bool IsDangerCell(IntVec3 cell)
        {
            int ind = map.cellIndices.CellToIndex(cell);
            if (grid.toxicGrid[ind])
            {
                return true;
            }
            if (grid.corrosiveGrid[ind])
            {
                return true;
            }
            return false;
        }
        public bool IsDangerCell(int ind)
        {
            if (grid.toxicGrid[ind])
            {
                return true;
            }
            if (grid.corrosiveGrid[ind])
            {
                return true;
            }
            return false;
        }

        public bool ShouldPathThrough(IntVec3 cell, GasExpansion_PawnResistance resistance)
        {
            int ind = map.cellIndices.CellToIndex(cell);
            if (!resistance.toxic && grid.toxicGrid[ind])
            {
                return false;
            }
            if (!resistance.corrosive && grid.corrosiveGrid[ind])
            {
                return false;
            }
            return true;
        }
        public bool ShouldPathThrough(IntVec3 cell, Pawn pawn)
        {
            int ind = map.cellIndices.CellToIndex(cell);
            if (!grid.toxicGrid[ind] && !grid.corrosiveGrid[ind])
            {
                return true;
            }
            GasExpansion_PawnResistance resistance = GetResistance(pawn);
            if (!resistance.toxic && grid.toxicGrid[ind])
            {
                return false;
            }
            if (!resistance.corrosive && grid.corrosiveGrid[ind])
            {
                return false;
            }
            return true;
        }

        public static GasExpansion_PawnResistance GetResistance(Pawn pawn)
        {
            GasExpansion_PawnResistance resistance = new();
            if (pawn?.apparel?.WornApparel != null)
            {
                bool skip1 = true;
                foreach (Thing apparel in pawn.apparel.WornApparel)
                {
                    if (skip1 && apparel.def.apparel.tags.Any(x => x == "GasMask"))
                    {
                        resistance.toxic = true;
                        break;
                    }
                }
                if (pawn.GetStatValue(StatDefOf.ArmorRating_Heat) >= 0.5f)
                {
                    resistance.corrosive = true;
                }
            }
            return resistance;
        }
        internal void CachePathGrid()
        {
            grid.toxicGrid = new bool[map.cellIndices.NumGridCells];
            grid.corrosiveGrid = new bool[map.cellIndices.NumGridCells];
            for (int i = 0; i < grid.badGrids.Count; i++)
            {
                if (grid.badGrids[i].def.isToxic)
                {
                    foreach (int ind in grid.badGrids[i].gases)
                    {
                        if (grid.badGrids[i].gasGrid[ind] > 30)
                        {
                            grid.toxicGrid[ind] = true;
                        }
                    }
                }
                else if (grid.badGrids[i].def.isCorrosive)
                {
                    foreach (int ind in grid.badGrids[i].gases)
                    {
                        if (grid.badGrids[i].gasGrid[ind] > 30)
                        {
                            grid.corrosiveGrid[ind] = true;
                        }
                    }
                }
                else if (grid.badGrids[i].def.isToxic && grid.badGrids[i].def.isCorrosive)
                {
                    foreach (int ind in grid.badGrids[i].gases)
                    {
                        if (grid.badGrids[i].gasGrid[ind] > 30)
                        {
                            grid.corrosiveGrid[ind] = true;
                            grid.toxicGrid[ind] = true;
                        }
                    }
                }
            }
        }
        public static float GasVisionInCell(IntVec3 cell, Map map)
        {
            GasMapComponent comp = map.GetComponent<GasMapComponent>();
            int ind = comp.grid.gasGrids[0].CellToIndex(cell);
            float num = 0;
            for (int i = 0; i < comp.grid.concealingGrids.Count; i++)
            {
                num += Math.Min(comp.grid.concealingGrids[i].DensityInCell(ind), 512) / 512f;
            }
            return num;
        }
    }
}
