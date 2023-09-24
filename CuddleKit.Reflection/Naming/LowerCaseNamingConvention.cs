using System;

namespace CuddleKit.Reflection.Naming
{
	public sealed class LowerCaseNamingConvention : NamingConvention
	{
		public static readonly LowerCaseNamingConvention Shared = new();

		private LowerCaseNamingConvention() {}

		protected override int Apply(ReadOnlySpan<char> input, Span<char> output) =>
			input.ToLowerInvariant(output);
	}
}
