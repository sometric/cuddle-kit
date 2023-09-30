using CuddleKit.Reflection.Export;

namespace CuddleKit.Reflection.Portable
{
	public sealed class PortableObjectResolver : IMemberResolver
	{
		public static readonly PortableObjectResolver Shared = new();

		private static readonly IMemberExporter SharedExporter =
			new GenericObjectExporter<object>();

		IMemberExporter IMemberResolver.ResolveExporter(System.Type type) =>
			SharedExporter;
	}
}
