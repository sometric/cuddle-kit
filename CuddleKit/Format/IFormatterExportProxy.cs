using System;
using System.Diagnostics.Contracts;

namespace CuddleKit.Format
{
	public interface IFormatterExportProxy
	{
		ReadOnlySpan<char> Annotation { get; }

		[Pure] T Export<T>();
	}
}
