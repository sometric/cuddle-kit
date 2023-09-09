using System;
using System.Buffers;
using CuddleKit.Serialization;

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

		bool IFormatter.Import<TProxy>(in Document document, ValueReference reference, TProxy proxy)
		{
			if (!TryImport(document.GetData(reference), out var value))
				return false;

			document.TryGetAnnotation(reference, out var annotation);
			proxy.Import(value, annotation);

			return true;
		}

		ValueReference IFormatter.Export<TProxy>(ref Document document, in TProxy proxy)
		{
			var buffer = ArrayPool<char>.Shared.Rent(_bufferLength);
			try
			{
				Span<char> data = buffer;
				if (!TryExport(proxy.Export<TValue>(), data, out var length))
					return default;

				var annotation = _specification.HasFlag(FormatterFlags.ForceExportAnnotations)
					? _specification.Annotation
					: proxy.Annotation;

				return document.AddValue(_specification.DataType, data.Slice(0, length), annotation);
			}
			finally
			{
				ArrayPool<char>.Shared.Return(buffer);
			}
		}

		protected abstract bool TryImport(ReadOnlySpan<char> source, out TValue value);
		protected abstract bool TryExport(TValue value, Span<char> destination, out int exportedLength);
	}
}
