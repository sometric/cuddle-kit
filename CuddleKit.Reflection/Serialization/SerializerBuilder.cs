using System.Collections.Generic;
using CuddleKit.Format;
using CuddleKit.Reflection.Naming;

namespace CuddleKit.Reflection.Serialization
{
	public class SerializerBuilder
	{
		private SerializationSettings _settings;

		public static SerializerBuilder Create() =>
			new(SerializationSettings.Default);

		public static SerializerBuilder Create(SerializationSettings settings) =>
			new(settings);

		private SerializerBuilder(in SerializationSettings settings) =>
			_settings = settings;

		public SerializerBuilder WithReflectionPolicy(IReflectionPolicy reflectionPolicy)
		{
			_settings.ReflectionPolicy = reflectionPolicy
				?? SerializationSettings.Default.ReflectionPolicy;

			return this;
		}

		public SerializerBuilder WithNamingConvention(INamingConvention namingConvention)
		{
			_settings.NamingConvention = namingConvention
				?? SerializationSettings.Default.NamingConvention;

			return this;
		}

		public SerializerBuilder WithCustomFormatter(IFormatter formatter)
		{
			_settings.Formatters ??= new List<IFormatter>();
			_settings.Formatters.Add(formatter);
			return this;
		}

		public SerializerBuilder WithCustomResolver(IMemberResolver resolver)
		{
			_settings.CustomResolvers ??= new List<IMemberResolver>();
			_settings.CustomResolvers.Add(resolver);
			return this;
		}

		public SerializerBuilder TrimMemberPrefixes(params string[] prefixes)
		{
			_settings.MemberPrefixes = prefixes ?? System.Array.Empty<string>();
			return this;
		}

		public SerializerBuilder TrimDefaultMemberPrefixes()
		{
			_settings.MemberPrefixes = SerializationSettings.Default.MemberPrefixes;
			return this;
		}

		public SerializerBuilder WithMemberAccessMask(MemberAccess accessMask)
		{
			_settings.MemberAccessMask = accessMask;
			return this;
		}

		public SerializerBuilder WithMemberKindMask(MemberKind kindMask)
		{
			_settings.MemberKindMask = kindMask;
			return this;
		}

		public Serializer Build() =>
			new(_settings);
	}
}
