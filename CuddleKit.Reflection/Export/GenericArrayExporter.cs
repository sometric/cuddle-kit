using System;
using CuddleKit.Reflection.Description;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Export
{
	public abstract class GenericArrayExporter<TArray, TElement> : IArrayExporter
	{
		Type IArrayExporter.ElementType =>
			typeof(TElement);

		void IArrayExporter.Export<TInstance, TVisitor>(
			MemberDescriptor descriptor,
			in TInstance instance,
			ref TVisitor visitor,
			ref Document document)
		{
			var array = descriptor.GetValue<TInstance, TArray>(instance);
			Iterate(array, ref visitor, ref document);
		}

		protected abstract void Iterate<TVisitor>(TArray array, ref TVisitor visitor, ref Document document)
			where TVisitor : struct, IValueVisitor;
	}
}
