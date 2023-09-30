using System;
using System.Buffers;
using CuddleKit.Utility;

namespace CuddleKit.Reflection.Naming
{
	public abstract class NamingConvention : INamingConvention
	{
		private readonly int _capacityFactor;

		protected NamingConvention(int capacityFactor = 1) =>
			_capacityFactor = capacityFactor;

		SpanAllocation<char> INamingConvention.Apply(ReadOnlySpan<char> name, ArrayPool<char> pool, out ReadOnlySpan<char> result)
		{
			if (name.Length == 0)
			{
				result = default;
				return default;
			}

			var capacity = _capacityFactor * name.Length;
			var allocation = SpanAllocation<char>.Retain(pool, capacity, out var buffer);
			try
			{
				var length = Apply(name, buffer);
				result = buffer.Slice(0, length);
				return allocation;
			}
			catch
			{
				allocation.Dispose();
				throw;
			}
		}

		protected abstract int Apply(ReadOnlySpan<char> input, Span<char> output);
	}
}
