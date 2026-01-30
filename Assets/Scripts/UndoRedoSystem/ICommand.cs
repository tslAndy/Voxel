namespace UndoRedoSystem
{
    public interface ICommand
    {
        void Undo();
        void Redo();
    }
}
