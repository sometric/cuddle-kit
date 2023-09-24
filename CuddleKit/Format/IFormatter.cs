using CuddleKit.Serialization;

namespace CuddleKit.Format
{
	public interface IFormatter
	{
		ref readonly FormatterSpecification Specification { get; }

		bool Import<TProxy>(in Document document, ValueReference reference, ref TProxy importProxy)
			where TProxy : struct, IFormatterImportProxy;

		ValueReference Export<TProxy>(ref TProxy exportProxy, ref Document document)
			where TProxy : struct, IFormatterExportProxy;
	}

}
