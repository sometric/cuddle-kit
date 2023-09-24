using CuddleKit.Format;
using CuddleKit.Reflection.Naming;

namespace CuddleKit.Reflection.Serialization
{
	public class SerializerBuilder
	{
		private SerializationSettings _settings = SerializationSettings.Default;

		public static SerializerBuilder Create() =>
			new SerializerBuilder();

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

		public SerializerBuilder WithFormatter(IFormatter formatter)
		{

			return this;
		}

		public SerializerBuilder TrimMemberPrefixes(params string[] prefixes)
		{
			_settings.MemberPrefixes = prefixes;
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
