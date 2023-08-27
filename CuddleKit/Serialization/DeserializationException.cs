namespace CuddleKit.Serialization
{
	public class DeserializationException : System.FormatException
	{
		public DeserializationException(int line, int column, string message, System.Exception innerException = null)
			: base($"[{line + 1}:{column + 1}] {message}", innerException) {}
	}
}
