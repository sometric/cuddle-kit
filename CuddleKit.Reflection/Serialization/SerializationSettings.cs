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
			Formatters = FormatterRegistry.DefaultFormatters,
			ReflectionPolicy = PotableReflectionPolicy.Shared,
			NamingConvention = LispCaseNamingConvention.Shared,
			MemberPrefixes = new[] { "_", "m_" },
			MemberKindMask = MemberKind.Field | MemberKind.Property,
			MemberAccessMask = MemberAccess.Public
		};

		public IReadOnlyList<IFormatter> Formatters;
		public IReflectionPolicy ReflectionPolicy;
		public INamingConvention NamingConvention;
		public IReadOnlyList<string> MemberPrefixes;
		public MemberKind MemberKindMask;
		public MemberAccess MemberAccessMask;
	}
}
