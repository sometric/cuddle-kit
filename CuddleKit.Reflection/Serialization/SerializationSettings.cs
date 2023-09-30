using System.Collections.Generic;
using CuddleKit.Format;
using CuddleKit.Reflection.Naming;
using CuddleKit.Reflection.Portable;

namespace CuddleKit.Reflection.Serialization
{
	public struct SerializationSettings
	{
		public static readonly SerializationSettings Default = new()
		{
			Formatters = new List<IFormatter>(FormatterRegistry.DefaultFormatters),
			ReflectionPolicy = PortableReflectionPolicy.Shared,
			NamingConvention = LispCaseNamingConvention.Shared,
			MemberPrefixes = new []{ "_", "m_" },
			MemberKindMask = MemberKind.Field | MemberKind.Property,
			MemberAccessMask = MemberAccess.Public
		};

		public List<IFormatter> Formatters;
		public IReflectionPolicy ReflectionPolicy;
		public List<IMemberResolver> CustomResolvers;
		public INamingConvention NamingConvention;
		public string[] MemberPrefixes;
		public MemberKind MemberKindMask;
		public MemberAccess MemberAccessMask;
	}
}
