using System.Runtime.CompilerServices;

namespace CuddleKit.Serialization
{
	using Detail;

	public readonly struct ValueReference
	{
		internal readonly SafeIndex Index;

		internal ValueReference(int index) =>
			Index = index;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Index.IsValid;
		}
	}
}
