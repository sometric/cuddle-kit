using System;

namespace CuddleKit.Reflection.Naming
{
	public sealed class CamelCaseNamingConvention : NamingConvention
	{
		public static readonly CamelCaseNamingConvention Shared = new();

		private CamelCaseNamingConvention() {}

		protected override int Apply(ReadOnlySpan<char> input, Span<char> output)
		{
			input.CopyTo(output);
			output[0] = char.ToLowerInvariant(input[0]);
			return input.Length;
		}
	}
}
