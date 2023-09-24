using System;

namespace CuddleKit.Reflection.Naming
{
	public sealed class PascalCaseNamingConvention : NamingConvention
	{
		public static readonly PascalCaseNamingConvention Shared = new();

		private PascalCaseNamingConvention() {}

		protected override int Apply(ReadOnlySpan<char> input, Span<char> output)
		{
			input.CopyTo(output);
			output[0] = char.ToUpperInvariant(input[0]);
			return input.Length;
		}
	}
}
