using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CuddleKit.Reflection.Dynamic
{
	using Description;

	internal sealed class DynamicPropertyDescriptor : PropertyDescriptor
	{
		private readonly IInstanceAccessor _accessor;
		private readonly Delegate _getterDelegate;

		public DynamicPropertyDescriptor(PropertyInfo propertyInfo) : base(propertyInfo)
		{
			var instanceType = propertyInfo.DeclaringType
				?? throw new InvalidOperationException("Unable to handle property");

			_accessor = instanceType.IsValueType
				? (IInstanceAccessor) Activator.CreateInstance(typeof(ValueInstanceAccessor<>).MakeGenericType(instanceType))
					?? throw new InvalidOperationException("Unable to create a property accessor")
				: ReferenceInstanceAccessor.Shared;

			_getterDelegate = propertyInfo.GetMethod?.CreateDelegate(
				_accessor.GetGetterDelegateType(instanceType, propertyInfo.PropertyType));
		}

		public override TValue GetValue<TInstance, TValue>(in TInstance instance) =>
			_accessor.GetValue<TInstance, TValue>(instance, _getterDelegate);

		private interface IInstanceAccessor
		{
			Type GetGetterDelegateType(Type instanceType, Type valueType);
			TValue GetValue<TInstance, TValue>(in TInstance instance, Delegate getDelegate);
		}

		private sealed class ValueInstanceAccessor<TDeclaredInstance> : IInstanceAccessor
			where TDeclaredInstance : struct
		{
			private delegate TValue GetDelegate<TInstance, out TValue>(ref TInstance instance);

			Type IInstanceAccessor.GetGetterDelegateType(Type instanceType, Type valueType) =>
				typeof(GetDelegate<,>).MakeGenericType(typeof(TDeclaredInstance), instanceType, valueType);

			TValue IInstanceAccessor.GetValue<TInstance, TValue>(in TInstance instance, Delegate getter) =>
				typeof(TInstance).IsValueType
					? ((GetDelegate<TInstance, TValue>) getter)(ref Unsafe.AsRef(instance))
					: ((GetDelegate<TDeclaredInstance, TValue>) getter)(ref Unsafe.Unbox<TDeclaredInstance>(instance));
		}

		private sealed class ReferenceInstanceAccessor : IInstanceAccessor
		{
			private delegate TValue GetDelegate<in TInstance, out TValue>(TInstance instance);

			public static readonly ReferenceInstanceAccessor Shared = new();

			Type IInstanceAccessor.GetGetterDelegateType(Type instanceType, Type valueType) =>
				typeof(GetDelegate<,>).MakeGenericType(instanceType, valueType);

			TValue IInstanceAccessor.GetValue<TInstance, TValue>(in TInstance instance, Delegate getter) =>
				((GetDelegate<TInstance, TValue>) getter)(instance);
		}
	}
}
