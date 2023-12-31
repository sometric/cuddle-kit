using System;

namespace CuddleKit.Serialization
{
	using Internal;

	public struct TokenTable
	{
		private Vector<Token> _tokens;
		private MultiVector<char> _data;

		public void Dispose()
		{
			_tokens.Dispose();
			_data.Dispose();
		}

		public Span<char> AllocateToken(DataType type, int length, out TokenReference token)
		{
			token = new TokenReference(_tokens.Length); 
			_tokens.Push() = new Token(type, _data.RowsCount);
			_data.PushRow(length);
			return _data.Push(token.Index, length);
		}

		public TokenReference AllocateToken(DataType type, ReadOnlySpan<char> data)
		{
			var token = new TokenReference(_tokens.Length);
			_tokens.Push() = new Token(type, _data.RowsCount);
			_data.PushRow(data);
			return token;
		}

		public TokenReference AllocateKeywordToken(int keywordIndex)
		{
			var token = new TokenReference(_tokens.Length);
			_tokens.Push() = new Token(DataType.Keyword, keywordIndex);
			return token;
		}

		public readonly ReadOnlySpan<char> GetTokenData(TokenReference token)
		{
			ref readonly var header = ref _tokens[token.Index];
			return header.Type switch
			{
				DataType.Keyword => Characters.Keywords[header.RowIndex],
				_ => _data[header.RowIndex]
			};
		}

		public readonly DataType GetTokenType(TokenReference token) =>
			_tokens[token.Index].Type;

		public void Clear()
		{
			_tokens.Clear();
			_data.Clear();
		}

		private readonly struct Token
		{
			public readonly DataType Type;
			public readonly int RowIndex;

			public Token(DataType type, int index) =>
				(Type, RowIndex) = (type, index);
		}
	}
}
