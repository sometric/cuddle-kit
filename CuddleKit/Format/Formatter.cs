using System;
using CuddleKit.Serialization;
using CuddleKit.Utility;

namespace CuddleKit.Format
{
	public abstract class Formatter<TValue> : IFormatter
	{
		private readonly FormatterSpecification _specification;
		private readonly int _bufferLength;

		ref readonly FormatterSpecification IFormatter.Specification =>
			ref _specification;

		protected Formatter(DataType dataType, string annotation, FormatterFlags flags, int bufferLength)
		{
			_specification = new FormatterSpecification(typeof(TValue), dataType, annotation, flags);
			_bufferLength = bufferLength;
		}

		public ValueReference Export(TValue value, ReadOnlySpan<char> annotation, ref Document document)
		{
			using var allocation = SpanAllocation<char>.Retain(_bufferLength, out var data);

			if (!TryExport(value, data, out var length))
				return default;

			annotation = annotation.IsEmpty & _specification.HasFlag(FormatterFlags.ForceExportAnnotations)
				? _specification.Annotation
				: annotation;

			return document.AddValue(_specification.DataType, data.Slice(0, length), annotation);
		}

		bool IFormatter.Import<TProxy>(in Document document, ValueReference reference, ref TProxy importProxy)
		{
			if (!TryImport(document.GetData(reference), out var value))
				return false;

			document.TryGetAnnotation(reference, out var annotation);
			importProxy.Import(value, annotation);

			return true;
		}

		ValueReference IFormatter.Export<TProxy>(ref TProxy proxy, ref Document document) =>
			Export(proxy.Export<TValue>(), proxy.Annotation, ref document);

		protected abstract bool TryImport(ReadOnlySpan<char> source, out TValue value);
		protected abstract bool TryExport(TValue value, Span<char> destination, out int exportedLength);
	}
}
