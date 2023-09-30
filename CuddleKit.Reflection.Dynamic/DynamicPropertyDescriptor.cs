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
		private readonly Delegate _setterDelegate;

		public DynamicPropertyDescriptor(PropertyInfo propertyInfo) : base(propertyInfo)
		{
			var instanceType = propertyInfo.DeclaringType
				?? throw new InvalidOperationException("Unable to handle property");

			_accessor = instanceType.IsValueType
				? (IInstanceAccessor) Activator.CreateInstance(typeof(ValueInstanceAccessor<>).MakeGenericType(instanceType))
					?? throw new InvalidOperationException("Unable to create a property accessor")
				: ReferenceInstanceAccessor.Shared;

			var valueType = propertyInfo.PropertyType;

			_getterDelegate = propertyInfo.GetMethod?
				.CreateDelegate(_accessor.GetGetterDelegateType(instanceType, valueType));

			_setterDelegate = propertyInfo.SetMethod?
				.CreateDelegate(_accessor.GetSetterDelegateType(instanceType, valueType));
		}

		public override TValue GetValue<TInstance, TValue>(in TInstance instance) =>
			_accessor.GetValue<TInstance, TValue>(instance, _getterDelegate);

		public override void SetValue<TInstance, TValue>(ref TInstance instance, in TValue value) =>
			_accessor.SetValue(ref instance, value, _setterDelegate);

		private interface IInstanceAccessor
		{
			Type GetGetterDelegateType(Type instanceType, Type valueType);
			Type GetSetterDelegateType(Type instanceType, Type valueType);

			TValue GetValue<TInstance, TValue>(in TInstance instance, Delegate getDelegate);
			void SetValue<TInstance, TValue>(ref TInstance instance, in TValue value, Delegate setDelegate);
		}

		private sealed class ValueInstanceAccessor<TDeclaredInstance> : IInstanceAccessor
			where TDeclaredInstance : struct
		{
			private delegate TValue Get<TInstance, out TValue>(ref TInstance instance);
			private delegate void Set<TInstance, TValue>(ref TInstance instance, TValue value);

			Type IInstanceAccessor.GetGetterDelegateType(Type instanceType, Type valueType) =>
				typeof(Get<,>).MakeGenericType(typeof(TDeclaredInstance), instanceType, valueType);

			Type IInstanceAccessor.GetSetterDelegateType(Type instanceType, Type valueType) =>
				typeof(Set<,>).MakeGenericType(typeof(TDeclaredInstance), instanceType, valueType);

			TValue IInstanceAccessor.GetValue<TInstance, TValue>(in TInstance instance, Delegate getter)
			{
				if (!typeof(TInstance).IsValueType)
				{
					var boxedValueGetter = (Get<TDeclaredInstance, TValue>) getter;
					return boxedValueGetter(ref Unsafe.Unbox<TDeclaredInstance>(instance));
				}

				var valueGetter = (Get<TInstance, TValue>) getter;
				return valueGetter(ref Unsafe.AsRef(instance));
			}

			void IInstanceAccessor.SetValue<TInstance, TValue>(ref TInstance instance, in TValue value, Delegate setter)
			{
				if (!typeof(TInstance).IsValueType)
				{
					var boxedValueSetter = (Set<TDeclaredInstance, TValue>) setter;
					boxedValueSetter(ref Unsafe.Unbox<TDeclaredInstance>(instance), value);
				}

				var valueSetter = (Set<TInstance, TValue>) setter;
				valueSetter(ref instance, value);
			}
		}

		private sealed class ReferenceInstanceAccessor : IInstanceAccessor
		{
			private delegate TValue Get<in TInstance, out TValue>(TInstance instance);
			private delegate void Set<in TInstance, TValue>(TInstance instance, TValue value);

			public static readonly ReferenceInstanceAccessor Shared = new();

			Type IInstanceAccessor.GetGetterDelegateType(Type instanceType, Type valueType) =>
				typeof(Get<,>).MakeGenericType(instanceType, valueType);

			Type IInstanceAccessor.GetSetterDelegateType(Type instanceType, Type valueType) =>
				typeof(Set<,>).MakeGenericType(instanceType, valueType);

			TValue IInstanceAccessor.GetValue<TInstance, TValue>(in TInstance instance, Delegate getter) =>
				((Get<TInstance, TValue>) getter)(instance);

			void IInstanceAccessor.SetValue<TInstance, TValue>(ref TInstance instance, in TValue value, Delegate setter) =>
				((Set<TInstance, TValue>) setter)(instance, value);
		}
	}
}
