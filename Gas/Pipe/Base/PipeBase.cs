using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion.Gas.Pipe.Base
{
    public abstract class PipeBase : Building
    {

        public abstract IntVec3[] ConnectorCells
        {
            get;
        }
        public virtual bool CanConnect(PipeBase pipe)
        {
            if (ConnectorCells == null || pipe == null || pipe.ConnectorCells == null)
            {
                return false;
            }
            if (pipe.ConnectorCells.Contains(Position) && ConnectorCells.Contains(pipe.Position))
            {
                return true;
            }
            return false;
        }
        public abstract void UpdateConnections(bool updateAdj);

        public abstract void UpdateConnectorCells();

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            UpdateConnectorCells();
            UpdateConnections(true);
        }
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            UpdateConnectorCells();
            UpdateConnections(true);
        }

        public override string LabelCap
        {
            get
            {
                return base.LabelCap + " " + thingIDNumber.ToString();
            }
        }
    }
}
