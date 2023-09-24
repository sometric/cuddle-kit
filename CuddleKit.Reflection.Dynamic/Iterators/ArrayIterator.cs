using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Dynamic.Iterators
{
	internal sealed class ArrayIterator<TElement> : IArrayIterator
	{
		void IArrayIterator.Iterate<TInstance, TVisitor>(in BoundMember<TInstance> member, ref TVisitor visitor, ref Document document)
		{
			var array = member.Export<TElement[]>();
			for (int i = 0, length = array.Length; i < length; ++i)
				visitor.Visit(array[i], ref document);
		}
	}
}
