using HotSwap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GasExpansion.Gas.GasTrackers
{

    struct MeshProperties
    {
        public int index;
        public float alpha;
        public int color;

        public static int Size()
        {
            return sizeof(int) * 2 + sizeof(float);
        }
    }
#if DEBUG
    [HotSwappable]
#endif
    public class GasDrawer
    {
        public GasDrawer(GasGridTracker grid, Map map)
        {
            this.grid = grid;
            this.map = map;
        }
        //References
        public GasGridTracker grid;
        public Map map;
        //Buffers
        private Material material;
        private float angle;
        private static Bounds bounds = new(Vector3.zero, Vector3.one * 10000f);

        private MeshProperties[] meshProperties;

        private ComputeBuffer meshBuffer;
        private ComputeBuffer argsBuffer;
        private ComputeBuffer matrixBuffer;
        private ComputeBuffer colorBuffer;

        internal bool bufferIsDirty = true;
        private bool isInitialised = false;
        public void Draw()
        {
            if (grid.totalGasCount == 0)
            {
                return;
            }
            if (!isInitialised)
            {
                GetMaterial();
                CreateConstBuffers();
                UpdateBuffer();
                isInitialised = true;
            }
            angle = (Find.TickManager.TicksGame % 720) / Mathf.PI * 180f / 1000f;
            material.SetFloat("_Angle", angle);
            if (bufferIsDirty)
            {
                meshBuffer.Release();
                UpdateBuffer();
                UpdateArgsBuffer();
                bufferIsDirty = false;

            }
            Graphics.DrawMeshInstancedIndirect(MeshPool.plane10, 0, material, bounds, argsBuffer);
        }
        private void UpdateBuffer()
        {
            meshProperties = new MeshProperties[grid.totalGasCount];
            FillMeshProps();
            DispatchBuffers();
        }

        private void FillMeshProps()
        {

            int j = 0;
            int ind = 0;
            foreach (GasGrid gasGrid in grid.gasGrids)
            {
                foreach (int i in gasGrid.gases)
                {
                    meshProperties[j].index = i;
                    meshProperties[j].color = ind;
                    meshProperties[j].alpha = gasGrid.transparencyGrid[i];
                    j++;
                }
                ind++;
            }
        }
        private void DispatchBuffers()
        {
            meshBuffer = new ComputeBuffer(grid.totalGasCount, MeshProperties.Size());
            meshBuffer.SetData(meshProperties, 0, 0, grid.totalGasCount);
            material.SetBuffer("_MeshProperties", meshBuffer);
        }
        private void CreateConstBuffers()
        {
            material.SetFloat("_MaxAlpha", 512f);
            SetColorBuffer();
            SetMatrixBuffer();
        }
        private void SetColorBuffer()
        {
            Vector4[] colors = new Vector4[grid.gasGrids.Count];
            for (int i = 0; i < grid.gasGrids.Count; i++)
            {
                colors[i] = new Color(grid.gasGrids[i].def.color.r, grid.gasGrids[i].def.color.g, grid.gasGrids[i].def.color.b, 255f);
                Log.Message(colors[i].ToString() + grid.gasGrids[i].def.LabelCap);
            }
            colorBuffer = new ComputeBuffer(colors.Length, sizeof(float) * 4);
            colorBuffer.SetData(colors);
            material.SetBuffer("_Colors", colorBuffer);
        }
        private void SetMatrixBuffer()
        {
            Vector3 size = new(2.5f, 0f, 2.5f);
            Matrix4x4[] matrices = new Matrix4x4[map.cellIndices.NumGridCells];
            for (int i = 0; i < map.cellIndices.NumGridCells; i++)
            {
                Rand.PushState(i);
                Vector3 pos = map.cellIndices.IndexToCell(i).ToVector3ShiftedWithAltitude(AltitudeLayer.Gas);
                pos.x += Rand.Range(-0.25f, 0.25f);
                pos.z += Rand.Range(-0.24f, 0.24f);
                Quaternion rotation = Quaternion.AngleAxis(((float)Rand.Range(0, 360)), Vector3.up);
                matrices[i] = Matrix4x4.TRS(pos, rotation, size);
                Rand.PopState();
            }
            matrixBuffer = new ComputeBuffer(matrices.Length, sizeof(float) * 4 * 4);
            matrixBuffer.SetData(matrices);
            material.SetBuffer("_Matrices", matrixBuffer);
        }
        private void UpdateArgsBuffer()
        {
            if (argsBuffer != null)
            {
                argsBuffer.Release();
            }
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = MeshPool.plane10.GetIndexCount(0);
            args[1] = (uint)(grid.totalGasCount);
            args[2] = MeshPool.plane10.GetIndexStart(0);
            args[3] = MeshPool.plane10.GetBaseVertex(0);
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            argsBuffer.SetData(args);
        }
        private void GetMaterial()
        {
            if (GasExpansionContentDatabase.InstancedColor == null)
            {
                Log.Error("Shader is null");
            }
            MaterialRequest req = new(color: new Color(255, 255, 255, 255), tex: Textures.tex, shader: GasExpansionContentDatabase.InstancedColor);
            req.mainTex.name = "_MainTex";
            material = MaterialPool.MatFrom(req);
            material.enableInstancing = true;
        }

        public void ReleaseAllBuffers()
        {
            meshBuffer.Release();
            argsBuffer.Release();
            matrixBuffer.Release();
            colorBuffer.Release();
        }
    }
}
