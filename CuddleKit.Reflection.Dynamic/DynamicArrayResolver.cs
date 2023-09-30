using System;
using System.Collections.Generic;
using CuddleKit.Reflection.Export;
using CuddleKit.Reflection.Portable;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Dynamic
{
	internal sealed class DynamicArrayResolver : IMemberResolver
	{
		public static readonly IMemberResolver Shared = new DynamicArrayResolver();

		IMemberExporter IMemberResolver.ResolveExporter(Type type)
		{
			var exporterType = GetExporterType(type);
			return exporterType != null
				? (IMemberExporter) Activator.CreateInstance(exporterType)
				: PortableArrayResolver.Shared.ResolveExporter(type);
		}

		private static Type GetExporterType(Type type)
		{
			if (type.IsArray && type.GetElementType() is {} elementType)
				return typeof(ArrayExporter<>).MakeGenericType(elementType);

			if (type.TryGetGenericInterface(typeof(IReadOnlyList<>), out var typeArguments))
				return typeof(ReadOnlyListExporter<>).MakeGenericType(typeArguments);

			if (type.TryGetGenericInterface(typeof(IEnumerable<>), out typeArguments))
				return typeof(EnumerableExporter<>).MakeGenericType(typeArguments);

			return null;
		}

		private sealed class ArrayExporter<TElement> : GenericArrayExporter<TElement[], TElement>
		{
			protected override void Iterate<TVisitor>(
				TElement[] array,
				ref TVisitor visitor,
				ref Document document)
			{
				for (int i = 0, length = array.Length; i < length; ++i)
					visitor.Visit(array[i], ref document);
			}
		}

		private sealed class ReadOnlyListExporter<TElement> :
			GenericArrayExporter<IReadOnlyList<TElement>, TElement>
		{
			protected override void Iterate<TVisitor>(
				IReadOnlyList<TElement> array,
				ref TVisitor visitor,
				ref Document document)
			{
				for (int i = 0, length = array.Count; i < length; ++i)
					visitor.Visit(array[i], ref document);
			}
		}

		private sealed class EnumerableExporter<TElement> :
			GenericArrayExporter<IEnumerable<TElement>, TElement>
		{
			protected override void Iterate<TVisitor>(
				IEnumerable<TElement> array,
				ref TVisitor visitor,
				ref Document document)
			{
				foreach (var element in array)
					visitor.Visit(element, ref document);
			}
		}
	}
}
