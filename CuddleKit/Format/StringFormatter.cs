using System;
using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public sealed class StringFormatter : IFormatter
	{
		private readonly FormatterSpecification _specification;

		public static readonly StringFormatter Default =
			new(FormatterFlags.FallbackSystemType | FormatterFlags.FallbackDocumentType);

		public ValueReference Export(string value, ReadOnlySpan<char> annotation, ref Document document)
		{
			annotation = annotation.IsEmpty & _specification.HasFlag(FormatterFlags.ForceExportAnnotations)
				? _specification.Annotation
				: annotation;

			return value is null
				? document.AddValue(DataType.Keyword, "null", annotation)
				: document.AddValue(_specification.DataType, value, annotation);
		}

		public ValueReference Export<TValue>(in TValue value, ReadOnlySpan<char> annotation, ref Document document)
		{
			return value is string stringValue
				? Export(stringValue, annotation, ref document)
				: default;
		}

		ref readonly FormatterSpecification IFormatter.Specification =>
			ref _specification;

		public StringFormatter(FormatterFlags flags) =>
			_specification =
				new FormatterSpecification(typeof(string), DataType.String, string.Empty, flags);

		bool IFormatter.Import<TProxy>(in Document document, ValueReference reference, ref TProxy importProxy)
		{
			var data = document.GetData(reference);
			document.TryGetAnnotation(reference, out var annotation);
			importProxy.Import(data.ToString(), annotation);

			return true;
		}

		ValueReference IFormatter.Export<TProxy>(ref TProxy exportProxy, ref Document document) =>
			Export(exportProxy.Export<string>(), exportProxy.Annotation, ref document);
	}
}
