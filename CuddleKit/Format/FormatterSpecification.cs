using System.Runtime.CompilerServices;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public readonly struct FormatterSpecification
	{
		public readonly System.Type SystemType;
		public readonly ValueType DocumentType;
		public readonly string Annotation;
		public readonly FormatterFlags Flags;

		public FormatterSpecification(
			System.Type systemType,
			ValueType documentType,
			string annotation,
			FormatterFlags flags) =>
			(SystemType, DocumentType, Annotation, Flags) = (systemType, documentType, annotation, flags);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasFlag(FormatterFlags flags) =>
			(Flags & flags) == flags;
	}
}
