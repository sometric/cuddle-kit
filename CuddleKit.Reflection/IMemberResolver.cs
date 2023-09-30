using CuddleKit.Reflection.Export;

namespace CuddleKit.Reflection
{
	public interface IMemberResolver
	{
		IMemberExporter ResolveExporter(System.Type type);
	}
}
