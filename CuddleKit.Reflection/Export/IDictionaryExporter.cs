using CuddleKit.Reflection.Description;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Export
{
	public interface IDictionaryExporter : IMemberExporter
	{
		System.Type KeyType { get; }
		System.Type ValueType { get; }

		void Export<TInstance, TVisitor>(
			MemberDescriptor descriptor,
			in TInstance instance,
			ref TVisitor visitor,
			ref Document document)
			where TVisitor : struct, IKeyValueVisitor;
	}
}
