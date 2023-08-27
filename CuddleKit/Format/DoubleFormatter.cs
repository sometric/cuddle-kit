using System;
using System.Globalization;

namespace CuddleKit.Format
{
	using ValueType = Serialization.ValueType;

	public sealed class DoubleFormatter : Formatter<double>
	{
		private const int BufferLength = 768 + 1; // 767 for the longest input + 1 for rounding: 4.9406564584124654E-324

		private static readonly IFormatProvider DefaultProvider =
			CultureInfo.CreateSpecificCulture("en-US");

		public static readonly DoubleFormatter Default = new(FormatterFlags.FallbackSystemType);

		private readonly IFormatProvider _provider;

		public DoubleFormatter(FormatterFlags flags, IFormatProvider provider = null)
			: base(ValueType.Real, "f64", flags, BufferLength) =>
			_provider = provider ?? DefaultProvider;

		protected override bool TryImport(ReadOnlySpan<char> source, out double value) =>
			double.TryParse(source, NumberStyles.Number, _provider, out value);

		protected override bool TryExport(double value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength, null, _provider);
	}
}
