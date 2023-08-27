using System;
using System.Globalization;

namespace CuddleKit.Format
{
	using ValueType = Serialization.ValueType;

	public sealed class Int64Formatter : Formatter<int>
	{
		private const int BufferLength = 20; // 19 for the longest input: 9,223,372,036,854,775,807

		public static readonly Int64Formatter Default = new(FormatterFlags.FallbackSystemType);

		private readonly IFormatProvider _provider;

		public Int64Formatter(FormatterFlags flags, IFormatProvider provider = null)
			: base(ValueType.Integer, "i64", flags, BufferLength) =>
			_provider = provider;

		protected override bool TryImport(ReadOnlySpan<char> source, out int value) =>
			int.TryParse(source, NumberStyles.Integer, _provider, out value);

		protected override bool TryExport(int value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength, default, _provider);
	}
}
