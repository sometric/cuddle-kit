using System;
using System.IO;

namespace CuddleKit.Output
{
	public readonly struct TextWriterOutput : IDocumentOutput
	{
		private readonly TextWriter _textWriter;

		public TextWriterOutput(TextWriter textWriter) =>
			_textWriter = textWriter;

		public void Write(ReadOnlySpan<char> value) =>
			_textWriter?.Write(value);
	}
}
