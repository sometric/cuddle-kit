using CuddleKit.Serialization;

namespace CuddleKit.Reflection.Serialization
{
	public class Deserializer
	{
		private const MemberAccess MemberAccessMask =
			MemberAccess.Public | MemberAccess.NonPublic | MemberAccess.WriteOnly;

		private readonly TypeCache _typeCache;

		public Deserializer(IReflectionPolicy reflectionPolicy)
		{
			_typeCache = new TypeCache(reflectionPolicy, MemberAccessMask);
		}

		public void Deserialize<T>(in Document document, ref T instance)
		{

		}

		private void ReadInstance<TInstance>(
			in Document document,
			NodeReference parentReference,
			ref TInstance instance)
		{
			//var typeDescriptor = _typeCache.GetTypeDescriptor(instance.GetType());


		}
	}
}
