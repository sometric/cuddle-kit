using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CuddleKit.Reflection.Dynamic
{
	using Description;

	internal sealed class DynamicFieldDescriptor : FieldDescriptor
	{
		private readonly IntPtr _fieldOffset;

		public DynamicFieldDescriptor(FieldInfo fieldInfo) : base(fieldInfo) =>
			_fieldOffset =
				(IntPtr) (Marshal.ReadInt32(fieldInfo.FieldHandle.Value + (4 + IntPtr.Size)) & 0xFFFFFF);

		public override TValue GetValue<TInstance, TValue>(in TInstance instance)
		{
			ref var instancePointer = ref Unsafe.As<TInstance, IntPtr>(ref Unsafe.AsRef(instance));

			if (typeof(TInstance).IsValueType)
				return Unsafe.As<IntPtr, TValue>(ref Unsafe.AddByteOffset(ref instancePointer, _fieldOffset));

			var fieldPointer = Marshal.ReadIntPtr(instancePointer, IntPtr.Size + _fieldOffset.ToInt32());
			return Unsafe.As<IntPtr, TValue>(ref fieldPointer);
		}

		public override void SetValue<TInstance, TValue>(ref TInstance instance, in TValue value)
		{
			ref var instancePointer = ref Unsafe.As<TInstance, IntPtr>(ref instance);

			if (typeof(TInstance).IsValueType)
			{
				Unsafe.As<IntPtr, TValue>(ref Unsafe.AddByteOffset(ref instancePointer, _fieldOffset)) = value;
			}
			else
			{
				var fieldPointer = Marshal.ReadIntPtr(instancePointer, IntPtr.Size + _fieldOffset.ToInt32());
				Unsafe.As<IntPtr, TValue>(ref fieldPointer) = value;
			}
		}
	}
}
