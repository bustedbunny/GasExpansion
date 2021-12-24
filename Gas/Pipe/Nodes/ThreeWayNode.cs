using GasExpansion.Gas.Pipe.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion
{
    public class ThreeWayNode : Node
    {
        public override Node[] ConnectedNodes => connectedNodes;
        private readonly Node[] connectedNodes = new Node[3];
        public override PipeBase[] ConnectedPipes => connectedPipes;
        private readonly PipeBase[] connectedPipes = new PipeBase[3];

        public override IntVec3[] ConnectorCells => connectorCells;
        private IntVec3[] connectorCells;


        private static readonly (Rot4, short, short, short)[] _offsets =
        {
            (Rot4.North, 0, 2 ,3),
            (Rot4.South, 1, 3, 2),
            (Rot4.West, 2, 1, 0),
            (Rot4.East, 3, 0, 1)
        };

        public override void UpdateConnectorCells()
        {
            for (int i = 0; i < 4; i++)
            {
                if (_offsets[i].Item1 == Rotation)
                {
                    connectorCells = new IntVec3[3];
                    ConnectorCells[0] = Position + PipeUtility.Offsets[_offsets[i].Item2];
                    ConnectorCells[1] = Position + PipeUtility.Offsets[_offsets[i].Item3];
                    ConnectorCells[2] = Position + PipeUtility.Offsets[_offsets[i].Item4];
                }
            }
        }
    }
}
