using System;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace ChunksRendering
{
    public class MergedMesh
    {
        private readonly List<Vector3> _vertices;
        private readonly List<Vector3> _normals;
        private readonly List<int> _triangles;
        private readonly Mesh _mesh;

        public MergedMesh()
        {
            _mesh = new Mesh();
            _mesh.MarkDynamic();

            _vertices = new List<Vector3>();
            _normals = new List<Vector3>();
            _triangles = new List<int>();
        }

        public void Clear()
        {
            if (_triangles.Count == 0)
                return;

            _mesh.Clear();
            _vertices.Clear();
            _normals.Clear();
            _triangles.Clear();
        }

        // true if whould be rendered, false otherwise
        public bool Apply(out Mesh mesh)
        {
            if (_triangles.Count == 0)
            {
                mesh = null;
                return false;
            }

            _mesh.SetVertices(_vertices);
            _mesh.SetNormals(_normals);
            _mesh.SetTriangles(_triangles, 0);

            mesh = _mesh;
            return true;
        }

        // TODO check if transparent voxels need backface culling
        public void AddQuads(IReadOnlyList<ChunkMesh.Quad> quads, int start, int length, Vector3Int pos, Vector3 camPos, Span<long> transpBitmap)
        {
            bool checkBackface = ((transpBitmap[quads[0].matIndex / 64] >> (quads[0].matIndex % 64)) & 1) == 0;

            for (int i = 0; i < length; i++)
            {
                ChunkMesh.Quad quad = quads[start + i];

                (Vector3 norm, Vector3Int a, Vector3Int b, Vector3Int c, Vector3Int d) =
                quad.norm switch
                {

                    ChunkMesh.Norm.Up => (
                        Vector3.up,
                        new Vector3Int(0, 0, 0), // a
                        new Vector3Int(0, 0, quad.d2), // b
                        new Vector3Int(quad.d1, 0, quad.d2), // c
                        new Vector3Int(quad.d1, 0, 0)), // d

                    ChunkMesh.Norm.Down => (
                        Vector3.down,
                        new Vector3Int(quad.d1, 0, quad.d2), // c
                        new Vector3Int(0, 0, quad.d2), // b
                        new Vector3Int(0, 0, 0), // a
                        new Vector3Int(quad.d1, 0, 0) // d
                        ),

                    ChunkMesh.Norm.Left => (
                        Vector3.left,
                        new Vector3Int(0, quad.d2, quad.d1), // c
                        new Vector3Int(0, quad.d2, 0), // b
                        new Vector3Int(0, 0, 0), // a
                        new Vector3Int(0, 0, quad.d1) // d
                    ),

                    ChunkMesh.Norm.Right => (
                        Vector3.right,
                        new Vector3Int(0, 0, 0), // a
                        new Vector3Int(0, quad.d2, 0), // b
                        new Vector3Int(0, quad.d2, quad.d1), // c
                        new Vector3Int(0, 0, quad.d1) // d
                    ),

                    ChunkMesh.Norm.Forward => (
                        Vector3.forward,
                        new Vector3Int(quad.d1, quad.d2, 0), // c
                        new Vector3Int(0, quad.d2, 0), // b
                        new Vector3Int(0, 0, 0), // a
                        new Vector3Int(quad.d1, 0, 0) // d
                    ),

                    ChunkMesh.Norm.Back => (
                        Vector3.back,
                        new Vector3Int(0, 0, 0), // a
                        new Vector3Int(0, quad.d2, 0), // b
                        new Vector3Int(quad.d1, quad.d2, 0), // c
                        new Vector3Int(quad.d1, 0, 0) // d
                    ),
                    _ => throw new ArgumentException("No such normal")
                };

                Vector3Int delta = pos + new Vector3Int(quad.x, quad.y, quad.z);
                a += delta;
                b += delta;
                c += delta;
                d += delta;

                if (checkBackface && Vector3.Dot(a - camPos, norm) > 0)
                    continue;

                int t = _vertices.Count;

                _vertices.Add(a);
                _vertices.Add(b);
                _vertices.Add(c);
                _vertices.Add(d);

                _normals.Add(norm);
                _normals.Add(norm);
                _normals.Add(norm);
                _normals.Add(norm);

                _triangles.Add(t);
                _triangles.Add(t + 1);
                _triangles.Add(t + 3);

                _triangles.Add(t + 1);
                _triangles.Add(t + 2);
                _triangles.Add(t + 3);
            }
        }
    }
}