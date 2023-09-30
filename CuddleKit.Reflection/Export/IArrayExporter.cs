using CuddleKit.Reflection.Description;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Export
{
	public interface IArrayExporter : IMemberExporter
	{
		System.Type ElementType { get; }

		void Export<TInstance, TVisitor>(
			MemberDescriptor descriptor,
			in TInstance instance,
			ref TVisitor visitor,
			ref Document document)
			where TVisitor : struct, IValueVisitor;
	}
}
