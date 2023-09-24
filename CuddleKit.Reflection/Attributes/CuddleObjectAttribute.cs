using System;
using CuddleKit.Reflection.Naming;

namespace CuddleKit.Reflection.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class CuddleObjectAttribute : Attribute
	{
		public INamingConvention NamingConvention { get; set; }

		public CuddleObjectAttribute(INamingConvention namingConvention)
		{

		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class CuddleMember : Attribute
	{

	}
}
