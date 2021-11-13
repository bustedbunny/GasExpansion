using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class DebugActionsGasExpansion
    {
        [DebugAction("Spawning", "Pipe", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Pipe()
        {
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(PipeOptions()));

        }

        public static List<DebugMenuOption> PipeOptions()
        {
            List<DebugMenuOption> list = new();

            list.Add(new DebugMenuOption("Explode", DebugMenuOptionMode.Tool, delegate
            {
                foreach (Thing item in UI.MouseCell().GetThingList(Find.CurrentMap))
                {
                    if (item is Segment segment)
                    {
               //         segment.Spread(null, 50);
                    }
                }
            }));
            return list;
        }








        [DebugAction("Spawning", "Explode", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Explode()
        {
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(ExplodeOption()));

        }

        public static List<DebugMenuOption> ExplodeOption()
        {
            List<DebugMenuOption> list = new();

            list.Add(new DebugMenuOption("Explode", DebugMenuOptionMode.Tool, delegate
            {
                foreach (GasGrid grid in Find.CurrentMap.GetComponent<GasMapComponent>().explosiveGrids)
                {
                    grid.Explode(UI.MouseCell(), null);
                }
            }));
            return list;
        }

        [DebugAction("Spawning", "Try place gasGrid...", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TryGridPlace()
        {
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(Options()));
        }

        [DebugAction("Spawning", "Clear All Gas", false, false, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void ClearAllGas()
        {
            foreach (GasGrid grid in Find.CurrentMap.GetComponent<GasMapComponent>().gasGrids)
            {
                grid.gases.Clear();
                grid.gasGrid = new float[grid.gasGrid.Length];
            }
        }

        public static List<DebugMenuOption> Options()
        {
            List<DebugMenuOption> list = new();
            foreach (GasDef item in DefDatabase<GasDef>.AllDefs)
            {
                list.Add(new DebugMenuOption(item.LabelCap, DebugMenuOptionMode.Tool, delegate
                {
                    Find.CurrentMap.GetComponent<GasMapComponent>().gasGrids.First(x => x.def == item).CreateGas(UI.MouseCell(), 5000);
                }));
            }
            return list;
        }
    }
}
