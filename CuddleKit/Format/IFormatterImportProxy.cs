using System;

namespace CuddleKit.Format
{
	public interface IFormatterImportProxy
	{
		void Import<T>(T value, ReadOnlySpan<char> annotation);
	}
}
