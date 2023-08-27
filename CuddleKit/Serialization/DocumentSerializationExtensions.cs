using System.IO;
using System.Text;

namespace CuddleKit.Serialization
{
	using Output;

	public static class DocumentSerializationExtensions
	{
		public static void Write(this Document document, StringBuilder stringBuilder) =>
			new Writer<StringBuilderOutput>(new StringBuilderOutput(stringBuilder)).Write(document);

		public static void Write(this Document document, TextWriter textWriter) =>
			new Writer<TextWriterOutput>(new TextWriterOutput(textWriter)).Write(document);
	}
}
