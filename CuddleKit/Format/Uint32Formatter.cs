using System;
using System.Globalization;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public sealed class Uint32Formatter : Formatter<uint>
	{
		private const int BufferLength = 11; // 10 for the longest input: 4,294,967,295

		public static readonly Uint32Formatter Default = new(FormatterFlags.FallbackSystemType);

		private readonly IFormatProvider _provider;

		public Uint32Formatter(FormatterFlags flags, IFormatProvider provider = null)
			: base(DataType.Integer, "u32", flags, BufferLength) =>
			_provider = provider;

		protected override bool TryImport(ReadOnlySpan<char> source, out uint value) =>
			uint.TryParse(source, NumberStyles.Integer, _provider, out value);

		protected override bool TryExport(uint value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength, default, _provider);
	}
}
