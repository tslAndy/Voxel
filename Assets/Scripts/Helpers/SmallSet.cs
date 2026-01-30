using System;
using System.Numerics;
using UnityEngine;

namespace Helpers
{
    /// <summary>
    /// Simple Set defined on stack, uses SIMD for speeding up checks
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public ref struct SmallSet<T> where T : struct, IEquatable<T>
    {
        private Span<T> _span;
        private int _count;

        public SmallSet(Span<T> mem)
        {
            _span = mem;
            _count = 0;
        }

        public int Count => _count;

        public void Add(T elem)
        {
            Vector<T> compare = new Vector<T>(elem);
            int size = Vector<T>.Count;

            int i = 0;
            for (; i < _count - size; i++)
                if (Vector.EqualsAny(compare, new Vector<T>(_span.Slice(i, size))))
                    return;

            for (; i < _count; i++)
                if (_span[i].Equals(elem))
                    return;

            _span[_count++] = elem;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || _count <= index)
                    throw new ArgumentOutOfRangeException(nameof(index), "Index out of bounds");
                return _span[index];
            }
        }
    }
}