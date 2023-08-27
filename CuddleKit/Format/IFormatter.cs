using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public interface IFormatter
	{
		ref readonly FormatterSpecification Specification { get; }

		bool Import<TProxy>(in Document document, ValueReference reference, TProxy proxy)
			where TProxy : struct, IFormatterImportProxy;

		ValueReference Export<TProxy>(ref Document document, in TProxy valueGetter)
			where TProxy : struct, IFormatterExportProxy;
	}
}
