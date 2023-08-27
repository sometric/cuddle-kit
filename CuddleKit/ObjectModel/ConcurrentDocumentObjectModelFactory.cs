using System.Collections.Concurrent;

namespace CuddleKit.ObjectModel
{
	public sealed class ConcurrentDocumentObjectModelFactory : IDocumentObjectModelFactory
	{
		private static readonly ConcurrentQueue<DocumentObjectModel> Pool = new();

		public static readonly ConcurrentDocumentObjectModelFactory Shared = new();

		DocumentObjectModel IDocumentObjectModelFactory.Retain() =>
			Pool.TryDequeue(out var documentObject)
				? documentObject
				: new DocumentObjectModel();

		void IDocumentObjectModelFactory.Release(DocumentObjectModel document) =>
			Pool.Enqueue(document);
	}
}
