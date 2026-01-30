using System.Collections.Generic;

namespace UndoRedoSystem
{
    public class CommandAccum : ICommand
    {
        private readonly List<ICommand> _commands = new List<ICommand>();

        public void AddComm(ICommand comm) => _commands.Add(comm);

        public void Redo()
        {
            for (int i = 0; i < _commands.Count; i++)
                _commands[i].Redo();
        }

        public void Undo()
        {
            for (int i = _commands.Count - 1; i >= 0; i--)
                _commands[i].Undo();
        }

        /// <summary>
        /// Used only by UndoRedoManager. If accumulator has length of 1, it's cheaper to
        /// add command from accumulator, and return list to pool for reuse
        /// </summary>
        /// <param name="comm"></param>
        /// <returns>True if accumulator has length of 1</returns>
        public bool TryPopAndClear(out ICommand comm)
        {
            if (_commands.Count > 1)
            {
                comm = null;
                return false;
            }

            comm = _commands[0];
            _commands.Clear();
            return true;
        }
    }
}