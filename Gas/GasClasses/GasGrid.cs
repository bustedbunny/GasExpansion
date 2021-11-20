using GasExpansion.Gas.GasTrackers;
using HotSwap;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace GasExpansion
{
    [HotSwappable]
    public class GasGrid : IExposable
    {
        public GasDef def;
        public Map map;
        public GasMapComponent parent;
        public float[] gasGrid = new float[1];
        public float[] transparencyGrid;
        public HashSet<int> gases;
        public List<float> savedGases;
        public readonly HashSet<int> gasesToRemove = new();
        public readonly HashSet<int> gasesToAdd = new();
        internal int layer;

        public virtual void Initialize()
        {
            gasGrid = new float[map.cellIndices.NumGridCells];
            transparencyGrid = new float[map.cellIndices.NumGridCells];
            gases = new HashSet<int>();
        }

        public virtual void Tick()
        {
        }

        public virtual void TickThrottled()
        {
            if (def.needRoof)
            {
                foreach (int i in gases)
                {
                    if (!map.roofGrid.Roofed(i)) gasGrid[i] -= def.unroofedDiffusion / 1024f;
                    Spread(i);
                }
            }
            else
            {
                foreach (int i in gases)
                {
                    Spread(i);
                }
            }
            foreach (int i in gasesToRemove)
            {
                gases.Remove(i);
                transparencyGrid[i] = 0;
            }
            foreach (int i in gasesToAdd) gases.Add(i);
            gasesToRemove.Clear();
            gasesToAdd.Clear();
        }
        private void Spread(int i)
        {
            gasGrid[i] -= def.constDiffusion / 1024f;
            if (gasGrid[i] <= 1)
            {
                gasGrid[i] = 0;
                gasesToRemove.Add(i);
                return;
            }
            if (IsOutside(i, map))
            {
                SpreadOutside(i);
            }
            else
            {
                SpreadInside(i);
            }
        }
        private void SpreadInside(int ind)
        {
            float reserve = gasGrid[ind] / def.spreadSpeed;
            for (int i = 0; i < 4; i++)
            {
                int ind2;
                switch (i)
                {
                    case 0:
                        if (ind % map.Size.x == map.Size.x - 1)
                        {
                            gasGrid[ind] -= reserve;
                            continue;
                        }
                        ind2 = ind + 1;
                        break;
                    case 1:
                        if (ind % map.Size.x == 0)
                        {
                            gasGrid[ind] -= reserve;
                            continue;
                        }
                        ind2 = ind - 1;
                        break;
                    case 2:
                        ind2 = ind + map.Size.x;
                        if (ind2 > gasGrid.Length - 1)
                        {
                            gasGrid[ind] -= reserve;
                            continue;
                        }
                        break;
                    default:
                        ind2 = ind - map.Size.x;
                        if (ind2 < 0)
                        {
                            gasGrid[ind] -= reserve;
                            continue;
                        }
                        break;
                }
                if (gasGrid[ind2] < gasGrid[ind] && !CanMoveTo(ind2)) continue;

                gasGrid[ind2] += reserve;
                gasGrid[ind] -= reserve;
                gasesToAdd.Add(ind2);
            }
        }
        private void SpreadOutside(int ind)
        {
            float wallOffset = 1;
            for (int i = 0; i < 4; i++)
            {
                float reserve = gasGrid[ind] / def.spreadSpeed * parent.weather.windOffset[i] * wallOffset;
                int ind2;
                switch (i)
                {
                    case 0:
                        ind2 = ind + map.Size.x;
                        if (ind2 > gasGrid.Length - 1)
                        {
                            gasGrid[ind] -= reserve;
                            continue;
                        }
                        break;
                    case 1:
                        if (ind % map.Size.x == map.Size.x - 1)
                        {
                            gasGrid[ind] -= reserve;
                            continue;
                        }
                        ind2 = ind + 1;
                        break;
                    case 2:
                        ind2 = ind - map.Size.x;
                        if (ind2 < 0)
                        {
                            gasGrid[ind] -= reserve;
                            continue;
                        }
                        break;
                    default:
                        if (ind % map.Size.x == 0)
                        {
                            gasGrid[ind] -= reserve;
                            continue;
                        }
                        ind2 = ind - 1;
                        break;
                }
                if (gasGrid[ind2] == 0 && !CanMoveTo(ind2))
                {
                    wallOffset += 0.25f;
                    continue;
                }
                gasGrid[ind2] += reserve;
                gasGrid[ind] -= reserve;
                gasesToAdd.Add(ind2);
            }
        }
        public static bool IsOutside(int ind, Map map)
        {
            Room room = map.regionGrid?.regionGrid[ind]?.District.Room;
            if (room == null)
            {
                return true;
            }
            if (room.cachedOpenRoofCount == -1)
            {
                _ = room.OpenRoofCount;
            }
            if (room.cachedCellCount == -1)
            {
                _ = room.CellCount;

            }
            return room.cachedOpenRoofCount >= room.cachedCellCount * 0.25;
        }
        public void CreateGas(IntVec3 pos, float density)
        {
            int ind = map.cellIndices.CellToIndex(pos);
            if (ind > gasGrid.Length - 1) return;
            if (ind % map.Size.x == map.Size.x - 1) return;
            if (ind < 0) return;
            if (ind % map.Size.x == 0) return;
            if (!CanMoveTo(ind)) return;
            gasGrid[ind] += density;
            if (gases.Add(ind))
            {
                transparencyGrid[ind] = 0;
                parent.grid.totalGasCount++;
            }

        }
        public void MoveSomewhereElse(IntVec3 cell2)
        {
            int ind = CellToIndex(cell2);
            if (CanMoveTo(ind))
            {
                return;
            }
            if (!gases.Remove(ind))
            {
                return;
            }
            parent.grid.totalGasCount--;
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
                if (gasGrid[ind2] == 0 && !CanMoveTo(ind2)) continue;
                gasGrid[ind2] += gasGrid[ind];
                gasesToAdd.Add(ind2);
                parent.grid.totalGasCount++;
                break;
            }
            gasGrid[ind] = 0;
        }
        public void DestroyGas(IntVec3 cell2)
        {
            int ind = CellToIndex(cell2);
            gasGrid[ind] = 0;
            gases.Remove(ind);
        }
        public virtual void TickRare() { }
        public virtual void TickLong()
        {
            map.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
        }

        private const int maxRadial = 26;
        public void Explode(IntVec3 cell, Thing instigator)
        {
            int ind = CellToIndex(cell);
            if (gasGrid[ind] == 0)
            {
                return;
            }
            float totalGas = gasGrid[ind];
            int num = GenRadial.NumCellsInRadius((float)Math.Pow(gasGrid[ind] / Mathf.PI, 0.5f));
            if (num > maxRadial)
            {
                num = maxRadial;
            }
            int curInd = 0;
            while (curInd < num)
            {
                curInd++;
                int nextInd = CellToIndex(cell + GenRadial.RadialPattern[curInd]);
                if (nextInd < gasGrid.Length && gasGrid[nextInd] > 0)
                {
                    totalGas += gasGrid[nextInd];
                    gasGrid[nextInd] = 0;
                    transparencyGrid[nextInd] = 0;
                    gases.Remove(nextInd);
                    if (num < maxRadial)
                    {
                        num = GenRadial.NumCellsInRadius((float)Math.Pow(totalGas, 0.5f) / Mathf.PI);
                        if (num > maxRadial)
                        {
                            num = maxRadial;
                        }
                    }
                }
            }
            GenExplosion.DoExplosion(cell, map, (float)Math.Min(Math.Pow(totalGas / Mathf.PI, 0.5f), Math.Pow(num * 100f / Mathf.PI, 0.5f)), def.damageDef, instigator, (int)(totalGas / def.damageDivider));
            gasGrid[ind] = 0;
            transparencyGrid[ind] = 0;
            gases.Remove(ind);
        }


        public int CellToIndex(IntVec3 c)
        {
            return c.z * map.Size.x + c.x;
        }
        public IntVec3 IndexToCell(int ind)
        {
            return new IntVec3(ind % map.Size.x, 0, ind / map.Size.x);
        }
        public float DensityInCell(IntVec3 pos)
        {
            return gasGrid[CellToIndex(pos)];
        }
        public float DensityInCell(int ind)
        {
            return gasGrid[ind];
        }
        private bool CanMoveTo(int ind)
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

        internal int[] signGrid;
        internal int[] angleGrid;
        internal float[] xGrid;
        internal float[] zGrid;
        private void GenerateRandGrids()
        {
            int numCells = map.cellIndices.NumGridCells;
            signGrid = new int[numCells];
            angleGrid = new int[numCells];
            xGrid = new float[numCells];
            zGrid = new float[numCells];

            for (int i = 0; i < numCells; i++)
            {
                Rand.PushState();
                Rand.Seed = i + layer;
                signGrid[i] = Rand.Sign;
                angleGrid[i] = Rand.Range(0, 360);
                xGrid[i] += Rand.Range(-0.025f, 0.025f);
                zGrid[i] += Rand.Range(-0.024f, 0.024f);
                Rand.PopState();
            }
        }
        public void UpdateTransparency()
        {
            foreach (int i in gases)
            {
                if (transparencyGrid[i] < gasGrid[i])
                {
                    transparencyGrid[i] += Mathf.Clamp(gasGrid[i] - transparencyGrid[i], 0.5f, 2f);
                }
                else if (transparencyGrid[i] > gasGrid[i])
                {
                    transparencyGrid[i] -= Mathf.Clamp(transparencyGrid[i] - gasGrid[i], 0.5f, 2f);
                }
            }
        }

        public virtual void PostLoad()
        {
            parent = map.GetComponent<GasMapComponent>();
            GenerateRandGrids();

        }
        public virtual void ExposeData()
        {
            Scribe_References.Look(ref map, "map");
            Scribe_Defs.Look(ref def, "def");
            Scribe_Collections.Look(ref gases, "gases");
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                savedGases = new List<float>(gases.Count);
                for (int i = 0; i < gases.Count; i++)
                {
                    savedGases.Add(gasGrid[gases.ElementAt(i)]);
                }
            }
            Scribe_Collections.Look(ref savedGases, "savedGases");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                gasGrid = new float[map.cellIndices.NumGridCells];
                for (int i = 0; i < gases.Count; i++)
                {
                    gasGrid[gases.ElementAt(i)] = savedGases.ElementAt(i);
                }
                transparencyGrid = (float[])gasGrid.Clone();
                for (int i = 0; i < transparencyGrid.Length; i++)
                {
                    if (transparencyGrid[i] > 512)
                    {
                        transparencyGrid[i] = 512;
                    }
                }
            }
        }
    }
}
