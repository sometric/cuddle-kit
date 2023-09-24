using System;
using CuddleKit.Serialization;
using CuddleKit.Utility;

namespace CuddleKit.Reflection.Naming
{
	public abstract class NamingConvention : INamingConvention
	{
		private readonly int _capacityFactor;

		protected NamingConvention(int capacityFactor = 1) =>
			_capacityFactor = capacityFactor;

		TokenReference INamingConvention.Write(ReadOnlySpan<char> name, ref Document document)
		{
			if (name.Length == 0)
				return default;

			var capacity = _capacityFactor * name.Length;
			using var allocation = SpanAllocation<char>.Retain(capacity, out var buffer);

			var length = Apply(name, buffer);

			return document.AllocateString(buffer.Slice(0, length));
		}

		protected abstract int Apply(ReadOnlySpan<char> input, Span<char> output);
	}
}
