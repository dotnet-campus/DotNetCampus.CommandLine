﻿using System;
using System.Runtime.CompilerServices;

namespace dotnetCampus.Cli.Compatibility
{
    internal static class LegacyFrameworkCompatibility
    {
#if NETCOREAPP3_0 || NETCOREAPP3_1 || NETCOREAPP5_0 || NET5_0 || NET6_0
#else
        public static int IndexOf(this string @string, char value, StringComparison stringComparison)
        {
            return @string.IndexOf(value.ToString(), stringComparison);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] EmptyArray<T>()
        {
#if NETCOREAPP3_0 || NETCOREAPP3_1 || NETCOREAPP5_0 || NET5_0 || NET6_0
            return Array.Empty<T>();
#else
            return new T[0];
#endif
        }
    }
}
