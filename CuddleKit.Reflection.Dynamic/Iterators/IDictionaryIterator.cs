using System.Collections;
using System.Collections.Generic;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Dynamic.Iterators
{
	internal interface IDictionaryIterator
	{
		void Iterate<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document)
			where TVisitor : struct, IKeyValueVisitor;
	}

	internal sealed class DictionaryIterator<TKey, TValue> : IDictionaryIterator
	{
		void IDictionaryIterator.Iterate<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document)
		{
			var dictionary = member.Export<Dictionary<TKey, TValue>>();
			foreach (var (key, value) in dictionary)
				visitor.Visit(key, value, ref document);
		}
	}

	internal sealed class ReadonlyDictionaryIterator<TKey, TValue> : IDictionaryIterator
	{
		void IDictionaryIterator.Iterate<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document)
		{
			var dictionary = member.Export<IReadOnlyDictionary<TKey, TValue>>();
			foreach (var (key, value) in dictionary)
				visitor.Visit(key, value, ref document);
		}
	}

	internal sealed class EnumerableDictionaryIterator<TKey, TValue> : IDictionaryIterator
	{
		void IDictionaryIterator.Iterate<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document)
		{
			var dictionary = member.Export<IDictionary<TKey, TValue>>();
			foreach (var (key, value) in dictionary)
				visitor.Visit(key, value, ref document);
		}
	}

	internal sealed class FallbackDictionaryIterator : IDictionaryIterator
	{
		public static readonly FallbackDictionaryIterator Shared = new();

		void IDictionaryIterator.Iterate<TInstance, TVisitor>(
			in BoundMember<TInstance> member,
			ref TVisitor visitor,
			ref Document document)
		{
			var dictionary = member.Export<IDictionary>();
			foreach (var key in dictionary.Keys)
				visitor.Visit(key, dictionary[key], ref document);
		}
	}
}
