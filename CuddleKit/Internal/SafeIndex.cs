using System.Runtime.CompilerServices;

namespace CuddleKit.Internal
{
	internal readonly struct SafeIndex
	{
		private readonly int _value;

		private SafeIndex(int index) =>
			_value = index + 1;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SafeIndex(int index) =>
			new(index);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(SafeIndex index) =>
			index._value - 1;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _value > 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValidForRange(int range) =>
			(_value > 0) & (_value <= range);
	}
}
