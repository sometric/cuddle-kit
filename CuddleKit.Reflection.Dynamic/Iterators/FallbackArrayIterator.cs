using System.Collections;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Dynamic.Iterators
{
	internal sealed class FallbackArrayIterator : IArrayIterator
	{
		public static readonly FallbackArrayIterator Shared = new();

		void IArrayIterator.Iterate<TInstance, TVisitor>(in BoundMember<TInstance> member, ref TVisitor visitor, ref Document document)
		{
			foreach (var element in member.Export<IEnumerable>())
				visitor.Visit(element, ref document);
		}
	}
}
