using System;
using System.Reflection;

namespace CuddleKit.Reflection
{
	using Description;
	using CuddleKit.Serialization;

	public interface IReflectionPolicy
	{
		MemberDescriptor Describe(FieldInfo fieldInfo);

		MemberDescriptor Describe(PropertyInfo propertyInfo);

		void VisitObject<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document)
			where TVisitor : struct, IValueVisitor;

		void VisitArray<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document,
			Type[] typeArguments)
			where TVisitor : struct, IValueVisitor;

		void VisitDictionary<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document, Type[] typeArguments)
			where TVisitor : struct, IKeyValueVisitor;
	}
}
