using System;
using System.Runtime.CompilerServices;

namespace CuddleKit.Internal
{
	internal static class SpanExtensions
	{
#if !NETCOREAPP3_0_OR_GREATER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool Contains(this ReadOnlySpan<char> span, char value)
		{
			var found = false;
			for (int i = 0, length = span.Length; !found & (i < length); ++i)
				found = span[i] == value;

			return found;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<T> TrimStart<T>(this Span<T> span, T value)
			where T : struct, IEquatable<T>
		{
			var start = 0;

			for (; start < span.Length; ++start)
			{
				if (!value.Equals(span[start]))
					break;
			}

			return span.Slice(start);
		}
#endif
	}
}
