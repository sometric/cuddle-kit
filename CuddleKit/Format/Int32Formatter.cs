using System;
using System.Globalization;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public sealed class Int32Formatter : Formatter<int>
	{
		private const int BufferLength = 11; // 10 for the longest input: 2,147,483,647

		public static readonly Int32Formatter Default =
			new(FormatterFlags.FallbackSystemType | FormatterFlags.FallbackDocumentType);

		private readonly IFormatProvider _provider;

		public Int32Formatter(FormatterFlags flags, IFormatProvider provider = null)
			: base(DataType.Integer, "i32", flags, BufferLength) =>
			_provider = provider;

		protected override bool TryImport(ReadOnlySpan<char> source, out int value) =>
			int.TryParse(source, NumberStyles.Integer, _provider, out value);

		protected override bool TryExport(int value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength, default, _provider);
	}
}
