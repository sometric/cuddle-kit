using CuddleKit.Serialization;

namespace CuddleKit.Reflection
{
	public interface IValueVisitor
	{
		void Visit<TValue>(in TValue entry, ref Document document);
	}
}
