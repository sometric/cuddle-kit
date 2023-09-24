using System.Runtime.CompilerServices;

namespace CuddleKit.Serialization
{
	using Internal;

	public readonly struct TokenReference
	{
		internal readonly SafeIndex Index;

		internal TokenReference(int index) =>
			Index = index;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Index.IsValid;
		}
	}
}
