using System.Collections.Generic;
using Helpers;
using Unity.Mathematics;
using UnityEngine;

namespace Chunks
{
    public class VoxelManager : Singleton<VoxelManager>
    {
        public int size; // in voxels

        private Chunk[,,] _chunks;
        private Stack<Chunk> _pool;

        private void Start()
        {
            _chunks = new Chunk[size / 8, size / 8, size / 8];
            _pool = new Stack<Chunk>();
        }

        public Chunk GetChunk(int x, int y, int z) => _chunks[x, y, z];


        public byte this[int x, int y, int z] // voxel coord
        {
            get
            {
                Chunk chunk = _chunks[x / 8, y / 8, z / 8];
                return chunk == null ? default : chunk[x % 8, y % 8, z % 8];
            }

            set
            {
                Chunk chunk = _chunks[x / 8, y / 8, z / 8];
                if (chunk == null)
                {
                    if (!_pool.TryPop(out chunk))
                        chunk = new Chunk();

                    _chunks[x / 8, y / 8, z / 8] = chunk;
                }

                chunk[x % 8, y % 8, z % 8] = value;
                if (chunk.count == 0)
                {
                    _pool.Push(chunk);
                    _chunks[x / 8, y / 8, z / 8] = null;
                }
            }
        }

        public byte this[int3 ind]
        {
            get => this[ind.x, ind.y, ind.z];
            set => this[ind.x, ind.y, ind.z] = value;
        }

    }
}