using System.Collections.Generic;
using System.Reflection;

namespace CuddleKit.Reflection.Portable
{
	using Description;

	public sealed class PortableReflectionPolicy : IReflectionPolicy
	{
		public static readonly PortableReflectionPolicy Shared = new();

		private static readonly IMemberResolver[] Resolvers = {
			PortableObjectResolver.Shared,
			PortableArrayResolver.Shared,
			PortableDictionaryResolver.Shared
		};

		MemberDescriptor IReflectionPolicy.Describe(FieldInfo fieldInfo) =>
			new PortableFieldDescriptor(fieldInfo);

		MemberDescriptor IReflectionPolicy.Describe(PropertyInfo propertyInfo) =>
			new PortablePropertyDescriptor(propertyInfo);

		public IReadOnlyList<IMemberResolver> GetResolvers() =>
			Resolvers;
	}
}
