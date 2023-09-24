using System;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Naming
{
	public interface INamingConvention
	{
		TokenReference Write(ReadOnlySpan<char> name, ref Document document);
	}
}
