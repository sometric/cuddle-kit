using System.Reflection;
using CuddleKit.Reflection.Description;

namespace CuddleKit.Reflection.Portable
{
	internal sealed class PortableFieldDescriptor : FieldDescriptor
	{
		private readonly FieldInfo _fieldInfo;

		public PortableFieldDescriptor(FieldInfo fieldInfo) : base(fieldInfo) =>
			_fieldInfo = fieldInfo;

		public override TValue GetValue<TInstance, TValue>(in TInstance instance) =>
			(TValue) _fieldInfo.GetValue(instance);

		public override void SetValue<TInstance, TValue>(ref TInstance instance, in TValue value) =>
			_fieldInfo.SetValue(instance, value);
	}
}
