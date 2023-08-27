namespace CuddleKit.Output
{
	public interface IDocumentOutput
	{
		void Write(System.ReadOnlySpan<char> value);
	}
}
