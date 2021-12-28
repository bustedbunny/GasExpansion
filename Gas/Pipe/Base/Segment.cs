using GasExpansion.Gas.Pipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion.Gas.Pipe.Base
{
    public abstract class Segment : PipeBase
    {
        public PipeBase aSegment;
        public PipeBase bSegment;

        public abstract (Rot4, short, short)[] Offsets
        {
            get;
        }
        private IntVec3[] connectorCells;
        public override IntVec3[] ConnectorCells => connectorCells;
        public PipeBase GetOtherPipe(PipeBase pipe)
        {
            if (aSegment == pipe)
            {
                return bSegment;
            }
            if (bSegment == pipe)
            {
                return aSegment;
            }
            //Log.Error("Returned error in GetNextPipe " + LabelCap + ". PrevPipe " + (pipe?.LabelCap ?? "null") + ". aSeg " + (aSegment?.LabelCap ?? "null")
            //    + ". bSeg " + (bSegment?.LabelCap ?? "null"));P
            return null;
        }
        public override void UpdateConnectorCells()
        {
            for (int i = 0; i < 4; i++)
            {
                if (Offsets[i].Item1 == Rotation)
                {
                    connectorCells = new IntVec3[2];
                    ConnectorCells[0] = Position + PipeUtility.Offsets[Offsets[i].Item2];
                    ConnectorCells[1] = Position + PipeUtility.Offsets[Offsets[i].Item3];
                    return;
                }
            }
            connectorCells = null;
        }

        public override void UpdateConnections(bool updateAdj)
        {
            PipeBase aOldSegment = aSegment;
            PipeBase bOldSegment = bSegment;
            aSegment = null;
            bSegment = null;
            if (updateAdj)
            {
                PipeUtility.FindNodeRecursively(this, aOldSegment)?.UpdateConnections(false);
                PipeUtility.FindNodeRecursively(this, bOldSegment)?.UpdateConnections(false);
                aOldSegment?.UpdateConnections(false);
                bOldSegment?.UpdateConnections(false);
            }
            if (connectorCells == null)
            {
                return;
            }
            PipeBase pipe = PipeUtility.GetFirstPipe<PipeBase>(ConnectorCells[0], Map);
            if (CanConnect(pipe))
            {
                aSegment = pipe;
            }
            pipe = PipeUtility.GetFirstPipe<PipeBase>(ConnectorCells[1], Map);
            if (CanConnect(pipe))
            {
                bSegment = pipe;
            }
            if (updateAdj)
            {
                aSegment?.UpdateConnections(false);
                bSegment?.UpdateConnections(false);
                PipeUtility.FindNodeRecursively(this, aSegment)?.UpdateConnections(false);
                PipeUtility.FindNodeRecursively(this, bSegment)?.UpdateConnections(false);
            }
        }



        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.Append(PipeUtility.RotationDirectionText(Rotation));
            stringBuilder.Append($"\nA segment: {aSegment?.thingIDNumber.ToString() ?? "null"}");
            stringBuilder.Append($"\nB segment: {bSegment?.thingIDNumber.ToString() ?? "null"}");
            return stringBuilder.ToString();
        }
    }


}
