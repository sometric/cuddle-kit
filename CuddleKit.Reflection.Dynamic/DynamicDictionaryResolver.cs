using System;
using System.Collections.Generic;
using CuddleKit.Reflection.Export;
using CuddleKit.Reflection.Portable;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Dynamic
{
	internal sealed class DynamicDictionaryResolver : IMemberResolver
	{
		public static readonly IMemberResolver Shared = new DynamicDictionaryResolver();

		IMemberExporter IMemberResolver.ResolveExporter(Type type)
		{
			var exporterType = GetExporterType(type);
			return exporterType != null
				? (IMemberExporter) Activator.CreateInstance(exporterType)
				: PortableDictionaryResolver.Shared.ResolveExporter(type);
		}

		private static Type GetExporterType(Type type)
		{
			if (type.TryGetGenericInterface(typeof(Dictionary<,>), out var typeArguments))
				return typeof(DictionaryExporter<,>).MakeGenericType(typeArguments);

			if (type.TryGetGenericInterface(typeof(IReadOnlyDictionary<,>), out typeArguments))
				return typeof(ReadonlyDictionaryExporter<,>).MakeGenericType(typeArguments);

			if (type.TryGetGenericInterface(typeof(IDictionary<,>), out typeArguments))
				return typeof(EnumerableDictionaryExporter<,>).MakeGenericType(typeArguments);

			return null;
		}

		private sealed class DictionaryExporter<TKey, TValue> :
			GenericDictionaryExporter<Dictionary<TKey, TValue>, TKey, TValue>
		{
			protected override void Iterate<TVisitor>(
				Dictionary<TKey, TValue> dictionary,
				ref TVisitor visitor,
				ref Document document)
			{
				foreach (var (key, value) in dictionary)
					visitor.Visit(key, value, ref document);
			}
		}

		private sealed class ReadonlyDictionaryExporter<TKey, TValue> :
			GenericDictionaryExporter<IReadOnlyDictionary<TKey, TValue>, TKey, TValue>
		{
			protected override void Iterate<TVisitor>(
				IReadOnlyDictionary<TKey, TValue> dictionary,
				ref TVisitor visitor,
				ref Document document)
			{
				foreach (var (key, value) in dictionary)
					visitor.Visit(key, value, ref document);
			}
		}

		private sealed class EnumerableDictionaryExporter<TKey, TValue> :
			GenericDictionaryExporter<IDictionary<TKey, TValue>, TKey, TValue>
		{
			protected override void Iterate<TVisitor>(
				IDictionary<TKey, TValue> dictionary,
				ref TVisitor visitor,
				ref Document document)
			{
				foreach (var (key, value) in dictionary)
					visitor.Visit(key, value, ref document);
			}
		}
	}
}
