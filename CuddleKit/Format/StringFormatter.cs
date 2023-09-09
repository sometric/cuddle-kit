using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public sealed class StringFormatter : IFormatter
	{
		private readonly FormatterSpecification _specification;

		public static readonly StringFormatter Default =
			new(FormatterFlags.FallbackSystemType | FormatterFlags.FallbackDocumentType);

		ref readonly FormatterSpecification IFormatter.Specification =>
			ref _specification;

		public StringFormatter(FormatterFlags flags) =>
			_specification =
				new FormatterSpecification(typeof(string), DataType.String, string.Empty, flags);

		bool IFormatter.Import<TProxy>(in Document document, ValueReference reference, TProxy proxy)
		{
			var data = document.GetData(reference);
			document.TryGetAnnotation(reference, out var annotation);
			proxy.Import(data.ToString(), annotation);

			return true;
		}

		ValueReference IFormatter.Export<TProxy>(ref Document document, in TProxy proxy)
		{
			var annotation = _specification.HasFlag(FormatterFlags.ForceExportAnnotations)
				? _specification.Annotation
				: proxy.Annotation;

			return document.AddValue(_specification.DataType, proxy.Export<string>(), annotation);
		}
	}
}
