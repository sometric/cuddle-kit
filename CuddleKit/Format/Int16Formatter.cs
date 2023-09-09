using System;
using System.Globalization;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public sealed class Int16Formatter : Formatter<short>
	{
		private const int BufferLength = 6; // 5 for the longest input: 32,767

		public static readonly Int16Formatter Default = new(FormatterFlags.FallbackSystemType);

		private readonly IFormatProvider _provider;

		public Int16Formatter(FormatterFlags flags, IFormatProvider provider = null)
			: base(DataType.Integer, "i16", flags, BufferLength) =>
			_provider = provider;

		protected override bool TryImport(ReadOnlySpan<char> source, out short value) =>
			short.TryParse(source, NumberStyles.Integer, _provider, out value);

		protected override bool TryExport(short value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength, default, _provider);
	}
}
