using GasExpansion.Gas.Pipe.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class TwoWayNode : Node
    {
        public override Node[] ConnectedNodes => connectedNodes;
        private readonly Node[] connectedNodes = new Node[2];
        public override PipeBase[] ConnectedPipes => connectedPipes;
        private readonly PipeBase[] connectedPipes = new PipeBase[2];

        public override IntVec3[] ConnectorCells => connectorCells;
        private IntVec3[] connectorCells;



        private static readonly (Rot4, short, short)[] _offsets =
        {
            (Rot4.North, 1, 0),
            (Rot4.South, 0, 1),
            (Rot4.West, 3, 2),
            (Rot4.East, 2, 3)
        };

        public override void UpdateConnectorCells()
        {
            for (int i = 0; i < 4; i++)
            {
                if (_offsets[i].Item1 == Rotation)
                {
                    connectorCells = new IntVec3[2];
                    ConnectorCells[0] = Position + PipeUtility.Offsets[_offsets[i].Item2];
                    ConnectorCells[1] = Position + PipeUtility.Offsets[_offsets[i].Item3];
                    return;
                }
            }
            connectorCells = null;
        }
    }
}
