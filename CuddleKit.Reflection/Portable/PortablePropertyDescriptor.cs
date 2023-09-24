using System.Reflection;
using CuddleKit.Reflection.Description;

namespace CuddleKit.Reflection.Portable
{
	internal sealed class PortablePropertyDescriptor : PropertyDescriptor
	{
		private readonly PropertyInfo _propertyInfo;

		public PortablePropertyDescriptor(PropertyInfo propertyInfo) : base(propertyInfo) =>
			_propertyInfo = propertyInfo;

		public override TValue GetValue<TInstance, TValue>(in TInstance instance) =>
			(TValue) _propertyInfo.GetValue(instance);
	}
}
