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
        public float currentPressure;

        private const float maxPressure = 1000f;
        private static readonly IntVec3[] offsets =
{
            new IntVec3(0, 0,1),
            new IntVec3(0, 0,-1),
            new IntVec3(-1, 0,0),
            new IntVec3(1, 0,0)
        };
        public virtual float Spread(Segment from, float pressure)
        {
            Segment target = (from == zeroSegment) ? oneSegment : zeroSegment;
            float excessivePressure = Pressure(target, pressure);
            return excessivePressure;
        }
        protected float Pressure(Segment to, float pressure)
        {
            float excessivePressure = 0;
            if (to != null)
            {
                currentPressure += to.Spread(this, pressure);
                if (currentPressure > maxPressure)
                {
                    excessivePressure = (maxPressure - currentPressure) * -1f;
                    currentPressure = maxPressure;
                }
            }
            else
            {
                currentPressure += pressure;
                if (currentPressure > maxPressure)
                {
                    excessivePressure = (maxPressure - currentPressure) * -1f;
                    currentPressure = maxPressure;
                }
            }
            return excessivePressure;
        }

        public void UpdateConnections()
        {
            if (this.Rotation == Rot4.South)
            {
                zeroSegment = GetFirstPipe(Position + offsets[1]);
                oneSegment = GetFirstPipe(Position + offsets[0]);
            }
            else if (this.Rotation == Rot4.North)
            {
                zeroSegment = GetFirstPipe(Position + offsets[0]);
                oneSegment = GetFirstPipe(Position + offsets[1]);
            }
            else if (this.Rotation == Rot4.West)
            {
                zeroSegment = GetFirstPipe(Position + offsets[2]);
                oneSegment = GetFirstPipe(Position + offsets[3]);
            }
            else if (this.Rotation == Rot4.East)
            {
                zeroSegment = GetFirstPipe(Position + offsets[3]);
                oneSegment = GetFirstPipe(Position + offsets[2]);
            }
        }

        private Segment GetFirstPipe(IntVec3 cell)
        {
            foreach (Thing item in cell.GetThingList(Map))
            {
                if (item is Segment segment)
                {
                    return segment;
                }
            }
            return null;
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            UpdateConnections();
            for (int i = 0; i < 4; i++)
            {
                GetFirstPipe(Position + offsets[i])?.UpdateConnections();
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
            stringBuilder.Append(currentPressure.ToString());
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
