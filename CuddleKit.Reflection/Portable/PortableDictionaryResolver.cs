using System.Collections;
using CuddleKit.Reflection.Export;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Portable
{
	public sealed class PortableDictionaryResolver : IMemberResolver
	{
		public static readonly IMemberResolver Shared = new PortableDictionaryResolver();

		private static readonly DefaultDictionaryExporter SharedExporter = new();

		IMemberExporter IMemberResolver.ResolveExporter(System.Type type) =>
			typeof(IDictionary).IsAssignableFrom(type) ? SharedExporter : null;

		private sealed class DefaultDictionaryExporter : GenericDictionaryExporter<IDictionary, object, object>
		{
			protected override void Iterate<TVisitor>(
				IDictionary dictionary,
				ref TVisitor visitor,
				ref Document document)
			{
				foreach (var key in dictionary.Keys)
					visitor.Visit(key, dictionary[key], ref document);
			}
		}
	}
}
