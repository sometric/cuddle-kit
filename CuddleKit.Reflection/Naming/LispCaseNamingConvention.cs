using System;
using CuddleKit.Reflection.Utility;
using CuddleKit.Utility;

namespace CuddleKit.Reflection.Naming
{
	public sealed class LispCaseNamingConvention : SeparatedNamingConvention
	{
		public static readonly LispCaseNamingConvention Shared = new();

		private LispCaseNamingConvention() : base("-") {}

		protected override int Apply(ReadOnlySpan<char> input, Span<char> output)
		{
			using var allocation = SpanAllocation<char>.Retain(output.Length, out var intermediateOutput);
			var length = base.Apply(input, intermediateOutput);
			return intermediateOutput.ReadonlySlice(0, length).ToLowerInvariant(output);
		}
	}
}
