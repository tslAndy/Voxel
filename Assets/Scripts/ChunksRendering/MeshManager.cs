using System;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace ChunksRendering
{
    public class MeshManager : MonoBehaviour
    {
        [SerializeField] private Material opaqEmisMat, opaqNonEmisMat, transpEmisMat, transpNonEmisMat;
        [SerializeField] private ChunkOccluder occluder;
        [SerializeField] private Transform camTrs;

        private readonly List<MeshMatPair> _meshMatPairs = new List<MeshMatPair>();

        public int CreateMatProp()
        {
            MeshMatPair pair = new MeshMatPair();
            pair.matProp.name = $"Material {_meshMatPairs.Count + 1}";

            _meshMatPairs.Add(pair);
            return _meshMatPairs.Count;
        }

        public MatProp GetMatProp(int index) => _meshMatPairs[index - 1].matProp;


        private void Update()
        {
            // bitmap where bit and index of material corresponds to it's transparency
            Span<long> transpBitmap = stackalloc long[_meshMatPairs.Count / 64 + ((_meshMatPairs.Count % 64 == 0) ? 0 : 1)];
            transpBitmap.Fill(0);
            for (int i = 0; i < _meshMatPairs.Count; i++)
            {
                if ((_meshMatPairs[i].matProp.matType & MatProp.MatType.Transparent) != 0)
                    transpBitmap[(i + 1) / 64] |= 1L << ((i + 1) % 64);
            }

            for (int i = 0; i < _meshMatPairs.Count; i++)
                _meshMatPairs[i].mergedMesh.Clear();

            for (int i = 0; i < occluder.Count; i++)
            {
                Vector3Int pos = occluder[i];

                Chunk chunk = VoxelManager.Instance.GetChunk(pos.x, pos.y, pos.z);
                chunk.Update(transpBitmap);

                for (int j = 0; j < chunk.ranges.Count; j++)
                {
                    ChunkMesh.MatRange range = chunk.ranges[j];
                    MeshMatPair pair = _meshMatPairs[range.matIndex - 1];

                    pair.mergedMesh.AddQuads(chunk.quads, range.start, range.length, pos * 8, camTrs.position, transpBitmap);
                }
            }

            for (int i = 0; i < _meshMatPairs.Count; i++)
            {
                MeshMatPair pair = _meshMatPairs[i];
                if (!pair.mergedMesh.Apply(out Mesh msh))
                    continue;

                Material mat = pair.matProp.matType switch
                {
                    MatProp.MatType.Opaque => opaqNonEmisMat,
                    MatProp.MatType.OpaqueEmissive => opaqEmisMat,
                    MatProp.MatType.Transparent => transpNonEmisMat,
                    MatProp.MatType.TransparentEmissive => transpEmisMat,
                    _ => null
                };

                Graphics.DrawMesh(msh, Matrix4x4.identity, mat, 0, null, 0, pair.matProp.propBlock);
            }
        }
    }
}