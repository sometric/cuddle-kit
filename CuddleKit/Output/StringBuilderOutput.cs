using System;
using System.Text;

namespace CuddleKit.Output
{
	public readonly struct StringBuilderOutput : IDocumentOutput
	{
		private readonly StringBuilder _stringBuilder;

		public StringBuilderOutput(StringBuilder stringBuilder) =>
			_stringBuilder = stringBuilder;

		public static implicit operator StringBuilderOutput(StringBuilder stringBuilder) =>
			new(stringBuilder);

		public void Write(ReadOnlySpan<char> value) =>
			_stringBuilder?.Append(value);
	}
}
