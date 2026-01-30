using System;
using System.Collections.Generic;
using Chunks;
using UnityEngine;

namespace ChunksRendering
{
    public class ChunkOccluder : MonoBehaviour
    {
        [SerializeField] private Camera cam;

        private readonly List<Vector3Int> _chunks = new List<Vector3Int>(); // positions of chunk aren't global, divided by 8
        private readonly Plane[] _planes = new Plane[6];
        private readonly Stack<Node> _stack = new Stack<Node>();

        public int Count => _chunks.Count;
        public Vector3Int this[int index] => _chunks[index];


        private void Update()
        {
            GeometryUtility.CalculateFrustumPlanes(cam, _planes);

            _chunks.Clear();
            _stack.Push(new Node(0, 0, 0, VoxelManager.Instance.size));

            while (_stack.Count != 0)
            {
                Node node = _stack.Pop();
                OcclusionCheck check = CheckNode(node);

                Vector3Int pos = new Vector3Int(node.x / 8, node.y / 8, node.z / 8);

                if (node.s == 8)
                {
                    if (check != OcclusionCheck.None && VoxelManager.Instance.GetChunk(pos.x, pos.y, pos.z) != null)
                        _chunks.Add(pos);
                    continue;
                }

                if (check == OcclusionCheck.Full)
                {
                    int s = node.s / 8;
                    for (int x = pos.x; x < pos.x + s; x++)
                        for (int y = pos.y; y < pos.y + s; y++)
                            for (int z = pos.z; z < pos.z + s; z++)
                                if (VoxelManager.Instance.GetChunk(x, y, z) != null)
                                    _chunks.Add(new Vector3Int(x, y, z));
                    continue;
                }

                int hs = node.s / 2;
                _stack.Push(new Node(node.x, node.y, node.z, hs));
                _stack.Push(new Node(node.x + hs, node.y, node.z, hs));
                _stack.Push(new Node(node.x, node.y, node.z + hs, hs));
                _stack.Push(new Node(node.x + hs, node.y, node.z + hs, hs));
                _stack.Push(new Node(node.x, node.y + hs, node.z, hs));
                _stack.Push(new Node(node.x + hs, node.y + hs, node.z, hs));
                _stack.Push(new Node(node.x, node.y + hs, node.z + hs, hs));
                _stack.Push(new Node(node.x + hs, node.y + hs, node.z + hs, hs));
            }
        }

        private OcclusionCheck CheckNode(Node node)
        {
            Span<Vector3> points = stackalloc Vector3[8]
            {
            new Vector3(node.x, node.y, node.z),
            new Vector3(node.x + node.s, node.y, node.z),
            new Vector3(node.x, node.y, node.z + node.s),
            new Vector3(node.x + node.s, node.y, node.z + node.s),
            new Vector3(node.x, node.y + node.s, node.z),
            new Vector3(node.x + node.s, node.y + node.s, node.z),
            new Vector3(node.x, node.y + node.s, node.z + node.s),
            new Vector3(node.x + node.s, node.y + node.s, node.z + node.s),
        };

            int count = 0;
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 point = points[i];

                int j;
                for (j = 0; j < _planes.Length; j++)
                    if (_planes[j].GetDistanceToPoint(point) < 0)
                        break;

                if (j < 6 && count > 0)
                    return OcclusionCheck.Partial;

                if (j == 6)
                    count++;
            }

            if (count == 0)
                return OcclusionCheck.None;
            if (count == 8)
                return OcclusionCheck.Full;
            return OcclusionCheck.Partial;
        }

        private enum OcclusionCheck
        {
            None,
            Partial,
            Full
        }

        private struct Node
        {
            public int x, y, z, s;

            public Node(int x, int y, int z, int s)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.s = s;
            }
        }
    }
}