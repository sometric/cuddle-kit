using CuddleKit.Reflection.Description;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Export
{
	public sealed class GenericObjectExporter<TValue> : IObjectExporter
	{
		void IObjectExporter.Export<TInstance, TVisitor>(
			MemberDescriptor descriptor,
			in TInstance instance,
			ref TVisitor visitor,
			ref Document document) =>
			visitor.Visit(descriptor.GetValue<TInstance, TValue>(instance), ref document);
	}
}
