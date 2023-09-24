using System;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public sealed class BooleanFormatter : Formatter<bool>
	{
		private const int BufferLength = 6; // 5 for the longest input: false

		public static readonly BooleanFormatter Default = new(FormatterFlags.FallbackSystemType);

		public BooleanFormatter(FormatterFlags flags)
			: base(DataType.Keyword, string.Empty, flags, BufferLength)
		{
		}

		protected override bool TryImport(ReadOnlySpan<char> source, out bool value) =>
			bool.TryParse(source, out value);

		protected override bool TryExport(bool value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength);
	}
}
