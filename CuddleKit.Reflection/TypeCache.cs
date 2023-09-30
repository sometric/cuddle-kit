using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CuddleKit.Reflection.Export;

namespace CuddleKit.Reflection
{
	using Description;

	internal readonly struct TypeCache
	{
		private readonly Dictionary<Type, TypeDescriptor> _descriptors;
		private readonly IReflectionPolicy _policy;
		private readonly MemberAccess _memberAccessMask;
		private readonly List<IMemberResolver> _resolvers;
		private readonly Dictionary<Type, IMemberExporter> _exporters;

		public TypeCache(
			IReflectionPolicy reflectionPolicy,
			IReadOnlyList<IMemberResolver> customResolvers,
			MemberAccess memberAccessMask)
		{
			_descriptors = new Dictionary<Type, TypeDescriptor>();
			_policy = reflectionPolicy;
			_memberAccessMask = memberAccessMask;

			var policyResolvers = reflectionPolicy.GetResolvers() ?? Array.Empty<IMemberResolver>();
			customResolvers ??= Array.Empty<IMemberResolver>();

			_resolvers = new List<IMemberResolver>(policyResolvers.Count + customResolvers.Count);

			for (int i = 0, length = policyResolvers.Count; i < length; ++i)
				_resolvers.Add(policyResolvers[i]);

			for (int i = 0, length = customResolvers.Count; i < length; ++i)
				_resolvers.Add(customResolvers[i]);

			_exporters = new Dictionary<Type, IMemberExporter>();
		}

		public TypeDescriptor GetTypeDescriptor(Type type, MemberAccess accessMask, MemberKind kindMask)
		{
			lock (_descriptors)
			{
				if (_descriptors.TryGetValue(type, out var descriptor))
					return descriptor;

				// todo: get filter overrides form type annotation or explicit overrides list
				descriptor = Describe(type, accessMask & _memberAccessMask, kindMask);
				_descriptors.Add(type, descriptor);

				return descriptor;
			}
		}

		public IMemberExporter GetTypeExporter(Type type)
		{
			lock (_exporters)
			{
				if (_exporters.TryGetValue(type, out var exporter))
					return exporter;

				for (var i = _resolvers.Count - 1; i >= 0; --i)
				{
					exporter = _resolvers[i].ResolveExporter(type);
					if (exporter == null)
						continue;

					_exporters.Add(type, exporter);

					return exporter;
				}
			}

			return null;
		}

		private TypeDescriptor Describe(Type type, MemberAccess accessMask, MemberKind kindMask)
		{
			var bindingFlags = BindingFlags.Instance | BindingFlags.FlattenHierarchy;

			var considerPublic = (accessMask & MemberAccess.Public) != 0;
			var considerNonPublic = (accessMask & MemberAccess.NonPublic) != 0;
			var considerReadOnly = (accessMask & MemberAccess.ReadOnly) != 0;
			var considerWriteOnly = (accessMask & MemberAccess.WriteOnly) != 0;

			if (considerPublic)
				bindingFlags |= BindingFlags.Public;

			if (considerNonPublic)
				bindingFlags |= BindingFlags.NonPublic;

			var fields = (kindMask & MemberKind.Field) != 0
				? type.GetFields(bindingFlags)
				: Array.Empty<FieldInfo>();

			var properties = (kindMask & MemberKind.Property) != 0
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

				var skipProperty =
					canRead & !canWrite & !considerReadOnly ||
					canWrite & !canRead & !considerWriteOnly;

				if (skipProperty)
					continue;

				descriptors[descriptorsCount++] = _policy.Describe(propertyInfo);
			}

			return new TypeDescriptor(
				type,
				new ArraySegment<MemberDescriptor>(descriptors, 0, descriptorsCount));
		}
	}
}
