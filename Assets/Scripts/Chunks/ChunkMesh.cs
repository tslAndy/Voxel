using System;
using System.Collections.Generic;
using Helpers;

namespace Chunks
{
    public class ChunkMesh
    {
        private readonly List<MatRange> _ranges;
        private readonly List<Quad> _quads;
        private readonly ChunkData _chunk;

        public IReadOnlyList<MatRange> ranges => _ranges;
        public IReadOnlyList<Quad> quads => _quads;

        public ChunkMesh(ChunkData chunk)
        {
            _ranges = new List<MatRange>();
            _quads = new List<Quad>();

            _chunk = chunk;
        }

        public void Update(Span<long> transpBitmap)
        {
            _quads.Clear();
            _ranges.Clear();

            BakeUp(transpBitmap);
            BakeDown(transpBitmap);

            BakeLeft(transpBitmap);
            BakeRight(transpBitmap);

            BakeForward(transpBitmap);
            BakeBack(transpBitmap);

            UpdateRanges();
        }

        private void UpdateRanges()
        {
            _quads.Sort((a, b) => a.matIndex.CompareTo(b.matIndex));

            int i = 0;
            while (i < _quads.Count)
            {
                byte matIndex = _quads[i].matIndex;

                int j = i;
                while (j < _quads.Count && _quads[j].matIndex == matIndex)
                    j++;

                _ranges.Add(new MatRange(matIndex, i, j - i));
                i = j;
            }
        }

        private void BakeUp(Span<long> transpBitmap)
        {
            Span<byte> slice = stackalloc byte[64];
            long occlusion = 0L;
            for (int y = 7; y >= 0; y--)
            {
                for (int z = 0; z < 8; z++)
                    for (int x = 0; x < 8; x++)
                        slice[z * 8 + x] = _chunk[x, y, z];

                List<MeshBaker.Quad> sliceQuads = MeshBaker.BakeQuads(slice, transpBitmap, ref occlusion);
                for (int i = 0; i < sliceQuads.Count; i++)
                {
                    MeshBaker.Quad quad = sliceQuads[i];
                    _quads.Add(
                        new Quad
                        {
                            matIndex = quad.matIndex,
                            x = quad.c1,
                            y = (byte)(y + 1),
                            z = quad.c2,
                            d1 = quad.d1,
                            d2 = quad.d2,
                            norm = Norm.Up
                        }
                    );
                }
            }
        }

        private void BakeDown(Span<long> transpBitmap)
        {
            Span<byte> slice = stackalloc byte[64];
            long occlusion = 0L;
            for (int y = 0; y < 8; y++)
            {
                for (int z = 0; z < 8; z++)
                    for (int x = 0; x < 8; x++)
                        slice[z * 8 + x] = _chunk[x, y, z];

                List<MeshBaker.Quad> sliceQuads = MeshBaker.BakeQuads(slice, transpBitmap, ref occlusion);
                for (int i = 0; i < sliceQuads.Count; i++)
                {
                    MeshBaker.Quad quad = sliceQuads[i];
                    _quads.Add(
                        new Quad
                        {
                            matIndex = quad.matIndex,
                            x = quad.c1,
                            y = (byte)y,
                            z = quad.c2,
                            d1 = quad.d1,
                            d2 = quad.d2,
                            norm = Norm.Down
                        }
                    );
                }
            }
        }

        private void BakeLeft(Span<long> transpBitmap)
        {
            Span<byte> slice = stackalloc byte[64];
            long occlusion = 0L;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                    for (int z = 0; z < 8; z++)
                        slice[y * 8 + z] = _chunk[x, y, z];

                List<MeshBaker.Quad> sliceQuads = MeshBaker.BakeQuads(slice, transpBitmap, ref occlusion);
                for (int i = 0; i < sliceQuads.Count; i++)
                {
                    MeshBaker.Quad quad = sliceQuads[i];
                    _quads.Add(
                        new Quad
                        {
                            matIndex = quad.matIndex,
                            x = (byte)x,
                            y = quad.c2,
                            z = quad.c1,
                            d1 = quad.d1,
                            d2 = quad.d2,
                            norm = Norm.Left
                        }
                    );
                }
            }
        }

        private void BakeRight(Span<long> transpBitmap)
        {
            Span<byte> slice = stackalloc byte[64];
            long occlusion = 0L;

            for (int x = 7; x >= 0; x--)
            {
                for (int y = 0; y < 8; y++)
                    for (int z = 0; z < 8; z++)
                        slice[y * 8 + z] = _chunk[x, y, z];

                List<MeshBaker.Quad> sliceQuads = MeshBaker.BakeQuads(slice, transpBitmap, ref occlusion);
                for (int i = 0; i < sliceQuads.Count; i++)
                {
                    MeshBaker.Quad quad = sliceQuads[i];
                    _quads.Add(
                        new Quad
                        {
                            matIndex = quad.matIndex,
                            x = (byte)(x + 1),
                            y = quad.c2,
                            z = quad.c1,
                            d1 = quad.d1,
                            d2 = quad.d2,
                            norm = Norm.Right
                        }
                    );
                }
            }
        }

        private void BakeForward(Span<long> transpBitmap)
        {
            Span<byte> slice = stackalloc byte[64];
            long occlusion = 0L;

            for (int z = 7; z >= 0; z--)
            {
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                        slice[y * 8 + x] = _chunk[x, y, z];

                List<MeshBaker.Quad> sliceQuads = MeshBaker.BakeQuads(slice, transpBitmap, ref occlusion);
                for (int i = 0; i < sliceQuads.Count; i++)
                {
                    MeshBaker.Quad quad = sliceQuads[i];
                    _quads.Add(
                        new Quad
                        {
                            matIndex = quad.matIndex,
                            x = quad.c1,
                            y = quad.c2,
                            z = (byte)(z + 1),
                            d1 = quad.d1,
                            d2 = quad.d2,
                            norm = Norm.Forward
                        }
                    );
                }
            }
        }

        private void BakeBack(Span<long> transpBitmap)
        {
            Span<byte> slice = stackalloc byte[64];
            long occlusion = 0L;

            for (int z = 0; z < 8; z++)
            {
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                        slice[y * 8 + x] = _chunk[x, y, z];

                List<MeshBaker.Quad> sliceQuads = MeshBaker.BakeQuads(slice, transpBitmap, ref occlusion);
                for (int i = 0; i < sliceQuads.Count; i++)
                {
                    MeshBaker.Quad quad = sliceQuads[i];
                    _quads.Add(
                        new Quad
                        {
                            matIndex = quad.matIndex,
                            x = quad.c1,
                            y = quad.c2,
                            z = (byte)z,
                            d1 = quad.d1,
                            d2 = quad.d2,
                            norm = Norm.Back
                        }
                    );
                }
            }
        }

        public struct MatRange
        {
            public byte matIndex;
            public int start, length;

            public MatRange(byte matIndex, int start, int length)
            {
                this.matIndex = matIndex;
                this.start = start;
                this.length = length;
            }
        }

        public struct Quad
        {
            public byte matIndex;
            public byte x, y, z;
            public byte d1, d2;
            public Norm norm;
        }

        public enum Norm : byte
        {
            Up,
            Down,
            Left,
            Right,
            Forward,
            Back
        };
    }
}