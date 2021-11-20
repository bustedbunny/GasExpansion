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
        private Bounds bounds;
        private struct MeshProperties
        {
            public uint index;
            public Vector4 color;
            public float yOffset;
            public float angle;
            public static int Size()
            {
                return sizeof(float) * 7;
            }
        }


        private MaterialPropertyBlock block = new();
        private Material material;
        private CellRect currentViewRect;
        private float minGas;
        private float angle;

        public GasGridTracker grid;
        public Map map;


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
                UpdateMatrixBuffer();
            }
            if (meshPropertiesBuffer != null)
            {
                meshPropertiesBuffer.Release();
            }
            //    angle = (Find.TickManager.TicksGame % 720) / 5;
            angle = (Find.TickManager.TicksGame % 720) / Mathf.PI * 180f /1000f;
            MeshProperties[] properties = new MeshProperties[grid.totalGasCount];
            int ind = 0;
            foreach (GasGrid gasGrid in grid.gasGrids)
            {
                foreach (int i in gasGrid.gases)
                {
                    IntVec3 p = gasGrid.IndexToCell(i);
                    MeshProperties props = new MeshProperties();
                    props.angle = angle * gasGrid.signGrid[i];
                    props.index = (uint)i;
                    props.yOffset = gasGrid.layer;
                    props.color = new Color(gasGrid.color.r, gasGrid.color.g, gasGrid.color.b, Math.Min(gasGrid.transparencyGrid[i] / 512, 200)); //
                    //1707 0.3
                    properties[ind] = props;
                    ind++;
                }
            }
            meshPropertiesBuffer = new ComputeBuffer(grid.totalGasCount, MeshProperties.Size());
            meshPropertiesBuffer.SetData(properties);
            material.SetBuffer("_Properties", meshPropertiesBuffer);

            uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)MeshPool.plane10.GetIndexCount(0);
            args[1] = (uint)grid.totalGasCount;
            args[2] = (uint)MeshPool.plane10.GetIndexStart(0);
            args[3] = (uint)MeshPool.plane10.GetBaseVertex(0);
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), bufferType);
            argsBuffer.SetData(args);
            Graphics.DrawMeshInstancedIndirect(MeshPool.plane10, 0, material, bounds, argsBuffer);

        }
        private static ComputeBufferType bufferType = ComputeBufferType.IndirectArguments;
        private ComputeBuffer meshPropertiesBuffer;
        private ComputeBuffer argsBuffer;
        private ComputeBuffer matrixBuffer;
        public void Initialize()
        {
            bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
        }

        private void UpdateMatrixBuffer()
        {
            if (matrixBuffer != null)
            {
                matrixBuffer.Release();
            }
            Vector3 size = new(4f, 0f, 4f);
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
        /*
CameraDriver camera = Find.CameraDriver;
currentViewRect = camera.CurrentViewRect;
currentViewRect.ClipInsideMap(map);
currentViewRect.ExpandedBy(1);
minGas = camera.rootSize * 2f;
*/

        //    Vector3 pos = p.ToVector3ShiftedWithAltitude(AltitudeLayer.Gas);
        //    pos.y += gasGrid.layer;
        //    pos.x += gasGrid.xGrid[i];
        //    pos.z += gasGrid.zGrid[i];
        //    Quaternion rotation = Quaternion.AngleAxis((float)(gasGrid.angleGrid[i] + angle + gasGrid.signGrid[i]), Vector3.up);
    }
}
