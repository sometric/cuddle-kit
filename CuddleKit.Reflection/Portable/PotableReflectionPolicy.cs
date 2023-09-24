using System;
using System.Collections;
using System.Reflection;
using CuddleKit.Reflection.Description;

namespace CuddleKit.Reflection.Portable
{
	using CuddleKit.Serialization;

	public sealed class PotableReflectionPolicy : IReflectionPolicy
	{
		public static readonly PotableReflectionPolicy Shared = new();

		MemberDescriptor IReflectionPolicy.Describe(FieldInfo fieldInfo) =>
			new PortableFieldDescriptor(fieldInfo);

		MemberDescriptor IReflectionPolicy.Describe(PropertyInfo propertyInfo) =>
			new PortablePropertyDescriptor(propertyInfo);

		void IReflectionPolicy.VisitObject<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document) =>
			visitor.Visit(member.Export<object>(), ref document);

		void IReflectionPolicy.VisitArray<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document,
			Type[] typeArguments)
		{
			foreach (var entry in member.Export<IEnumerable>())
				visitor.Visit(entry, ref document);
		}

		void IReflectionPolicy.VisitDictionary<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document,
			Type[] typeArguments)
		{
			var dictionary = member.Export<IDictionary>();
			foreach (var key in dictionary.Keys)
				visitor.Visit(key, dictionary[key], ref document);
		}
	}
}
