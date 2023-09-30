using System;
using System.Buffers;
using CuddleKit.Utility;

namespace CuddleKit.Reflection.Naming
{
	public sealed class UnalteredNamingConvention : INamingConvention
	{
		public static readonly UnalteredNamingConvention Shared = new();

		private UnalteredNamingConvention() {}

		SpanAllocation<char> INamingConvention.Apply(ReadOnlySpan<char> name, ArrayPool<char> pool, out ReadOnlySpan<char> result)
		{
			result = name;
			return default;
		}
	}
}
