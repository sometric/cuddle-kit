using System.Collections.Generic;
using System.Reflection;

namespace CuddleKit.Reflection.Dynamic
{
	using Description;

	public sealed class DynamicReflectionPolicy : IReflectionPolicy
	{
		public static readonly IReflectionPolicy Shared = new DynamicReflectionPolicy();

		private static readonly IMemberResolver[] Resolvers = {
			DynamicObjectResolver.Shared,
			DynamicArrayResolver.Shared,
			DynamicDictionaryResolver.Shared
		};

		MemberDescriptor IReflectionPolicy.Describe(FieldInfo fieldInfo) =>
			new DynamicFieldDescriptor(fieldInfo);

		MemberDescriptor IReflectionPolicy.Describe(PropertyInfo propertyInfo) =>
			new DynamicPropertyDescriptor(propertyInfo);

		IReadOnlyList<IMemberResolver> IReflectionPolicy.GetResolvers() =>
			Resolvers;
	}
}
