using System;

namespace CuddleKit.Reflection.Description
{
	public abstract class MemberDescriptor
	{
		public readonly Type Type;

		public readonly string Name;

		public MemberStyle Style =>
			MemberStyle.NestedNode;

		protected MemberDescriptor(Type type, string name)
		{
			Type = type;
			Name = name;
		}

		public abstract TValue GetValue<TInstance, TValue>(in TInstance instance);
	}
}
