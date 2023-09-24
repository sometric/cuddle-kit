using System;
using System.Collections.Generic;
using System.Reflection;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Dynamic
{
	using Description;
	using Iterators;

	public sealed class DynamicReflectionPolicy : IReflectionPolicy
	{
		public static readonly DynamicReflectionPolicy Shared = new();

		private readonly List<ValueTuple<Type, Type>> _arrayGenerics = new()
		{
			(typeof(IEnumerable<>), typeof(EnumerableIterator<>)),
			(typeof(IReadOnlyList<>), typeof(ReadOnlyListIterator<>))
		};

		private readonly List<ValueTuple<Type, Type>> _dictionaryGenerics = new()
		{
			(typeof(IDictionary<,>), typeof(EnumerableDictionaryIterator<,>)),
			(typeof(IReadOnlyDictionary<,>), typeof(ReadonlyDictionaryIterator<,>)),
			(typeof(Dictionary<,>), typeof(DictionaryIterator<,>))
		};

		private readonly Dictionary<Type, IMemberVisitor> _valueVisitors = new();
		private readonly Dictionary<Type, IArrayIterator> _arrayIterators = new();
		private readonly Dictionary<Type, IDictionaryIterator> _dictionaryIterators = new();

		MemberDescriptor IReflectionPolicy.Describe(FieldInfo fieldInfo) =>
			new DynamicFieldDescriptor(fieldInfo);

		MemberDescriptor IReflectionPolicy.Describe(PropertyInfo propertyInfo) =>
			new DynamicPropertyDescriptor(propertyInfo);

		void IReflectionPolicy.VisitObject<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document) =>
			GetMemberVisitor(member.MemberType).Visit(member, ref visitor, ref document);

		void IReflectionPolicy.VisitArray<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document,
			Type[] typeArguments) =>
			GetArrayIterator(member.MemberType, typeArguments).Iterate(member, ref visitor, ref document);

		void IReflectionPolicy.VisitDictionary<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document,
			Type[] typeArguments) =>
			GetDictionaryIterator(member.MemberType, typeArguments).Iterate(member, ref visitor, ref document);

		private IMemberVisitor GetMemberVisitor(Type memberType)
		{
			lock (_valueVisitors)
			{
				if (_valueVisitors.TryGetValue(memberType, out var visitor))
					return visitor;

				var visitorType = typeof(MemberVisitor<>).MakeGenericType(memberType);
				visitor = (IMemberVisitor) Activator.CreateInstance(visitorType);

				_valueVisitors.Add(memberType, visitor);

				return visitor;
			}
		}

		private IArrayIterator GetArrayIterator(Type memberType, Type[] typeArguments)
		{
			if (typeArguments == null)
				return FallbackArrayIterator.Shared;

			lock (_arrayIterators)
			{
				if (_arrayIterators.TryGetValue(memberType, out var cachedIterator))
					return cachedIterator;

				if (typeArguments[0].MakeArrayType().IsAssignableFrom(memberType))
					return CacheIterator(_arrayIterators, memberType, typeof(ArrayIterator<>), typeArguments);

				for (var i = _arrayGenerics.Count - 1; i >= 0; --i)
				{
					var (genericCollectionType, genericIteratorType) = _arrayGenerics[i];
					if (genericCollectionType.MakeGenericType(typeArguments).IsAssignableFrom(memberType))
						return CacheIterator(_arrayIterators, memberType, genericIteratorType, typeArguments);
				}
			}

			return FallbackArrayIterator.Shared;
		}

		private IDictionaryIterator GetDictionaryIterator(Type memberType, Type[] typeArguments)
		{
			if (typeArguments == null)
				return FallbackDictionaryIterator.Shared;

			lock (_dictionaryIterators)
			{
				if (_dictionaryIterators.TryGetValue(memberType, out var cachedIterator))
					return cachedIterator;

				for (var i = _dictionaryGenerics.Count - 1; i >= 0; --i)
				{
					var (genericCollectionType, genericIteratorType) = _dictionaryGenerics[i];
					if (genericCollectionType.MakeGenericType(typeArguments).IsAssignableFrom(memberType))
						return CacheIterator(_dictionaryIterators, memberType, genericIteratorType, typeArguments);
				}
			}

			return FallbackDictionaryIterator.Shared;
		}

		private static TIterator CacheIterator<TIterator>(
			Dictionary<Type, TIterator> cache,
			Type memberType,
			Type genericIteratorType,
			Type[] typeArguments)
		{
			var iteratorType = genericIteratorType.MakeGenericType(typeArguments);
			var iterator = (TIterator) Activator.CreateInstance(iteratorType);

			cache.TryAdd(memberType, iterator);

			return iterator;
		}

		private interface IMemberVisitor
		{
			void Visit<TInstance, TVisitor>(
				in BoundMember<TInstance> member,
				ref TVisitor visitor,
				ref Document document)
				where TVisitor : struct, IValueVisitor;
		}

		private sealed class MemberVisitor<TValue> : IMemberVisitor
		{
			void IMemberVisitor.Visit<TInstance, TVisitor>(
				in BoundMember<TInstance> member,
				ref TVisitor visitor,
				ref Document document) =>
				visitor.Visit(member.Export<TValue>(), ref document);
		}
	}
}
