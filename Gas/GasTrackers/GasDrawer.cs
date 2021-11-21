using HotSwap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace GasExpansion.Gas.GasTrackers
{
#if DEBUG
    [HotSwappable]
#endif
    public class GasDrawer
    {
        public GasDrawer(GasGridTracker grid, Map map)
        {
            this.grid = grid;
            this.map = map;
            bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
        }
        //References
        public GasGridTracker grid;
        public Map map;
        //Buffers
        private Material material;
        private float angle;
        private Bounds bounds;
        private ComputeBuffer meshPropertiesBuffer;
        private ComputeBuffer argsBuffer;
        private ComputeBuffer matrixBuffer;
        private ComputeBuffer colorBuffer;
        public void Draw()
        {
            if (grid.totalGasCount == 0)
            {
                return;
            }
            if (material == null)
            {
                GetMaterial();
            }
            if (matrixBuffer == null)
            {
                CreateConstBuffers();
            }
            if (meshPropertiesBuffer != null)
            {
                meshPropertiesBuffer.Release();
            }
            angle = (Find.TickManager.TicksGame % 720) / Mathf.PI * 180f / 1000f;
            material.SetFloat("_Angle", angle);

            UpdateBuffer();

            Graphics.DrawMeshInstancedIndirect(MeshPool.plane10, 0, material, bounds, argsBuffer);

        }
        private void UpdateBuffer()
        {
            float[] data = new float[map.cellIndices.NumGridCells * grid.gasGrids.Count];
            for (int i = 0; i < grid.gasGrids.Count; i++)
            {
                grid.gasGrids[i].gasGrid.CopyTo(data, i * map.cellIndices.NumGridCells);
            }
            meshPropertiesBuffer = new ComputeBuffer(map.cellIndices.NumGridCells * grid.gasGrids.Count, sizeof(float));
            meshPropertiesBuffer.SetData(data, 0, 0, data.Length);
            material.SetBuffer("_Alpha", meshPropertiesBuffer);
        }

        private void CreateConstBuffers()
        {
            Log.Message(map.cellIndices.NumGridCells.ToString());
            material.SetInt("_SizeOfArray", map.cellIndices.NumGridCells);
            material.SetFloat("_MaxAlpha", 512f);
            SetColorBuffer();
            SetMatrixBuffer();
            SetArgsBuffer();
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
        private void SetArgsBuffer()
        {
            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = MeshPool.plane10.GetIndexCount(0);
            args[1] = (uint)(map.cellIndices.NumGridCells * grid.gasGrids.Count);
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
            MaterialRequest req = new MaterialRequest(color: new Color(255, 255, 255, 255), tex: Textures.tex, shader: GasExpansionContentDatabase.InstancedColor);
            req.mainTex.name = "_MainTex";
            material = MaterialPool.MatFrom(req);
            material.enableInstancing = true;
        }

        public void ReleaseAllBuffers()
        {
            meshPropertiesBuffer.Release();
            argsBuffer.Release();
            matrixBuffer.Release();
            colorBuffer.Release();
        }
    }
}
