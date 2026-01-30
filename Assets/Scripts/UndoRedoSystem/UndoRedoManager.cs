using System;
using System.Collections.Generic;
using Helpers;

namespace UndoRedoSystem
{
    public class UndoRedoManager : Singleton<UndoRedoManager>
    {
        private readonly Dictionary<Type, Stack<object>> pool = new Dictionary<Type, Stack<object>>();
        private readonly List<ICommand> comms = new List<ICommand>();

        private int index;

        private const int MaxCommandCount = 127;

        private void ReturnToPool(ICommand comm)
        {
            if (!pool.TryGetValue(comm.GetType(), out Stack<object> stack))
            {
                stack = new Stack<object>();
                pool.Add(comm.GetType(), stack);
            }

            stack.Push(comm);
        }


        public void AddCommand(ICommand command)
        {
            if (command is CommandAccum accum && accum.TryPopAndClear(out ICommand temp))
            {
                ReturnToPool(command);
                command = temp;
            }

            if (index < comms.Count - 1)
            {
                for (int i = index + 1; i < comms.Count; i++)
                    ReturnToPool(comms[i]);
                comms.RemoveRange(index + 1, comms.Count - index - 1);
            }

            if (index == MaxCommandCount)
            {
                ICommand comm = comms[0];
                comms.RemoveAt(0);
                ReturnToPool(comm);
            }

            comms.Add(command);
            index++;
        }

        public void Undo()
        {
            if (comms.Count == 0 || index == -1)
                return;

            if (index == comms.Count)
                index = comms.Count - 1;

            comms[index--].Undo();
        }

        public void Redo()
        {
            if (comms.Count == 0 || index + 1 >= comms.Count)
                return;

            comms[++index].Redo();
        }

        /// <summary>
        /// Returns command from pool, commands should be requested only by this method,
        /// not by direct construction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Command</returns>
        public T GetCommand<T>() where T : ICommand, new()
        {
            if (!pool.TryGetValue(typeof(T), out Stack<object> stack))
            {
                stack = new Stack<object>();
                pool.Add(typeof(T), stack);
            }

            if (stack.TryPop(out object commandObj))
                return (T)commandObj;
            return new T();
        }
    }
}
