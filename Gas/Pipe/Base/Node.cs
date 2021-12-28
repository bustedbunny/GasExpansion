using GasExpansion.Gas.Pipe.Base;
using GasExpansion.Gas.Pipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace GasExpansion.Gas.Pipe.Base
{
    public abstract class Node : PipeBase
    {
        public abstract Node[] ConnectedNodes
        {
            get;
        }
        public abstract PipeBase[] ConnectedPipes
        {
            get;
        }


        public override void UpdateConnections(bool updateAdj)
        {
            PipeBase[] oldConnectedPipes = (PipeBase[])ConnectedPipes.Clone();
            Node[] oldConnectedNodes = (Node[])ConnectedNodes.Clone();
            for (int i = 0; i < ConnectedPipes.Length; i++)
            {
                ConnectedPipes[i] = null;
                ConnectedNodes[i] = null;
            }
            if (updateAdj)
            {
                for (int i = 0; i < oldConnectedPipes.Length; i++)
                {
                    oldConnectedPipes[i]?.UpdateConnections(false);
                    oldConnectedNodes[i]?.UpdateConnections(false);
                }
            }
            if (ConnectorCells == null)
            {
                Map mapHeld = Map ?? MapHeld;
                mapHeld?.GetComponent<GasMapComponent>().pipeTracker.DeregisterAllDirtyRelatedLinks(this);
                return;
            }
            for (int i = 0; i < ConnectorCells.Length; i++)
            {
                PipeBase pipe = PipeUtility.GetFirstPipe<PipeBase>(ConnectorCells[i], Map);
                if (CanConnect(pipe))
                {
                    if (pipe is Segment segment)
                    {
                        ConnectedPipes[i] = segment;
                        if (updateAdj) segment.UpdateConnections(false);
                        ConnectedNodes[i] = PipeUtility.FindNodeRecursively(this, segment);
                        if (updateAdj) ConnectedNodes[i].UpdateConnections(false);
                    }
                    else if (pipe is Node node)
                    {
                        ConnectedPipes[i] = node;
                        ConnectedNodes[i] = node;
                        if (updateAdj) node.UpdateConnections(false);
                    }
                    if (ConnectedNodes[i] != null)
                    {
                        Map mapHeld = Map ?? MapHeld;
                        Link link = mapHeld?.GetComponent<GasMapComponent>().pipeTracker.TryRegisterLink(this, ConnectedNodes[i]);
                        if (link != null)
                        {

                        }
                    }
                }
            }
            Map map = Map ?? MapHeld;
            map?.GetComponent<GasMapComponent>().pipeTracker.DeregisterAllDirtyRelatedLinks(this);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DebugUpdate",
                    action = () =>
                    {
                        this.UpdateConnections(true);
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "LogLinks",
                    action = () =>
                    {
                        Map.GetComponent<GasMapComponent>().pipeTracker.LogLinks();
                    }
                };
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(base.GetInspectString());
            stringBuilder.Append("Rotation: " + PipeUtility.RotationDirectionText(Rotation));
            string num;
            for (int i = 0; i < ConnectedPipes.Length; i++)
            {
                num = PipeUtility.NumToString(i);
                stringBuilder.Append($"\n{num} pipe: {ConnectedPipes[i]?.LabelCap.ToString() ?? "null"}");
                stringBuilder.Append($"\n{num} node: {ConnectedNodes[i]?.LabelCap.ToString() ?? "null"}");
            }
            return stringBuilder.ToString();
        }
    }
}
