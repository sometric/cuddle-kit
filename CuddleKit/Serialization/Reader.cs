using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace CuddleKit.Serialization
{
	using Detail;

	public ref struct Reader
	{
		private static readonly ArrayPool<char> Pool = ArrayPool<char>.Shared;

		private TextReader _reader;

		private char[] _buffer;
		private int _bufferLength;
		private int _bufferPosition;

		private int _lineNumber;
		private int _columnNumber;

		public Reader(TextReader reader) : this() =>
			_reader = reader ?? throw new ArgumentNullException(nameof(reader));

		public Reader(ReadOnlySpan<char> source) : this()
		{
			_buffer = Pool.Rent(source.Length);
			_bufferLength = source.Length;
			source.CopyTo(_buffer.AsSpan(0, _bufferLength));
		}

		public static implicit operator Reader(ReadOnlySpan<char> source) =>
			new(source);

		public static implicit operator Reader(string source) =>
			new(source);

		public static implicit operator Reader(TextReader reader) =>
			new(reader);

		public void Dispose() =>
			Pool.Return(_buffer);

		internal bool Done =>
			Peek(0) == default;

		internal TokenReference? ReadAnnotation(ref TokenTable tokenTable)
		{
			if (!ScanSymbol('('))
				return default;

			var token = ReadString(ref tokenTable) ?? ReadIdentifier(ref tokenTable);
			if (!token.HasValue)
				throw MakeExpectationException("String or Identifier");

			if (!ScanSymbol(')'))
				throw MakeExpectationException(")");

			return token;
		}

		public TokenReference? ReadIdentifier(ref TokenTable tokenTable)
		{/*
			(
				(identifier-char - digit - sign) identifier-char* |
				sign ((identifier-char - digit) identifier-char*)?
			) - keyword
*/
			using var identifierBuffer = new Vector<char>(16);

			var position = 0;
			var symbol = Peek(position);

			if (Characters.Signs.Contains(symbol))
			{
				identifierBuffer.Push() = symbol;
				symbol = Peek(++position);
			}

			if (Characters.IsNoneIdentifierSymbol(symbol) || Characters.DecimalDigits.Contains(symbol))
			{
				if (position == 0)
					return default;

				Advance(position);
				return tokenTable.AllocateToken(DataType.String, identifierBuffer.Buffer);
			}

			do
			{
				identifierBuffer.Push() = symbol;
				symbol = Peek(++position);
			} while (!Characters.IsNoneIdentifierSymbol(symbol));

			var identifier = identifierBuffer.ReadOnlyBuffer;
			for (int i = 0, length = Characters.Keywords.Length; i < length; ++i)
			{
				if (identifier.SequenceEqual(Characters.Keywords[i]))
					return default;
			}

			Advance(position);
			return tokenTable.AllocateToken(DataType.String, identifier);
		}

		private TokenReference? ReadEscapedString(ref TokenTable tokenTable)
		{
			if (!ScanSequence("\""))
				return default;

			using var stringBuffer = new Vector<char>(16);
			ReadOnlySpan<byte> digitsLookup = Characters.DigitsLookup;
			Span<byte> unicodeBytes = stackalloc byte[Characters.MaxUnicodeSequenceLength];

			for (var finished = false; !finished;)
			{
				if (ScanNewLine())
				{
					stringBuffer.Push() = '\n';
					continue;
				}

				var symbol = ScanSymbol();
				switch (symbol)
				{
					case '"': finished = true; break;

					case '\\':
						var escapedSymbol = ScanSymbol();
						var escapeIndex = Characters.EncodedEscapes.IndexOf(escapedSymbol);
						if (escapeIndex >= 0)
						{
							stringBuffer.Push() = Characters.DecodedEscapes[escapeIndex];
						}
						else if (escapedSymbol == 'u')
						{
							if (!ScanSymbol('{'))
								throw MakeExpectationException("{");

							var digitSet = Characters.HexDigits;
							var digit = ScanSymbol();

							if (!digitSet.Contains(char.ToLowerInvariant(digit)))
								throw MakeException("Invalid unicode point data");

							var codePoint = (uint) digitsLookup[digit - '0'];

							for (var length = 1;; ++length)
							{
								if (length > Characters.MaxUnicodeSequenceLength)
									throw MakeExpectationException("}");

								digit = ScanSymbol();
								if (digit == '}')
									break;

								if (!digitSet.Contains(char.ToLowerInvariant(digit)))
									throw MakeException($"Expected hex digit or '}}' but got '{digit}'");

								codePoint = (codePoint << 4) + digitsLookup[digit - '0'];
							}

							if (codePoint > 0x10ffffu)
								throw MakeException($"Invalid unicode code point {codePoint:x}");

							try
							{
#if NETCOREAPP3_0_OR_GREATER
								var rune = new Rune(codePoint);
								rune.EncodeToUtf16(stringBuffer.PushMany(rune.Utf16SequenceLength));
#else
								var unicodeLength = 0;

								for (unicodeBytes.Clear(); codePoint > 0; codePoint >>= 8)
									unicodeBytes[unicodeLength++] = (byte) codePoint;

								unicodeLength += unicodeLength % 2;

								var bytes = unicodeBytes.Slice(0, unicodeLength);
								var sequenceLength = Encoding.Unicode.GetCharCount(bytes);
								Encoding.Unicode.GetChars(bytes, stringBuffer.PushMany(sequenceLength));
#endif
							}
							catch (Exception exception)
							{
								throw MakeException("Invalid unicode symbol value", exception);
							}
						}
						else
							throw MakeException($"Invalid escape sequence '\\{escapedSymbol}'");

						break;

					default: stringBuffer.Push() = symbol; break;
				}
			}

			return tokenTable.AllocateToken(DataType.String, stringBuffer.ReadOnlyBuffer);
		}

		public TokenReference? ReadValue(ref TokenTable tokenTable) =>
			ReadString(ref tokenTable) ??
			ReadNumber(ref tokenTable) ??
			ReadKeyword(ref tokenTable);

		public TokenReference? ReadString(ref TokenTable tokenTable) =>
			ReadRawString(ref tokenTable) ??
			ReadEscapedString(ref tokenTable);

		public TokenReference? ReadRawString(ref TokenTable tokenTable)
		{
			if (Peek(0) != 'r')
				return default;

			var position = 1;
			var hashLength = 0;

			for (var finished = false; !finished; ++position)
			{
				switch (Peek(position))
				{
					case '#': ++hashLength; break;
					case '"': finished = true; break;
					default: return default;
				}
			}

			using var closingBuffer = new Vector<char>(1 + hashLength);
			closingBuffer.Push() = '"';
			closingBuffer.PushMany(hashLength).Fill('#');

			using var stringBuffer = new Vector<char>(16);
			var closingSequence = closingBuffer.ReadOnlyBuffer;

			Advance(position);

			while (true)
			{
				if (MatchSequence(0, closingSequence))
				{
					Advance(closingSequence.Length);
					break;
				}

				if (ScanNewLine())
				{
					stringBuffer.Push() = '\n';
					continue;
				}

				var symbol = Peek(0);
				stringBuffer.Push() = symbol != '\0'
					? symbol
					: throw MakeException($"Expected '{closingSequence.ToString()}' but got EOF");

				Advance(1);
			}

			return tokenTable.AllocateToken(DataType.String, stringBuffer.ReadOnlyBuffer);
		}

		public TokenReference? ReadReal(ref TokenTable tokenTable)
		{
			var sign = Peek(0);
			var position = Characters.Signs.Contains(sign) ? 1 : 0;
			var digitSet = Characters.DecimalDigits;

			var digit = Peek(position++);
			if (!digitSet.Contains(digit))
				return default;

			using var digitsBuffer = new Vector<char>(16);

			if (sign == '-')
				digitsBuffer.Push() = '-';

			digitsBuffer.Push() = digit;

			var readFraction = false;
			var readExponent = false;

			while (!readFraction & !readExponent)
			{
				switch (Peek(position++))
				{
					case '_':
						continue;

					case '.':
						digitsBuffer.Push() = '.';
						readFraction = true;
						break;

					case 'e':
					case 'E':
						digitsBuffer.Push() = 'E';
						readExponent = true;
						break;

					case var symbol when digitSet.Contains(symbol):
						digitsBuffer.Push() = symbol;
						break;

					default:
						return default;
				}
			}

			if (readFraction)
			{
				if (!digitSet.Contains(digitsBuffer.Push() = Peek(position++)))
					return default;

				for (var finished = false; !finished & !readExponent;)
				{
					switch (Peek(position++))
					{
						case '_':
							continue;

						case 'e':
						case 'E':
							digitsBuffer.Push() = 'E';
							readExponent = true;
							break;

						case var symbol when digitSet.Contains(symbol):
							digitsBuffer.Push() = symbol;
							break;

						default:
							finished = true;
							--position;
							break;
					}
				}
			}

			if (readExponent)
			{
				var firstSymbol = Peek(position++);
				var isSign = Characters.Signs.Contains(firstSymbol);
				if (!digitSet.Contains(firstSymbol) && !isSign)
					return default;

				if (!isSign)
					digitsBuffer.Push() = '+';

				digitsBuffer.Push() = firstSymbol;

				for (var finished = false; !finished;)
				{
					switch (Peek(position++))
					{
						case '_':
							continue;

						case var symbol when digitSet.Contains(symbol):
							digitsBuffer.Push() = symbol;
							break;

						default:
							finished = true;
							--position;
							break;
					}
				}
			}

			var data = tokenTable.AllocateToken(DataType.Real, digitsBuffer.Length, out var token);
			digitsBuffer.ReadOnlyBuffer.CopyTo(data);

			Advance(position);

			return token;
		}

		public TokenReference? ReadNumber(ref TokenTable tokenTable) =>
			ReadReal(ref tokenTable) ??
			ReadInteger(ref tokenTable);

		public TokenReference? ReadInteger(ref TokenTable tokenTable)
		{
			var sign = Peek(0);
			var position = Characters.Signs.Contains(sign) ? 1 : 0;

			var digitSet = (Peek(position), Peek(position + 1)) switch
			{
				('0', 'x') => Characters.HexDigits,
				('0', 'o') => Characters.OctalDigits,
				('0', 'b') => Characters.BinaryDigits,
				_ => Characters.DecimalDigits
			};

			if (digitSet.Length != 10)
				position += 2;

			var digit = Peek(position++);
			if (!digitSet.Contains(char.ToLowerInvariant(digit)))
				return default;

			using var digitsBuffer = new Vector<byte>(16);
			ReadOnlySpan<byte> digitsLookup = Characters.DigitsLookup;
			digitsBuffer.Push() = digitsLookup[digit - '0'];

			for (;; ++position)
			{
				var symbol = Peek(position);
				if (symbol == '_')
					continue;

				if (digitSet.Contains(char.ToLowerInvariant(symbol)))
					digitsBuffer.Push() = digitsLookup[symbol - '0'];
				else
					break;
			}

			var numberBase = digitSet.Length;
			var digits = digitsBuffer.Buffer;

			digits = digits.TrimStart<byte>(0);

			// convert non-decimal to decimal
			if ((numberBase != 10) & (digits.Length > 0))
			{
				using var invertedDigitsBuffer = new Vector<byte>(2 * digits.Length);

				for (var index = 0; index < digits.Length;) unchecked
				{
					var reminder = 0;
					for (var i = index; i < digits.Length; ++i)
					{
						reminder = reminder * numberBase + digits[i];
						digits[i] = (byte) (reminder / 10);
						reminder %= 10;
					}

					invertedDigitsBuffer.Push() = (byte) reminder;
					index += digits[index] == 0 ? 1 : 0;
				}

				var invertedDigits = invertedDigitsBuffer.Buffer;
				invertedDigits.Reverse();

				digitsBuffer.Clear();
				digitsBuffer.Push(invertedDigits.TrimStart<byte>(0));
				digits = digitsBuffer.Buffer;
			}

			ReadOnlySpan<byte> zero = stackalloc byte[] { 0 };
			var outputDigits = digits.Length == 0 ? zero : digits;

			var tokenLength = (sign == '-' ? 1 : 0) + outputDigits.Length;
			var data = tokenTable.AllocateToken(DataType.Integer, tokenLength, out var token);
			var dataIndex = 0;

			if (sign == '-')
				data[dataIndex++] = '-';

			for (var i = 0; i < outputDigits.Length; ++i, ++dataIndex)
				data[dataIndex] = Characters.HexDigits[outputDigits[i]];

			Advance(position);
			return token;
		}

		public readonly DeserializationException MakeException(string message, Exception innerException = null) =>
			new(_lineNumber, _columnNumber, message, innerException);

		public readonly DeserializationException MakeExpectationException(string expectation, Exception innerException = null) =>
			MakeException($"Expected '{expectation} but got '{(Peek() > 0 ? Peek().ToString() : "EOF")}''", innerException);

		public TokenReference? ReadKeyword(ref TokenTable tokenTable)
		{
			for (int i = 0, length = Characters.Keywords.Length; i < length; ++i)
			{
				if (ScanSequence(Characters.Keywords[i]))
					return tokenTable.AllocateKeywordToken(i);
			}

			return default;
		}

		private char ScanSymbol()
		{
			var symbol = Peek(0);
			Advance(1);
			return symbol;
		}

		public bool ScanSymbol(char symbol)
		{
			var match = Peek(0) == symbol;
			if (match)
				Advance(1);

			return match;
		}

		private bool ScanAny(in ReadOnlySpan<char> symbolSet)
		{
			var symbol = Peek(0);
			if (!symbolSet.Contains(symbol))
				return false;

			Advance(1);
			return true;
		}

		private bool ScanAny()
		{
			var symbol = Peek(0);
			Advance(1);
			return symbol != '\0';
		}

		internal bool Scan(Literal literal) =>
			literal switch
			{
				Literal.NewLine => ScanNewLine(),
				Literal.UnicodeSpace => ScanUnicodeSpace(),
				Literal.MultiLineComment => ScanMultiLineComment(),
				Literal.SingleLineComment => ScanSingleLineComment(),
				Literal.SlashDashComment => ScanSlashDashComment(),
				Literal.EscLine => ScanEscLine(),
				Literal.NodeSpace => ScanNodeSpace(),
				Literal.WhiteSpace => ScanUnicodeSpace() || ScanMultiLineComment(),
				Literal.LineSpace => ScanNewLine() || ScanUnicodeSpace() || ScanMultiLineComment() || ScanSingleLineComment(),
				Literal.NodeTerminator => ScanAny(";\0") || ScanNewLine() || ScanSingleLineComment(),
				_ => false
			};

		internal bool ScanAll(Literal literal)
		{
			var scanned = false;

			while (Scan(literal))
				scanned = true;

			return scanned;
		}


		private bool ScanNewLine()
		{
			var count = Peek(0) switch
			{
				'\u000d' when Peek(1) == '\u000a' => 2, // Carriage Return and Line Feed
				'\u000a' => 1, // Line Feed
				'\u000c' => 1, // Form Feed
				'\u000d' => 1, // Carriage Return
				'\u0085' => 1, // Next Line
				'\u2028' => 1, // Line Separator
				'\u2029' => 1, // Paragraph Separator
				_ => 0
			};

			Advance(count);

			if (count > 0)
			{
				++_lineNumber;
				_columnNumber = 0;
			}

			return count > 0;
		}

		private bool ScanUnicodeSpace()
		{
			var count = Peek(0) switch
			{
				'\u0009' => 1, // Tabulation
				'\u0020' => 1, // Space
				'\u00a0' => 1, // No-Break Space
				'\u1680' => 1, // Ogham Space Mark
				'\u2000' => 1, // En Quad
				'\u2001' => 1, // Em Quad
				'\u2002' => 1, // En Space
				'\u2003' => 1, // Em Space
				'\u2004' => 1, // Three-Per-Em Space
				'\u2005' => 1, // Four-Per-Em Space
				'\u2006' => 1, // Six-Per-Em Space
				'\u2007' => 1, // Figure Space
				'\u2008' => 1, // Punctuation Space
				'\u2009' => 1, // Thin Space
				'\u200a' => 1, // Hair Space
				'\u202f' => 1, // Narrow No-Break Space
				'\u205f' => 1, // Medium Mathematical Space
				'\u3000' => 1, // Ideographic Space
				'\ufeff' => 1, // BOM
				_ => 0
			};

			Advance(count);
			return count > 0;
		}

		private bool ScanMultiLineComment()
		{
			if (!ScanSequence("/*"))
				return false;

			while (!ScanSequence("*/"))
			{
				var success = ScanMultiLineComment() || ScanUnicodeSpace() || ScanNewLine() || ScanAny();
				if (!success)
					throw MakeException("Unexpected EOF");
			}

			return true;
		}

		private bool ScanSingleLineComment()
		{
			if (!ScanSequence("//"))
				return false;

			while (!ScanNewLine() && ScanAny()) {}

			return true;
		}

		private bool ScanSlashDashComment()
		{
			if (!ScanSequence("/-"))
				return false;

			while (ScanNodeSpace()) {}

			return true;
		}

		private bool ScanEscLine()
		{
			if (!ScanSequence("\\"))
				return false;

			while (ScanUnicodeSpace()) {}

			if (ScanNewLine() || ScanSingleLineComment())
				return true;

			throw MakeExpectationException("Line break or Single line comment");
		}

		private bool ScanNodeSpace() =>
			ScanAll(Literal.WhiteSpace) | Scan(Literal.EscLine) | ScanAll(Literal.WhiteSpace);

		private bool MatchSequence(int position, in ReadOnlySpan<char> sequence)
		{
			for (var i = 0; i < sequence.Length; ++i)
			{
				if (sequence[i] != Peek(i + position))
					return false;
			}

			return true;
		}

		private bool ScanSequence(in ReadOnlySpan<char> sequence)
		{
			if (!MatchSequence(0, sequence))
				return false;

			Advance(sequence.Length);
			return true;
		}

		private readonly char Peek() =>
			_bufferPosition < _bufferLength ? _buffer[_bufferPosition] : default;

		private char Peek(int offset)
		{
			if (_bufferPosition + offset < _bufferLength)
				return _buffer[_bufferPosition + offset];

			if (_reader == null)
				return '\0';

			var newBuffer = _buffer == null || _bufferPosition + offset >= _buffer.Length
				? Pool.Rent(Math.Max(1024, _bufferPosition + offset + 1))
				: _buffer;

			if (newBuffer != _buffer && _buffer != null)
			{
				_buffer.AsSpan(0, _bufferLength).CopyTo(newBuffer.AsSpan(0, _bufferLength));
				Pool.Return(_buffer);
			}

			_buffer = newBuffer;

			var symbolsToRead = _buffer.Length - _bufferLength;
			var readCount = _reader.Read(_buffer, _bufferLength, symbolsToRead);

			if (readCount < symbolsToRead)
				_reader = null;

			_bufferLength += readCount;

			return _bufferPosition + offset < _bufferLength ? _buffer[_bufferPosition + offset] : '\0';
		}

		private void Advance(int offset)
		{
			//if (_state == State.Done)
			//	return;

			//if (_bufferPosition + offset < _bufferLength)
			{
				_bufferPosition += offset;
				_columnNumber += offset;
				//return;
			}

			//_state = State.Done;
		}
	}
}
