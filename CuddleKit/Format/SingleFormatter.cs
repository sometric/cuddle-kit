using System;
using System.Globalization;

namespace CuddleKit.Format
{
	using ValueType = Serialization.ValueType;

	public sealed partial class SingleFormatter : Formatter<float>
	{
		private const int BufferLength = 113 + 1; // 112 for the longest input + 1 for rounding: 1.40129846E-45

		private static readonly IFormatProvider DefaultProvider =
			CultureInfo.CreateSpecificCulture("en-US");

		public static readonly SingleFormatter Default =
			new(FormatterFlags.FallbackSystemType | FormatterFlags.FallbackDocumentType);

		private readonly IFormatProvider _provider;

		public SingleFormatter(FormatterFlags flags, IFormatProvider provider = null)
			: base(ValueType.Real, "f32", flags, BufferLength) =>
			_provider = provider ?? DefaultProvider;

		protected override bool TryImport(ReadOnlySpan<char> source, out float value) =>
			float.TryParse(source, NumberStyles.Number, _provider, out value);

		protected override bool TryExport(float value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength, null, _provider);
	}
}
