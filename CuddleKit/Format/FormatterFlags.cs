using System;

namespace CuddleKit.Format
{
	[Flags]
	public enum FormatterFlags
	{
		FallbackSystemType = 0b1,
		FallbackDocumentType = 0b10,
		ForceExportAnnotations = 0b100
	}
}
