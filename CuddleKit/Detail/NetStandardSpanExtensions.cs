namespace CuddleKit.Detail
{
#if !NETCOREAPP3_0_OR_GREATER
	using System.Runtime.CompilerServices;

	internal static class NetStandardSpanExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Contains(this System.ReadOnlySpan<char> span, char value)
		{
			var found = false;
			for (int i = 0, length = span.Length; !found & (i < length); ++i)
				found = span[i] == value;

			return found;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.Span<T> TrimStart<T>(this System.Span<T> span, T value)
			where T : struct, System.IEquatable<T>
		{
			var start = 0;

			for (; start < span.Length; ++start)
			{
				if (!value.Equals(span[start]))
					break;
			}

			return span.Slice(start);
		}
	}
#endif
}
