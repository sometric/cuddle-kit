using System;

namespace CuddleKit.Reflection.Naming
{
	public class SeparatedNamingConvention : NamingConvention
	{
		private readonly string _separator;

		protected SeparatedNamingConvention(string separator) : base(separator.Length + 1) =>
			_separator = separator;

		protected override int Apply(ReadOnlySpan<char> input, Span<char> output)
		{
			var offset = 0;
			var word = TakeWord(input);

			var separator = _separator.AsSpan();
			var prefix = ReadOnlySpan<char>.Empty;

			while (!word.IsEmpty)
			{
				prefix.CopyTo(output.Slice(offset));
				offset += prefix.Length;

				word.CopyTo(output.Slice(offset));
				offset += word.Length;

				input = input.Slice(word.Length);
				word = TakeWord(input);
				prefix = separator;
			}

			return offset;
		}

		private static ReadOnlySpan<char> TakeWord(ReadOnlySpan<char> input)
		{
			for (int i = 1, length = input.Length; i < length; ++i)
			{
				if (char.IsUpper(input[i]))
					return input.Slice(0, i);
			}

			return input;
		}
	}
}
