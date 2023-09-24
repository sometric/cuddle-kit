using System;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Naming
{
	public sealed class UnalteredNamingConvention : INamingConvention
	{
		public static readonly UnalteredNamingConvention Shared = new();

		private UnalteredNamingConvention() {}

		TokenReference INamingConvention.Write(ReadOnlySpan<char> name, ref Document document) =>
			document.AllocateString(name);
	}
}
