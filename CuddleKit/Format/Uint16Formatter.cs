using System;
using System.Globalization;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public sealed class Uint16Formatter : Formatter<ushort>
	{
		private const int BufferLength = 6; // 5 for the longest input: 64,535

		public static readonly Uint16Formatter Default = new(FormatterFlags.FallbackSystemType);

		private readonly IFormatProvider _provider;

		public Uint16Formatter(FormatterFlags flags, IFormatProvider provider = null)
			: base(DataType.Integer, "u16", flags, BufferLength) =>
			_provider = provider;

		protected override bool TryImport(ReadOnlySpan<char> source, out ushort value) =>
			ushort.TryParse(source, NumberStyles.Integer, _provider, out value);

		protected override bool TryExport(ushort value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength, default, _provider);
	}
}
