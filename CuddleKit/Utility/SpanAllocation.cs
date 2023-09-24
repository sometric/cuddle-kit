using System;
using System.Buffers;

namespace CuddleKit.Utility
{
	public ref struct SpanAllocation<T>
	{
		private readonly ArrayPool<T> _pool;
		private T[] _array;

		private SpanAllocation(ArrayPool<T> pool, int minimumLength)
		{
			_pool = pool;
			_array = pool.Rent(minimumLength);
		}

		public static SpanAllocation<T> Retain(ArrayPool<T> pool, int minimumLength, out Span<T> span)
		{
			var allocation = new SpanAllocation<T>(ArrayPool<T>.Shared, minimumLength);
			span = allocation._array.AsSpan(0, minimumLength);
			return allocation;
		}

		public static SpanAllocation<T> Retain(int minimumLength, out Span<T> span) =>
			Retain(ArrayPool<T>.Shared, minimumLength, out span);

		public void Dispose()
		{
			var array = _array;
			_array = null;

			if (array != null)
				_pool.Return(array);
		}
	}
}
