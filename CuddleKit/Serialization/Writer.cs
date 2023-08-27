using System;

namespace CuddleKit.Serialization
{
	using Detail;
	using Output;

	public ref struct Writer<TOutput>
		where TOutput : struct, IDocumentOutput
	{
		private const string Ident = "    ";
		private TOutput _output;

		public Writer(TOutput output) =>
			_output = output;

		public void Write(in Document document) =>
			WriteNodes(document, document.Nodes, 0);

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
			if (document.TryGetAnnotation(value, out var valueType))
				WriteAnnotation(valueType);

			var valueData = document.GetData(value);
			switch (document.GetType(value))
			{
				case ValueType.Keyword:
				case ValueType.Integer:
				case ValueType.Real: Write(valueData); break;
				case ValueType.String: WriteString(valueData); break;
			}
		}

		private void WriteIdent(int ident)
		{
			for (var i = 0; i < ident; ++i)
				_output.Write(Ident);
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
