using System;

namespace CuddleKit.Format
{
	public interface IFormatterExportProxy
	{
		ReadOnlySpan<char> Annotation { get; }

		TValue Export<TValue>();
	}
}
