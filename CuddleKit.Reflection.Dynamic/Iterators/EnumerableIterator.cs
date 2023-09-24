using System.Collections.Generic;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Dynamic.Iterators
{
	internal sealed class EnumerableIterator<TElement> : IArrayIterator
	{
		void IArrayIterator.Iterate<TInstance, TVisitor>(in BoundMember<TInstance> member, ref TVisitor visitor, ref Document document)
		{
			foreach (var element in member.Export<IEnumerable<TElement>>())
				visitor.Visit(element, ref document);
		}
	}
}
