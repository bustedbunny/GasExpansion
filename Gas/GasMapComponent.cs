using HotSwap;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Verse;
using System.Diagnostics;
using GasExpansion.Gas.GasTrackers;

namespace GasExpansion
{
#if DEBUG
    [HotSwappable]
#endif
    public class GasMapComponent : MapComponent
    {
        public GasMapComponent(Map map)
            : base(map)
        {
            grid = new GasGridTracker();
            grid.map = map;
            grid.parent = this;
            grid.PreLoadInit();
        }
        public WeatherTracker weather;
        public GasGridTracker grid = new GasGridTracker();
        public override void MapComponentTick()
        {
            int tick = Find.TickManager.TicksGame;
            if (tick % 15 == 0)
            {
                grid.totalGasCount = 0;
            }
            Parallel.ForEach(grid.gasGrids, gasGrid =>
            {
                gasGrid.Tick();
                gasGrid.UpdateTransparency();
                if (tick % 15 == 0)
                {
                    gasGrid.TickThreaded();
                    lock (grid.gasCountLock)
                    {
                        grid.totalGasCount += gasGrid.gases.Count;
                    }

                }
            });
            if (tick % 250 == 0)
            {
                for (int i = 0; i < grid.gasGrids.Count; i++)
                {
                    grid.gasGrids[i].TickRare();
                }
                grid.pathTracker.CachePathGrid();
                weather.UpdateTracker();
                if (tick % 2000 == 0)
                {
                    for (int i = 0; i < grid.gasGrids.Count; i++)
                    {
                        grid.gasGrids[i].TickLong();
                    }
                }
            }
        }

        GasDrawer drawer;
        public override void MapComponentUpdate()
        {
            drawer.Draw();
        }

        /*
        grid.gasGrids[i].Draw(currentViewRect, material, angle, minGas);
        */


        public override void MapComponentOnGUI()
        {
            if (!Prefs.DevMode)
            {
                return;
            }
            CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
            currentViewRect.ClipInsideMap(map);
            currentViewRect.ExpandedBy(1);
            foreach (GasGrid grid in grid.gasGrids)
            {
                foreach (int i in grid.gases)
                {
                    IntVec3 pos = grid.IndexToCell(i);
                    if (!currentViewRect.Contains(pos))
                    {
                        continue;
                    }
                    GenMapUI.DrawText(new Vector2(pos.x + 0.5f, pos.z + 0.5f), $"{(int)grid.gasGrid[i]}", Color.red);
                }
            }
        }


        public bool CanMoveTo(int ind)
        {
            Building edifice = map.edificeGrid[ind];
            if (edifice == null || edifice.def.Fillage != FillCategory.Full)
            {
                return true;
            }
            if (edifice is Building_Door door && door.Open)
            {
                return true;
            }
            if (edifice is Building_Vent vent && (vent?.TryGetComp<CompFlickable>().SwitchIsOn ?? false))
            {
                return true;
            }
            return false;
        }
        public void PreLoadInit()
        {
            grid.map = map;
            grid.parent = this;
            grid.PreLoadInit();
        }
        public override void FinalizeInit()
        {
            weather = new WeatherTracker();
            weather.parent = this;
            grid.FinalizeInit();
            grid.pathTracker.CachePathGrid();
            drawer = new GasDrawer();
            drawer.grid = grid;
            drawer.map = map;
            drawer.Initialize();
        }
        public override void ExposeData()
        {
            Scribe_Deep.Look(ref grid, "gasGrids");
            Scribe_Deep.Look(ref weather, "weatherTracker");
        }
    }
}
