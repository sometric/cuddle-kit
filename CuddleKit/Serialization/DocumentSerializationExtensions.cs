using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace CuddleKit.Serialization
{
	using Output;

	public static class DocumentSerializationExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(this in Document document, StringBuilder stringBuilder, in WriteSettings settings) =>
			new Writer<StringBuilderOutput>(new StringBuilderOutput(stringBuilder), settings).Write(document);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(this in Document document, StringBuilder stringBuilder) =>
			Write(document, stringBuilder, WriteSettings.Default);

		public static void Write(this in Document document, TextWriter textWriter, in WriteSettings settings) =>
			new Writer<TextWriterOutput>(new TextWriterOutput(textWriter), settings).Write(document);

		public static void Write(this in Document document, TextWriter textWriter) =>
			Write(document, textWriter, WriteSettings.Default);
	}
}
