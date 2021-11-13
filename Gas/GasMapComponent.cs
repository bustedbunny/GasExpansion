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

namespace GasExpansion
{
#if DEBUG
    [HotSwappable]
#endif
    public class GasMapComponent : MapComponent
    {
        public List<GasGrid> gasGrids = new();
        public List<GasGrid> badGrids = new();
        public List<GasGrid> concealingGrids = new();
        public List<GasGrid> explosiveGrids = new();
        private bool[] toxicGrid;
        private bool[] corrosiveGrid;
        public GasMapComponent(Map map)
            : base(map)
        {

        }



        private float windDirectionTarget = Rand.Range(0, 360f);
        private float windDirection;
        public float windDirectionFast = 0;
        public float windStrength = 0;
        public float WindDirection => windDirection;
        private static readonly string[] windDirections = new string[8] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        public readonly float[] windOffset = new float[4];

        public byte[] pipeGrid;

        private const float rangeMultiplier = 1f;
        public bool direction = false;
        public Vector2 winddir = new();
        private void UpdateWind()
        {

            float dif = (windDirectionTarget - windDirection);
            if (dif > 0)
            {
                if (dif < 180)
                {
                    windDirection += Rand.Range(0f, 5f) * rangeMultiplier;
                }
                else
                {
                    windDirection -= Rand.Range(0f, 5f) * rangeMultiplier;
                }
            }
            else
            {
                if (dif < -180)
                {
                    windDirection += Rand.Range(0f, 5f) * rangeMultiplier;
                }
                else
                {
                    windDirection -= Rand.Range(0f, 5f) * rangeMultiplier;
                }
            }
            if (windDirection > 360f)
            {
                windDirection -= 360f;
            }
            if (windDirection < 0f)
            {
                windDirection += 360f;
            }

            while (Math.Abs(windDirectionTarget - windDirection) < 1f)
            {
                while (Math.Abs(windDirectionTarget - windDirection) < 30f)
                {

                    windDirectionTarget = Rand.Range(0, 360);
                }
            }
            windDirectionFast = Mathf.Deg2Rad * windDirection;
        }
        private const double minOffset = 0.5;
        private void CalculateWindOffsets()
        {
            double angle = windDirectionFast;
            double strength = (double)Math.Min(map.windManager.WindSpeed * 0.67, 6);
            double N = minOffset + Math.Pow(1.0 + Math.Cos(angle), strength);
            double E = minOffset + Math.Pow(1.0 + Math.Sin(angle), strength);
            double S = minOffset + Math.Pow(1.0 + Math.Cos(angle + Math.PI), strength);
            double W = minOffset + Math.Pow(1.0 + Math.Sin(angle + Math.PI), strength);
            double sum = minOffset * 4 * Math.Max(1, strength) / (N + E + S + W);
            windOffset[0] = (float)(N * sum);
            windOffset[1] = (float)(E * sum);
            windOffset[2] = (float)(S * sum);
            windOffset[3] = (float)(W * sum);
        }

        public void DoWindGUI(float xPos, ref float yPos)
        {
            float num = 100f;
            float width = 200f + num;
            float num2 = 26f;
            Rect rect = new(xPos - num, yPos - num2, width, num2);
            Text.Anchor = TextAnchor.MiddleRight;
            rect.width -= 15f;
            Text.Font = GameFont.Small;
            Widgets.Label(rect, WindStrengthText + WindDirectionText + " " + Math.Round(windDirection).ToString() + " " + windDirectionTarget.ToString());
            TooltipHandler.TipRegion(rect, "GE_Wind_Tooltip".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            yPos -= num2;
        }

        private string WindDirectionText
        {
            get
            {
                if (BeaufortScale == 0)
                {
                    return "";
                }
                int num = Mathf.Clamp(Mathf.RoundToInt(windDirection / 45f), 0, 7);
                return ", " + ("GE_Wind_Direction_" + windDirections[num]).Translate();
            }
        }
        private float WindStrength => Mathf.Min(3f * map.windManager.WindSpeed, 9f);
        private int BeaufortScale => Mathf.FloorToInt(WindStrength);
        private string WindStrengthText => ("GE_Wind_Beaufort" + BeaufortScale).Translate();


        public override void MapComponentTick()
        {

            int tick = Find.TickManager.TicksGame;
            for (int i = 0; i < gasGrids.Count; i++)
            {
                gasGrids[i].Tick(tick);

            }

            if (tick % 15 == 0)
            {
                Parallel.ForEach(gasGrids, grid =>
                {
                    grid.TickThrottled();
                });

                /*


                                for (int i = 0; i < gasGrids.Count; i++)
                {
                    gasGrids[i].TickThrottled();
                }
                */
            }
            if (tick % 250 == 0)
            {
                for (int i = 0; i < gasGrids.Count; i++)
                {
                    gasGrids[i].TickRare();
                }
                CachePathGrid();
                UpdateWind();
                CalculateWindOffsets();


                if (tick % 2000 == 0)
                {
                    for (int i = 0; i < gasGrids.Count; i++)
                    {
                        gasGrids[i].TickLong();
                        Log.Message(gasGrids[i].layer.ToString() + " " + gasGrids[i].def.LabelCap);
                    }
                }
            }
        }



        public bool IsDangerCell(IntVec3 cell)
        {
            int ind = map.cellIndices.CellToIndex(cell);
            if (toxicGrid[ind])
            {
                return true;
            }
            if (corrosiveGrid[ind])
            {
                return true;
            }
            return false;
        }
        public bool IsDangerCell(int ind)
        {
            if (toxicGrid[ind])
            {
                return true;
            }
            if (corrosiveGrid[ind])
            {
                return true;
            }
            return false;
        }

        public bool ShouldPathThrough(IntVec3 cell, GasExpansion_PawnResistance resistance)
        {
            int ind = map.cellIndices.CellToIndex(cell);
            if (!resistance.toxic && toxicGrid[ind])
            {
                return false;
            }
            if (!resistance.corrosive && corrosiveGrid[ind])
            {
                return false;
            }
            return true;
        }
        public bool ShouldPathThrough(int ind, GasExpansion_PawnResistance resistance)
        {
            if (!resistance.toxic && toxicGrid[ind])
            {
                return false;
            }
            if (!resistance.corrosive && corrosiveGrid[ind])
            {
                return false;
            }
            return true;
        }
        public bool ShouldPathThrough(IntVec3 cell, Pawn pawn)
        {
            int ind = map.cellIndices.CellToIndex(cell);
            if (!toxicGrid[ind] && !corrosiveGrid[ind])
            {
                return true;
            }
            GasExpansion_PawnResistance resistance = GetResistance(pawn);
            if (!resistance.toxic && toxicGrid[ind])
            {
                return false;
            }
            if (!resistance.corrosive && corrosiveGrid[ind])
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
        private void CachePathGrid()
        {
            toxicGrid = new bool[map.cellIndices.NumGridCells];
            corrosiveGrid = new bool[map.cellIndices.NumGridCells];
            for (int i = 0; i < badGrids.Count; i++)
            {
                if (badGrids[i].def.isToxic)
                {
                    foreach (int ind in badGrids[i].gases)
                    {
                        if (badGrids[i].gasGrid[ind] > 30)
                        {
                            toxicGrid[ind] = true;
                        }
                    }
                }
                else if (badGrids[i].def.isCorrosive)
                {
                    foreach (int ind in badGrids[i].gases)
                    {
                        if (badGrids[i].gasGrid[ind] > 30)
                        {
                            corrosiveGrid[ind] = true;
                        }
                    }
                }
                else if (badGrids[i].def.isToxic && badGrids[i].def.isCorrosive)
                {
                    foreach (int ind in badGrids[i].gases)
                    {
                        if (badGrids[i].gasGrid[ind] > 30)
                        {
                            corrosiveGrid[ind] = true;
                            toxicGrid[ind] = true;
                        }
                    }
                }
            }
        }

        public static float GasVisionInCell(IntVec3 cell, Map map)
        {
            GasMapComponent comp = map.GetComponent<GasMapComponent>();
            int ind = comp.gasGrids[0].CellToIndex(cell);
            float num = 0;
            for (int i = 0; i < comp.concealingGrids.Count; i++)
            {
                num += Math.Min(comp.concealingGrids[i].DensityInCell(ind), 512) / 512f;
            }
            return num;
        }

        public override void MapComponentUpdate()
        {
            CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
            currentViewRect.minX -= 17;
            currentViewRect.minZ -= 17;

            for (int i = 0; i < gasGrids.Count; i++)
            {
                gasGrids[i].Draw(currentViewRect, Find.TickManager.TicksGame);
            }


        }
#if DEBUG
        public override void MapComponentOnGUI()
        {
            if (!Prefs.DevMode)
            {
                return;
            }
            CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
            currentViewRect.ClipInsideMap(map);
            currentViewRect.ExpandedBy(1);
            foreach (GasGrid grid in gasGrids)
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
#endif

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
            pipeGrid = new byte[map.cellIndices.NumGridCells];
            toxicGrid = new bool[map.cellIndices.NumGridCells];
            corrosiveGrid = new bool[map.cellIndices.NumGridCells];
            for (int i = 0; i < gasGrids.Count; i++)
            {
                gasGrids[i].parent = this;
            }
        }
        public override void FinalizeInit()
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
                gasGrids[i].parent = this;
                gasGrids[i].layer = i;
            }
            CachePathGrid();
        }
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref gasGrids, "gasGrids", LookMode.Deep);
            Scribe_Values.Look(ref windDirection, "windDirection");
            Scribe_Values.Look(ref windDirectionTarget, "windDirectionTarget");
            Scribe_Values.Look(ref winddir, "winddir");
        }
    }
}
