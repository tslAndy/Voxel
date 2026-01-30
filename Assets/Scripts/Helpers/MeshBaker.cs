using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Helpers
{
    public static class MeshBaker
    {
        private static readonly List<Quad> temp = new List<Quad>();

        public static List<Quad> BakeQuads(Span<byte> slice, Span<long> transpBitmap, ref long occlusion)
        {
            temp.Clear();

            Span<byte> span = stackalloc byte[64];
            SmallSet<byte> matSet = new SmallSet<byte>(span);

            long map = ~0L;
            for (int i = 0; i < 64; i++)
            {
                byte vox = slice[i];
                if (vox == 0)
                    continue;

                matSet.Add(vox);
                if (((transpBitmap[vox >> 6] >> (vox & 63)) & 1) == 0)
                    map &= ~(1L << i);
            }

            occlusion &= ~map;
            map |= occlusion;

            for (int i = 0; i < matSet.Count; i++)
            {
                byte matIndex = matSet[i];

                long tempMap = ~0L;
                for (int j = 0; j < 64; j++)
                    if (slice[j] == matIndex)
                        tempMap &= ~(1L << j);
                tempMap |= occlusion;

                for (int y = 0; y < 8; y++)
                {
                    int x = 0;
                    long line = ((tempMap >> (y << 3)) & 255) | (-256L);

                    while (line != ~0L)
                    {
                        while ((line & 1) == 1)
                        {
                            x++;
                            line >>= 1;
                        }

                        int count = math.tzcnt(line);
                        line >>= count;
                        long mask = ((1 << count) - 1) << x;

                        int ty = y + 1;
                        for (; ty < 8; ty++)
                        {
                            if (((tempMap >> (ty << 3)) & 255 & mask) != 0)
                                break;
                            tempMap |= mask << (ty << 3);
                        }

                        temp.Add(new Quad
                        {
                            c1 = (byte)x,
                            c2 = (byte)y,
                            d1 = (byte)count,
                            d2 = (byte)(ty - y),
                            matIndex = matIndex
                        });

                        x += count;
                    }
                }
            }

            occlusion |= ~map;
            return temp;
        }

        public struct Quad
        {
            public byte c1, c2, d1, d2, matIndex;
        }
    }
}