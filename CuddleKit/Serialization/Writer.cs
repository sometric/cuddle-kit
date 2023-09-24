using System;

namespace CuddleKit.Serialization
{
	using Internal;
	using Output;

	public struct WriteSettings
	{
		public static readonly WriteSettings Default = new()
		{
			Ident = "    ",
			EndWithLineBreak = false
		};

		public string Ident;
		public bool EndWithLineBreak;
	}

	public ref struct Writer<TOutput>
		where TOutput : struct, IDocumentOutput
	{
		private readonly WriteSettings _settings;
		private TOutput _output;

		public Writer(TOutput output, in WriteSettings settings)
		{
			_settings = settings;
			_output = output;
		}

		public void Write(in Document document)
		{
			WriteNodes(document, document.Nodes, 0);
			if (_settings.EndWithLineBreak)
				_output.Write("\n");
		}

		private void WriteNodes(in Document document, ReadOnlySpan<NodeReference> nodes, int ident)
		{
			if (nodes.IsEmpty)
			{
				WriteSymbol('\n');
				return;
			}

			foreach (var node in nodes)
			{
				WriteIdent(ident);

				if (document.TryGetAnnotation(node, out var nodeType))
					WriteAnnotation(nodeType);

				WriteIdentifier(document.GetName(node));

				foreach (var argument in document.GetArguments(node))
				{
					WriteSymbol(' ');
					WriteValue(document, argument);
				}

				foreach (var property in document.GetProperties(node))
				{
					WriteSymbol(' ');
					WriteIdentifier(document.GetKey(property));
					WriteSymbol('=');
					WriteValue(document, property.Value);
				}

				var children = document.GetChildren(node);
				if (!children.IsEmpty)
				{
					Write(" {\n");
					WriteNodes(document, children, ident + 1);
					WriteIdent(ident);
					WriteSymbol('}');
				}

				WriteSymbol('\n');
			}
		}

		private void WriteValue(in Document document, ValueReference value)
		{
			if (document.TryGetAnnotation(value, out var annotation))
				WriteAnnotation(annotation);

			var valueData = document.GetData(value);
			switch (document.GetType(value))
			{
				case DataType.Keyword:
				case DataType.Integer:
				case DataType.Real: Write(valueData); break;
				case DataType.String: WriteString(valueData); break;
			}
		}

		private void WriteIdent(int ident)
		{
			for (var i = 0; i < ident; ++i)
				_output.Write(_settings.Ident);
		}

		private void WriteAnnotation(ReadOnlySpan<char> value)
		{
			WriteSymbol('(');
			WriteIdentifier(value);
			WriteSymbol(')');
		}

		private void WriteIdentifier(ReadOnlySpan<char> value)
		{
			if (IsIdentifier(value))
				Write(value);
			else
				WriteString(value);
		}

		private static bool IsIdentifier(ReadOnlySpan<char> value)
		{
			if (value.Length == 0)
				return false;

			var position = 0;
			var symbol = value[position];

			if (Characters.Signs.Contains(symbol))
				++position;

			if (Characters.IsNoneIdentifierSymbol(symbol) || Characters.DecimalDigits.Contains(symbol))
				return position == value.Length;

			while (position < value.Length)
			{
				if (Characters.IsNoneIdentifierSymbol(value[position++]))
					return false;
			}

			return true;
		}

		private void WriteString(ReadOnlySpan<char> value)
		{
			using var buffer = new Vector<char>(value.Length * 2);

			buffer.Push() = '"';
			for (var i = 0; i < value.Length; ++i)
			{
				var symbol = value[i];
				var escapeIndex = Characters.DecodedEscapes.IndexOf(symbol);
				if (escapeIndex >= 0 && symbol != '/')
				{
					buffer.Push() = '\\';
					buffer.Push() = Characters.EncodedEscapes[escapeIndex];
				}
				else if (char.IsSurrogate(symbol))
				{
					buffer.Push() = symbol;
					buffer.Push() = value[++i];
				}
				else
				{
					buffer.Push() = symbol;
				}
			}

			buffer.Push() = '"';

			Write(buffer.ReadOnlyBuffer);
		}

		private void Write(ReadOnlySpan<char> data) =>
			_output.Write(data);

		private void WriteSymbol(char symbol)
		{
			ReadOnlySpan<char> span = stackalloc char[] { symbol }; 
			_output.Write(span);
		}
	}
}
