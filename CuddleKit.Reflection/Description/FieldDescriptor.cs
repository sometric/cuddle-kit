using System.Reflection;

namespace CuddleKit.Reflection.Description
{
	public abstract class FieldDescriptor : MemberDescriptor
	{
		protected FieldDescriptor(FieldInfo info) : base(info.FieldType, info.Name) {}
	}
}
