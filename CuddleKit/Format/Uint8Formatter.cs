using System;
using System.Globalization;

namespace CuddleKit.Format
{
	using ValueType = Serialization.ValueType;

	public sealed class Uint8Formatter : Formatter<byte>
	{
		private const int BufferLength = 4; // 3 for the longest input: 255

		public static readonly Uint8Formatter Default = new(FormatterFlags.FallbackSystemType);

		private readonly IFormatProvider _provider;

		public Uint8Formatter(FormatterFlags flags, IFormatProvider provider = null)
			: base(ValueType.Integer, "u8", flags, BufferLength) =>
			_provider = provider;

		protected override bool TryImport(ReadOnlySpan<char> source, out byte value) =>
			byte.TryParse(source, NumberStyles.Integer, _provider, out value);

		protected override bool TryExport(byte value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength, default, _provider);
	}
}
