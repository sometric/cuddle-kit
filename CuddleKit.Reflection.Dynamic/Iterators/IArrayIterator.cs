using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Dynamic.Iterators
{
	internal interface IArrayIterator
	{
		void Iterate<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document)
			where TVisitor : struct, IValueVisitor;
	}
}
