using System.Runtime.CompilerServices;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public readonly struct FormatterSpecification
	{
		public readonly System.Type SystemType;
		public readonly DataType DataType;
		public readonly string Annotation;
		public readonly FormatterFlags Flags;

		public FormatterSpecification(
			System.Type systemType,
			DataType dataType,
			string annotation,
			FormatterFlags flags) =>
			(SystemType, DataType, Annotation, Flags) = (systemType, dataType, annotation, flags);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasFlag(FormatterFlags flags) =>
			(Flags & flags) == flags;
	}
}
