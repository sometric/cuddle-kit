using System;
using CuddleKit.Format;
using CuddleKit.Reflection.Description;
using CuddleKit.Reflection.Utility;
using CuddleKit.Serialization;
using CuddleKit.Utility;

namespace CuddleKit.Reflection.Serialization
{
	public class Deserializer
	{
		private const MemberAccess MemberAccessMask =
			MemberAccess.Public | MemberAccess.NonPublic | MemberAccess.WriteOnly;

		private readonly SerializationSettings _settings;
		private readonly TypeCache _typeCache;
		private FormatterRegistry _registry;

		public Deserializer(in SerializationSettings settings)
		{
			_settings = settings;
			_typeCache = new TypeCache(settings.ReflectionPolicy, settings.CustomResolvers, MemberAccessMask);
			_registry = new FormatterRegistry(settings.Formatters);
		}

		public void Deserialize<T>(in Document document, NodeReference instanceNode, ref T instance)
		{

		}

		private void ReadInstance<TInstance>(
			in Document document,
			NodeReference instanceNode,
			ref TInstance instance)
		{
			var typeDescriptor = _typeCache.GetTypeDescriptor(
				instance.GetType(),
				_settings.MemberAccessMask,
				_settings.MemberKindMask);

			using var memberMap = new Map<MemberDescriptor>();

			for (int i = 0, length = typeDescriptor.Members.Count; i < length; ++i)
			{
				var memberDescriptor = typeDescriptor.Members[i];
				var memberName = memberDescriptor.Name
					.AsSpan()
					.SkipPrefixes(_settings.MemberPrefixes);

				using var nameAllocation = _settings.NamingConvention.Apply(memberName, out memberName);
				memberMap.Insert(memberName) = memberDescriptor;
			}

			var children = document.GetChildren(instanceNode);
			for (int i = 0, length = children.Length; i < length; ++i)
			{
				var memberNode = children[i];
				var memberName = document.GetName(memberNode);

				var memberDescriptor = memberMap.Lookup(memberName, null);
				if (memberDescriptor == null)
					continue;

				var formatter = _registry.Lookup(memberDescriptor.Type, default);


				//memberDescriptor.SetValue(ref instance, ???);
			}
		}
	}
}
