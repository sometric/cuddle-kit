using System.Collections.Generic;
using System.Threading;

namespace CuddleKit.ObjectModel
{
	public sealed class TreadLocalDocumentObjectModelFactory : IDocumentObjectModelFactory
	{
		private static readonly ThreadLocal<TreadLocalDocumentObjectModelFactory> ThreadLocalFactory =
			new(() => new TreadLocalDocumentObjectModelFactory());

		public static TreadLocalDocumentObjectModelFactory Shared =>
			ThreadLocalFactory.Value;

		private readonly Queue<DocumentObjectModel> _pool = new();

		DocumentObjectModel IDocumentObjectModelFactory.Retain() =>
			_pool.TryDequeue(out var documentObject)
				? documentObject
				: new DocumentObjectModel();

		void IDocumentObjectModelFactory.Release(DocumentObjectModel document) =>
			_pool.Enqueue(document);
	}
}
