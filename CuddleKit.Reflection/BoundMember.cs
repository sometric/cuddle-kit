using System;
using System.Collections.Generic;
using CuddleKit.Format;

namespace CuddleKit.Reflection
{
	using Description;

	public readonly struct BoundMember<TInstance> : IFormatterExportProxy
	{
		private readonly TInstance _instance;
		private readonly MemberDescriptor _descriptor;
		private readonly string _annotation;

		internal BoundMember(in TInstance instance, MemberDescriptor descriptor, string annotation)
		{
			_instance = instance;
			_descriptor = descriptor;
			_annotation = annotation;
		}

		public Type MemberType =>
			_descriptor.Type;

		ReadOnlySpan<char> IFormatterExportProxy.Annotation =>
			_annotation;

		public TValue Export<TValue>() =>
			_descriptor.GetValue<TInstance, TValue>(_instance);

		internal MemberCategory ResolveCategory(out Type[] typeArguments)
		{
			var dictionaryInterfaceType = typeof(IDictionary<,>);
			var arrayInterfaceType = typeof(IEnumerable<>);
			var type = _descriptor.Type;

			if (TryGetGenericInterface(type, dictionaryInterfaceType, out typeArguments))
				return MemberCategory.Dictionary;

			return TryGetGenericInterface(type, arrayInterfaceType, out typeArguments)
				? MemberCategory.Array
				: MemberCategory.Object;
		}

		private static bool TryGetGenericInterface(Type type, Type genericInterfaceType, out Type[] typeArguments)
		{
			if (type.IsGenericType && type.GetGenericTypeDefinition() == genericInterfaceType)
			{
				typeArguments = type.GetGenericArguments();
				return true;
			}

			foreach (var interfaceType in type.GetInterfaces())
			{
				if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericInterfaceType)
				{
					typeArguments = interfaceType.GetGenericArguments();
					return true;
				}
			}

			typeArguments = null;
			return false;
		}
	}
}
