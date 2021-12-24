using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using GasExpansion.Gas.Pipe.Base;

namespace GasExpansion.Gas.Pipe.Base
{
    public static class PipeUtility
    {
        public static Segment GetFirstSegment(IntVec3 cell, Map map)
        {
            if (cell == null || map == null)
            {
                return null;
            }
            foreach (Thing thing in map.thingGrid.ThingsAt(cell))
            {
                if (thing is Segment)
                {
                    return thing as Segment;
                }
            }
            return null;
        }
        public static PipeBase GetFirstPipe(IntVec3 cell, Map map)
        {
            if (cell == null || map == null)
            {
                return null;
            }
            foreach (Thing thing in map.thingGrid.ThingsAt(cell))
            {
                if (thing is PipeBase)
                {
                    return thing as PipeBase;
                }
            }
            return null;
        }
        public static Node GetFirstNode(IntVec3 cell, Map map)
        {
            if (cell == null || map == null)
            {
                return null;
            }
            foreach (Thing thing in map.thingGrid.ThingsAt(cell))
            {
                if (thing is Node)
                {
                    return thing as Node;
                }
            }
            return null;
        }

        public static Node FindNodeRecursively(PipeBase prevPipe, PipeBase nextPipe)
        {
            if (nextPipe is Node)
            {
                return nextPipe as Node;
            }
            while (nextPipe is Segment segment)
            {
                nextPipe = segment.GetOtherPipe(prevPipe);
                if (nextPipe != null)
                {
                }
                if (nextPipe is Node node)
                {
                    return node;
                }
                prevPipe = segment;
            }
            return null;
        }

        public static void UpdatePipesRecursively(PipeBase startPipe, PipeBase nextPipe)
        {
            nextPipe?.UpdateConnections(false);
            while (nextPipe != null)
            {
                nextPipe.UpdateConnections(false);
                if (nextPipe is Segment segment)
                {
                    nextPipe = segment.GetOtherPipe(startPipe);
                    startPipe = segment;
                }
                else if (nextPipe is Node node)
                {
                    node.UpdateConnections(false);
                    return;
                }
            }
        }

        public static string RotationDirectionText(Rot4 rot)
        {
            if (rot == Rot4.North)
            {
                return "North";
            }
            if (rot == Rot4.South)
            {
                return "South";
            }
            if (rot == Rot4.East)
            {
                return "East";
            }
            if (rot == Rot4.West)
            {
                return "West";
            }
            return "Invalid";
        }

        public static string NumToString(int i)
        {
            switch (i)
            {
                case 0: return "First";
                case 1: return "Second";
                case 2: return "Third";
                default: return "Fourth";
            }
        }

        public static readonly IntVec3[] Offsets =
        {
            new IntVec3(0, 0, 1),
            new IntVec3(0, 0, -1),
            new IntVec3(-1, 0, 0),
            new IntVec3(1, 0, 0)
        };
    }
}
