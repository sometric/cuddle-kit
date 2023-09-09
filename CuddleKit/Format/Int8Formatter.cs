using System;
using System.Globalization;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public sealed class Int8Formatter : Formatter<sbyte>
	{
		private const int BufferLength = 4; // 3 for the longest input: 127

		public static readonly Int8Formatter Default = new(FormatterFlags.FallbackSystemType);

		private readonly IFormatProvider _provider;

		public Int8Formatter(FormatterFlags flags, IFormatProvider provider = null)
			: base(DataType.Integer, "i8", flags, BufferLength) =>
			_provider = provider;

		protected override bool TryImport(ReadOnlySpan<char> source, out sbyte value) =>
			sbyte.TryParse(source, NumberStyles.Integer, _provider, out value);

		protected override bool TryExport(sbyte value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength, default, _provider);
	}
}
