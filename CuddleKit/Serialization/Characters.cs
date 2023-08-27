using System;
using System.Runtime.CompilerServices;

namespace CuddleKit.Serialization
{
	using Detail;

	internal static class Characters
	{
		public static readonly string[] Keywords =
		{
			"null",
			"true",
			"false"
		};

		public static ReadOnlySpan<char> Signs => "+-";

		public static ReadOnlySpan<char> BinaryDigits => "01";
		public static ReadOnlySpan<char> OctalDigits => "01234567";
		public static ReadOnlySpan<char> DecimalDigits => "0123456789";
		public static ReadOnlySpan<char> HexDigits => "0123456789abcdef";

		public static ReadOnlySpan<char> NonIdentifier => "\\/(){}<>;[]=,\"";

		public static ReadOnlySpan<char> EncodedEscapes => "nrt\\/\"bf";
		public static ReadOnlySpan<char> DecodedEscapes => "\n\r\t\\/\"\b\f";

		public const int MaxUnicodeSequenceLength = 6;

		public static readonly byte[] DigitsLookup =
		{
			// 0 - 47 skipped. Subtract '0' from symbol to use this lookup
			0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, // 63
			0x0, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, // 79
			0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, // 95
			0x0, 0xa, 0xb, 0xc, 0xd, 0xe, 0xf // 102
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNoneIdentifierSymbol(char symbol) =>
			symbol <= 0x20 || NonIdentifier.Contains(symbol);
	}
}
