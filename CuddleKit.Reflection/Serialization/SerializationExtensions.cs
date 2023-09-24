using System;
using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Serialization
{
	public static class SerializationExtensions
	{
		public static Document Serialize<T>(this Serializer serializer, T instance, ReadOnlySpan<char> name)
		{
			var document = new Document();
			try
			{
				serializer.Serialize(instance, name, ref document);
			}
			catch
			{
				document.Dispose();
				throw;
			}

			return document;
		}
	}
}
