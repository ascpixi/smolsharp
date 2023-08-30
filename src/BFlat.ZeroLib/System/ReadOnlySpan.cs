// bflat minimal runtime library
// Copyright (C) 2021-2022 Michal Strehovsky
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    public readonly ref struct ReadOnlySpan<T>
    {
        private readonly ref T _reference;
        public readonly int Length;

        public ReadOnlySpan(in T element)
        {
            _reference = element;
            Length = 1;
        }

        public ReadOnlySpan(T[] array)
        {
            if (array == null) {
                this = default;
                return;
            }

            _reference = ref MemoryMarshal.GetArrayDataReference(array);
            Length = array.Length;
        }

        public ReadOnlySpan(T[] array, int start, int length)
        {

        }

        public unsafe ReadOnlySpan(void* pointer, int length)
        {
            _reference = ref Unsafe.As<byte, T>(ref *(byte*)pointer);
            Length = length;
        }

        public unsafe T* AsPointer()
        {
            return (T*)Unsafe.AsPointer(ref _reference);
        }

        public ref readonly T this[int index]
        {
            [Intrinsic]
            get
            {
                if ((uint)index >= (uint)Length)
                    Environment.FailFast(null);
                return ref Unsafe.Add(ref _reference, (nint)(uint)index);
            }
        }

        public static implicit operator ReadOnlySpan<T>(T[] array) => new ReadOnlySpan<T>(array);
    }
}
