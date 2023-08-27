using System;

namespace CuddleKit.Format
{
	using ValueType = Serialization.ValueType;

	public sealed class GuidFormatter : Formatter<Guid>
	{
		public static readonly GuidFormatter Default = new(FormatterFlags.FallbackSystemType);

		public GuidFormatter(FormatterFlags flags) : base(ValueType.String, "uuid", flags, 40)
		{
		}

		protected override bool TryImport(ReadOnlySpan<char> source, out Guid value) =>
			Guid.TryParse(source, out value);

		protected override bool TryExport(Guid value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength);
	}
}
