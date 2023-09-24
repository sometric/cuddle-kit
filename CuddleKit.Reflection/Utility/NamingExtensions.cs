using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CuddleKit.Reflection.Utility
{
	internal static class NamingExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpan<T> ReadonlySlice<T>(this Span<T> span, int start, int length) =>
			span.Slice(start, length);

		public static ReadOnlySpan<char> SkipPrefixes(this ReadOnlySpan<char> input, IReadOnlyList<string> prefixes)
		{
			for (int i = 0, length = prefixes.Count; i < length; ++i)
			{
				var prefix = prefixes[i].AsSpan();
				if (input.StartsWith(prefix))
					return input.Slice(prefix.Length);
			}

			return input;
		}
	}
}
