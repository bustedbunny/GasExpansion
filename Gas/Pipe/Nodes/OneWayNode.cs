using GasExpansion.Gas.Pipe.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class OneWayNode : Node
    {
        public override Node[] ConnectedNodes => connectedNodes;
        private readonly Node[] connectedNodes = new Node[1];
        public override PipeBase[] ConnectedPipes => connectedPipes;
        private readonly PipeBase[] connectedPipes = new PipeBase[1];

        public override IntVec3[] ConnectorCells => connectorCells;
        private IntVec3[] connectorCells;

        private static readonly (Rot4, short)[] offsets =
        {
            (Rot4.North, 0),
            (Rot4.South, 1),
            (Rot4.West, 2),
            (Rot4.East, 3)
        };
        public override void UpdateConnectorCells()
        {
            for (int i = 0; i < 4; i++)
            {
                if (offsets[i].Item1 == Rotation)
                {
                    connectorCells = new IntVec3[1];
                    connectorCells[0] = Position + PipeUtility.Offsets[offsets[i].Item2];
                    return;
                }
            }
            connectorCells = null;
        }
    }
}
