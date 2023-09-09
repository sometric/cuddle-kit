using System;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public sealed class GuidFormatter : Formatter<Guid>
	{
		public static readonly GuidFormatter Default = new(FormatterFlags.FallbackSystemType);

		public GuidFormatter(FormatterFlags flags) : base(DataType.String, "uuid", flags, 40)
		{
		}

		protected override bool TryImport(ReadOnlySpan<char> source, out Guid value) =>
			Guid.TryParse(source, out value);

		protected override bool TryExport(Guid value, Span<char> destination, out int exportedLength) =>
			value.TryFormat(destination, out exportedLength);
	}
}
