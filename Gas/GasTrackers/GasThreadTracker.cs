using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GasExpansion.Gas.GasTrackers
{/*
    public class GasThreadTracker
    {
        public GasGridTracker parent;
        public void CreateThreads()
        {
            List<Thread> threads = new List<Thread>();
            List<DrawThread> drawThreads = new List<DrawThread>();
            foreach (GasGrid grid in parent.gasGrids)
            {
                DrawThread drawThread = new DrawThread(grid);
                drawThreads.Add(drawThread);
                threads.Add(new Thread(new ThreadStart(drawThread.Draw)));
            }
        }

    }
    
    public class DrawThread
    {
        GasGrid grid;
        GasMapComponent parent;
        public bool isFinished = true;
        public bool shouldSkip = false;
        public DrawThread(GasGrid grid)
        {
            this.grid = grid;
        }
        public void Draw()
        {
            isFinished = false;
            foreach (int i in grid.gases)
            {
                shouldSkip = true;
                if (grid.transparencyGrid[i] < parent.minGas) continue;
                IntVec3 p = grid.IndexToCell(i);
                if (!parent.currentViewRect.Contains(p))
                {
                    continue;
                }
                grid.color.a = Math.Min(grid.transparencyGrid[i] / 1707, 0.3f);
                grid.block.SetColor(grid.colorID, grid.color);
                Vector3 pos = p.ToVector3ShiftedWithAltitude(AltitudeLayer.Gas);
                pos.y += grid.layer / 1000f;
                pos.x += grid.xGrid[i];
                pos.z += grid.zGrid[i];
                grid.matrix.SetTRS(pos, Quaternion.AngleAxis((float)(grid.angleGrid[i] + parent.angle * grid.signGrid[i]), Vector3.up), grid.drawSize);
                shouldSkip = false;
                //Wait for main thread to release it
            }
            isFinished = true;
            shouldSkip = true;
        }
    }
    */
}
