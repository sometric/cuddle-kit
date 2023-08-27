using System.Runtime.CompilerServices;

namespace CuddleKit.Serialization
{
	using Detail;

	public readonly struct NodeReference
	{
		internal readonly SafeIndex Index;

		internal NodeReference(int index) =>
			Index = index;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Index.IsValid;
		}
	}
}
