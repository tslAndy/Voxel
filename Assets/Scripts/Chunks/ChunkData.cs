using Helpers;

namespace Chunks
{
    public class ChunkData
    {
        public int count { get; private set; }
        private readonly byte[] _voxels = new byte[8 * 8 * 8];

        public byte this[int x, int y, int z]
        {
            get => _voxels[MortonUtil.GetMorton(x, y, z)];
            set
            {
                int id = MortonUtil.GetMorton(x, y, z);
                count += (((-value) >> 31) & 1) - (((-_voxels[id]) >> 31) & 1);
                _voxels[id] = value;
            }
        }
    }
}