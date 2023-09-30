using System.Collections.Generic;
using System.Reflection;

namespace CuddleKit.Reflection
{
	using Description;

	public interface IReflectionPolicy
	{
		MemberDescriptor Describe(FieldInfo fieldInfo);

		MemberDescriptor Describe(PropertyInfo propertyInfo);

		IReadOnlyList<IMemberResolver> GetResolvers();
	}
}
