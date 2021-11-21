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
            grid = new GasGridTracker
            {
                map = map,
                parent = this
            };
            grid.PreLoadInit();
        }
        public WeatherTracker weather;
        public GasGridTracker grid;
        
        private GasDrawer drawer;


        private bool _throttled = false;
        private bool _rare = false;
        private bool _long = false;
        public override async void MapComponentTick()
        {
            int tick = Find.TickManager.TicksGame;
            await Task.Run(() =>
            {
                foreach (GasGrid gasGrid in grid.gasGrids)
                {
                    gasGrid.UpdateTransparency();
                }
            });

            _throttled = (tick % 15 == 0);
            if (_throttled)
            {
                grid.totalGasCount = 0;
                drawer.bufferIsDirty = true;
            }
            _rare = (tick % 250 == 0);
            _long = (tick % 2000 == 0);
            if (_rare)
            {
                grid.pathTracker.CachePathGrid();
                weather.UpdateTracker();

            }
            foreach (GasGrid gasGrid in grid.gasGrids)
            {
                if (_throttled)
                {
                    gasGrid.TickThrottled();
                    grid.totalGasCount += gasGrid.gases.Count;

                }
                if (_rare)
                {
                    gasGrid.TickRare();

                    if (_long)
                    {
                        gasGrid.TickLong();
                    }
                }
            }
        }

        
        public override void MapComponentUpdate()
        {
            drawer.Draw();
        }


        /*
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
        */

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
            weather = new WeatherTracker(this);
            grid.FinalizeInit();
            drawer = new GasDrawer(grid, map);
        }
        public override void MapRemoved()
        {
            drawer.ReleaseAllBuffers();
        }
        public override void ExposeData()
        {
            Scribe_Deep.Look(ref grid, "gasGrids");
            Scribe_Deep.Look(ref weather, "weatherTracker");
        }
    }
}
