using System;
using System.Globalization;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public sealed class Uint64Formatter : Formatter<ulong>
	{
		private const int BufferLength = 20; // 19 for the longest input: 9,223,372,036,854,775,807

		public static readonly Uint64Formatter Default = new(FormatterFlags.FallbackSystemType);

		private readonly IFormatProvider _provider;

		public Uint64Formatter(FormatterFlags flags, IFormatProvider provider = null)
			: base(DataType.Integer, "u64", flags, BufferLength) =>
			_provider = provider;

		protected override bool TryImport(ReadOnlySpan<char> source, out ulong value) =>
			ulong.TryParse(source, NumberStyles.Integer, _provider, out value);

		protected override bool TryExport(ulong value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength, default, _provider);
	}
}
