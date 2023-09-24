using System;

namespace CuddleKit.Reflection.Description
{
	internal sealed class TypeDescriptor
	{
		public readonly Type Type;
		public readonly ArraySegment<MemberDescriptor> Members;

		public TypeDescriptor(Type type, ArraySegment<MemberDescriptor> members)
		{
			Type = type;
			Members = members;
		}
	}
}
