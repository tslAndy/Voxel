using Chunks;
using UndoRedoSystem;
using Unity.Mathematics;

namespace Common
{
    public class BrushComm : ICommand
    {
        public int3 pos;
        public byte matIndexFrom, matIndexTo;

        public void Redo() => VoxelManager.Instance[pos] = matIndexTo;
        public void Undo() => VoxelManager.Instance[pos] = matIndexFrom;
    }
}
