namespace CuddleKit.ObjectModel
{
	public interface IDocumentObjectModelFactory
	{
		DocumentObjectModel Retain();
		void Release(DocumentObjectModel document);
	}
}
