using System.Reflection;

namespace CuddleKit.Reflection.Description
{
	public abstract class PropertyDescriptor : MemberDescriptor
	{
		protected PropertyDescriptor(PropertyInfo info) : base(info.PropertyType, info.Name) {}
	}
}
