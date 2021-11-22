using GasExpansion.Gas.Pipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class Segment : Building
    {
        public Segment zeroSegment;
        public Segment oneSegment;

        protected static readonly IntVec3[] offsets =
{
            new IntVec3(0, 0,1),
            new IntVec3(0, 0,-1),
            new IntVec3(-1, 0,0),
            new IntVec3(1, 0,0)
        };

        public virtual void UpdateConnections()
        {
            if (this.Rotation == Rot4.South)
            {
                zeroSegment = PipeUtility.GetFirstSegment(Position + offsets[1], Map);
                oneSegment = PipeUtility.GetFirstSegment(Position + offsets[0], Map);
            }
            else if (this.Rotation == Rot4.North)
            {
                zeroSegment = PipeUtility.GetFirstSegment(Position + offsets[0], Map);
                oneSegment = PipeUtility.GetFirstSegment(Position + offsets[1], Map);
            }
            else if (this.Rotation == Rot4.West)
            {
                zeroSegment = PipeUtility.GetFirstSegment(Position + offsets[2], Map);
                oneSegment = PipeUtility.GetFirstSegment(Position + offsets[3], Map);
            }
            else if (this.Rotation == Rot4.East)
            {
                zeroSegment = PipeUtility.GetFirstSegment(Position + offsets[3], Map);
                oneSegment = PipeUtility.GetFirstSegment(Position + offsets[2], Map);
            }
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            UpdateConnections();
            for (int i = 0; i < 4; i++)
            {
                PipeUtility.GetFirstSegment(Position + offsets[i], Map)?.UpdateConnections();
            }
        }

        public override string LabelCap
        {
            get
            {
                return base.LabelCap + " " + thingIDNumber.ToString();
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.Append(Rotation.ToString());
            if (zeroSegment != null)
            {
                stringBuilder.Append("\nZero segment: " + zeroSegment?.thingIDNumber);
            }
            if (oneSegment != null)
            {
                stringBuilder.Append("\nOne segment: " + oneSegment?.thingIDNumber);
            }

            return stringBuilder.ToString();


        }
    }


}
