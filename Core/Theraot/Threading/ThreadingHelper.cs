﻿// Needed for NET40

using System;
using System.Threading;

namespace Theraot.Threading
{
    [System.Diagnostics.DebuggerNonUserCode]
    public static partial class ThreadingHelper
    {
        internal const int SleepCountHint = 10;

        public static void MemoryBarrier()
        {
#if NETCOREAPP1_1
            Interlocked.MemoryBarrier();
#else
            Thread.MemoryBarrier();
#endif
        }

        internal static long Milliseconds(long ticks)
        {
            return ticks / TimeSpan.TicksPerMillisecond;
        }

        internal static long TicksNow()
        {
            return DateTime.Now.Ticks;
        }
    }
}