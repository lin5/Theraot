﻿#if FAT

using System;
using System.Collections.Generic;
using System.Threading;
using Theraot.Core;
using Theraot.Threading;
using Theraot.Threading.Needles;

namespace Theraot.Collections.ThreadSafe
{
    internal static class ArrayPool<T>
    {
        private const int INT_DirtyCapacityHint = 128;
        private const int INT_MaxCapacity = 1024;
        private const int INT_MinCapacity = 16;
        private const int INT_PoolSize = 16;

        private static readonly NeedleBucket<FixedSizeQueueBucket<ArrayHolder>, LazyNeedle<FixedSizeQueueBucket<ArrayHolder>>> _data;
        private static readonly List<ArrayHolder> _dirtyData;
        private static readonly ReentryGuard _guard;
        private static readonly LazyNeedle<Work> _recycle;
        private static int _done;

        static ArrayPool()
        {
            _recycle = new LazyNeedle<Work>(() => WorkContext.DefaultContext.AddWork(CleanUp));
            _data = new NeedleBucket<FixedSizeQueueBucket<ArrayHolder>, LazyNeedle<FixedSizeQueueBucket<ArrayHolder>>>
            (
                _ => new FixedSizeQueueBucket<ArrayHolder>(INT_PoolSize),
                NumericHelper.Log2(INT_MaxCapacity) - NumericHelper.Log2(INT_MinCapacity)
            );
            _dirtyData = new List<ArrayHolder>(INT_DirtyCapacityHint);
            _guard = new ReentryGuard();
            Thread.MemoryBarrier();
            Thread.VolatileWrite(ref _done, 1);
        }

        internal static bool DonateArray(T[] array)
        {
            if (ReferenceEquals(array, null) || GCMonitor.FinalizingForUnload)
            {
                return false;
            }
            else
            {
                int capacity = array.Length;
                if (NumericHelper.PopulationCount(capacity) == 1)
                {
                    if (capacity < INT_MinCapacity || capacity > INT_MaxCapacity)
                    {
                        //Rejected
                        return false;
                    }
                    else
                    {
                        var ersatz = new ErsatzAction<T[]>(DonateArrayExtracted, array);
                        _guard.Execute(ersatz.Invoke);
                        return true;
                    }
                }
                else
                {
                    throw new ArgumentException("The size of the array must be a power of two.", "array");
                }
            }
        }

        internal static T[] GetArray(int capacity)
        {
            if (capacity == 0)
            {
                return ArrayReservoir<T>.EmptyArray;
            }
            else if (capacity < INT_MinCapacity)
            {
                capacity = INT_MinCapacity;
            }
            else
            {
                if (capacity > INT_MaxCapacity)
                {
                    //Too big to leak it
                    return new T[capacity];
                }
                else
                {
                    capacity = NumericHelper.PopulationCount(capacity) == 1 ? capacity : NumericHelper.NextPowerOf2(capacity);
                }
            }
            if (Thread.VolatileRead(ref _done) == 1 && !_guard.IsTaken)
            {
                var promise = _guard.Execute(() => GetArrayExtracted(capacity));
                return promise.Value;
            }
            else
            {
                return new T[capacity];
            }
        }

        private static void CleanUp()
        {
            do
            {
                foreach (var item in _dirtyData.RemoveWhereEnumerable(FuncHelper.GetTautologyPredicate<ArrayHolder>()))
                {
                    item.ClearIfNeeded();
                }
            } while (_dirtyData.Count > 0);
        }

        private static void DonateArrayExtracted(T[] array)
        {
            int index = GetIndex(array.Length);
            if (index < _data.Capacity)
            {
                var bucket = _data.Get(index);
                var holder = new ArrayHolder(array);
                bucket.Add(holder);
                _dirtyData.Add(holder);
                _recycle.Value.Start();
            }
        }

        private static T[] GetArrayExtracted(int capacity)
        {
            int index = GetIndex(capacity);
            if (index < _data.Capacity)
            {
                var bucket = _data.Get(index);
                ArrayHolder result;
                if (bucket.TryTake(out result))
                {
                    return result.Array;
                }
            }
            return new T[capacity];
        }

        private static int GetIndex(int capacity)
        {
            return NumericHelper.Log2(capacity) - NumericHelper.Log2(INT_MinCapacity);
        }

        private class ArrayHolder
        {
            private readonly T[] _array;
            private int _dirty;

            public ArrayHolder(T[] array)
            {
                _array = array;
                _dirty = 1;
            }

            public T[] Array
            {
                get
                {
                    ClearIfNeeded();
                    return _array;
                }
            }

            public void ClearIfNeeded()
            {
                if (Interlocked.CompareExchange(ref _dirty, 0, 1) == 1)
                {
                    System.Array.Clear(_array, 0, _array.Length);
                }
            }
        }
    }
}

#endif