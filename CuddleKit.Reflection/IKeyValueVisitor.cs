using CuddleKit.Serialization;

namespace CuddleKit.Reflection
{
	public interface IKeyValueVisitor
	{
		void Visit<TKey, TValue>(in TKey key, in TValue value, ref Document document);
	}
}
