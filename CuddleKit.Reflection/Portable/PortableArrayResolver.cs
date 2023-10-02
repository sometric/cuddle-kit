using System.Collections;
using CuddleKit.Reflection.Export;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Portable
{
	public sealed class PortableArrayResolver : IMemberResolver
	{
		public static readonly IMemberResolver Shared = new PortableArrayResolver();

		private static readonly Exporter SharedExporter = new();

		IMemberExporter IMemberResolver.ResolveExporter(System.Type type) =>
			typeof(IEnumerable).IsAssignableFrom(type) ? SharedExporter : null;

		private sealed class Exporter : GenericArrayExporter<IEnumerable, object>
		{
			protected override void Iterate<TVisitor>(
				IEnumerable array,
				ref TVisitor visitor,
				ref Document document)
			{
				foreach (var element in array)
					visitor.Visit(element, ref document);
			}
		}
	}
}
