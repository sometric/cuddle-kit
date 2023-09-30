using System;

namespace CuddleKit.Reflection.Dynamic
{
	internal static class TypeExtensions
	{
		public static bool TryGetGenericInterface(this Type type, Type genericType, out Type[] typeArguments)
		{
			if (!genericType.IsGenericType)
			{
				typeArguments = Array.Empty<Type>();
				return genericType.IsAssignableFrom(type);
			}

			if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
			{
				typeArguments = type.GetGenericArguments();
				return true;
			}

			foreach (var interfaceType in type.GetInterfaces())
			{
				if (!interfaceType.IsGenericType || interfaceType.GetGenericTypeDefinition() != genericType)
					continue;

				typeArguments = interfaceType.GetGenericArguments();
				return true;
			}

			typeArguments = null;
			return false;
		}
	}
}
