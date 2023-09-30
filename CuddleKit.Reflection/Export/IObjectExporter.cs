using CuddleKit.Reflection.Description;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Export
{
	public interface IObjectExporter : IMemberExporter
	{
		void Export<TInstance, TVisitor>(
			MemberDescriptor descriptor,
			in TInstance instance,
			ref TVisitor visitor,
			ref Document document)
			where TVisitor : struct, IValueVisitor;
	}
}
