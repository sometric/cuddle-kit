using System;
using System.Buffers;
using CuddleKit.Utility;

namespace CuddleKit.Reflection.Naming
{
	public interface INamingConvention
	{
		SpanAllocation<char> Apply(ReadOnlySpan<char> name, ArrayPool<char> pool, out ReadOnlySpan<char> result);

		SpanAllocation<char> Apply(ReadOnlySpan<char> name, out ReadOnlySpan<char> result) =>
			Apply(name, ArrayPool<char>.Shared, out result);
	}
}
