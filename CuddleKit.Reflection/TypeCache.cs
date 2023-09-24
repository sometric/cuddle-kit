using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CuddleKit.Reflection
{
	using Description;

	internal readonly struct TypeCache
	{
		private readonly Dictionary<Type, TypeDescriptor> _descriptors;
		private readonly IReflectionPolicy _policy;
		private readonly MemberAccess _memberAccessMask;

		public TypeCache(IReflectionPolicy reflectionPolicy, MemberAccess memberAccessMask)
		{
			_descriptors = new Dictionary<Type, TypeDescriptor>();
			_policy = reflectionPolicy;
			_memberAccessMask = memberAccessMask;
		}

		public TypeDescriptor GetTypeDescriptor(Type type, MemberAccess accessFilter, MemberKind kindFilter)
		{
			lock (_descriptors)
			{
				if (_descriptors.TryGetValue(type, out var descriptor))
					return descriptor;

				// todo: get filter overrides form type annotation or explicit overrides list
				descriptor = Describe(type, accessFilter & _memberAccessMask, kindFilter);
				_descriptors.Add(type, descriptor);

				return descriptor;
			}
		}

		private TypeDescriptor Describe(Type type, MemberAccess accessFilter, MemberKind kindFilter)
		{
			var bindingFlags = BindingFlags.Instance | BindingFlags.FlattenHierarchy;

			var considerPublic = (accessFilter & MemberAccess.Public) != 0;
			var considerNonPublic = (accessFilter & MemberAccess.NonPublic) != 0;
			var considerReadOnly = (accessFilter & MemberAccess.ReadOnly) != 0;
			var considerWriteOnly = (accessFilter & MemberAccess.WriteOnly) != 0;

			if (considerPublic)
				bindingFlags |= BindingFlags.Public;

			if (considerNonPublic)
				bindingFlags |= BindingFlags.NonPublic;

			var fields = (kindFilter & MemberKind.Field) != 0
				? type.GetFields(bindingFlags)
				: Array.Empty<FieldInfo>();

			var properties = (kindFilter & MemberKind.Property) != 0
				? type.GetProperties(bindingFlags)
				: Array.Empty<PropertyInfo>();

			var descriptors = new MemberDescriptor[fields.Length + properties.Length];
			var descriptorsCount = 0;

			foreach (var fieldInfo in fields)
			{
				var skipField =
					fieldInfo.IsNotSerialized ||
					fieldInfo.IsInitOnly & !considerReadOnly ||
					fieldInfo.GetCustomAttribute<CompilerGeneratedAttribute>() != null;

				if (skipField)
					continue;

				descriptors[descriptorsCount++] = _policy.Describe(fieldInfo);
			}

			foreach (var propertyInfo in properties)
			{
				var canRead = propertyInfo.GetMethod is { IsPublic: var isGetterPublic } &&
					(isGetterPublic == considerPublic) | (!isGetterPublic == considerNonPublic);

				var canWrite = propertyInfo.SetMethod is { IsPublic: var isSetterPublic } &&
					(isSetterPublic == considerPublic) | (!isSetterPublic == considerNonPublic);

				if (canRead & !canWrite & !considerReadOnly)
					continue;

				if (canWrite & !canRead & !considerWriteOnly)
					continue;

				descriptors[descriptorsCount++] = _policy.Describe(propertyInfo);
			}

			return new TypeDescriptor(
				type,
				new ArraySegment<MemberDescriptor>(descriptors, 0, descriptorsCount));
		}
	}
}
