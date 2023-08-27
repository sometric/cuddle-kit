using System;

namespace CuddleKit.ObjectModel
{
	/// <summary>
	/// Represents a visitor that visits a value of a specific type and returns a result.
	/// </summary>
	/// <typeparam name="TResult">The type of the result returned by the visitor.</typeparam>
	public interface IDocumentValueVisitor<out TResult>
	{
		/// <summary>
		/// Visits a value of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to visit.</param>
		/// <param name="annotation">The annotation associated with the value.</param>
		/// <returns>The result of visiting the value.</returns>
		TResult Visit<T>(in T value, ReadOnlySpan<char> annotation);
	}
}
