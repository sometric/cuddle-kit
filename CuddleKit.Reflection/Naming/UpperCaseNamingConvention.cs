using System;

namespace CuddleKit.Reflection.Naming
{
	public sealed class UpperCaseNamingConvention : NamingConvention
	{
		public static readonly UpperCaseNamingConvention Shared = new();

		private UpperCaseNamingConvention() {}

		protected override int Apply(ReadOnlySpan<char> input, Span<char> output) =>
			input.ToUpperInvariant(output);
	}
}
