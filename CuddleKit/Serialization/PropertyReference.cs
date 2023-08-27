using System.Runtime.CompilerServices;

namespace CuddleKit.Serialization
{
	public readonly struct PropertyReference
	{
		internal readonly TokenReference Key;
		public readonly ValueReference Value;

		internal PropertyReference(TokenReference key, ValueReference value) =>
			(Key, Value) = (key, value);

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Key.IsValid & Value.IsValid;
		}
	}
}
