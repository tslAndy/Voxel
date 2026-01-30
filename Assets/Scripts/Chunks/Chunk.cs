using System;
using System.Collections.Generic;

namespace Chunks
{
    public class Chunk
    {
        private readonly ChunkData _data;
        private readonly ChunkMesh _mesh;
        private bool _dirty;

        public IReadOnlyList<ChunkMesh.MatRange> ranges => _mesh.ranges;
        public IReadOnlyList<ChunkMesh.Quad> quads => _mesh.quads;
        public int count => _data.count;

        public Chunk()
        {
            _data = new ChunkData();
            _mesh = new ChunkMesh(_data);
        }

        public void Update(Span<long> transpBitmap)
        {
            if (!_dirty)
                return;

            _dirty = false;
            _mesh.Update(transpBitmap);
        }

        public byte this[int x, int y, int z]
        {
            get => _data[x, y, z];
            set
            {
                _dirty = true;
                _data[x, y, z] = value;
            }
        }
    }
}
